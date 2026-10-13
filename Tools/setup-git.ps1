# Unity Smart Merge setup (Windows / PowerShell).
# Locates UnityYAMLMerge.exe and configures the git merge driver used by .gitattributes.
# Run once per machine:
#   powershell -ExecutionPolicy Bypass -File Tools/setup-git.ps1
# Options:
#   -UnityYAMLMerge <path>   use a specific UnityYAMLMerge.exe
#   -Local                   write to this repository's local git config instead of global
param(
    [string]$UnityYAMLMerge = "",
    [switch]$Local
)

$ErrorActionPreference = "Stop"

function Find-UnityYAMLMerge {
    if ($UnityYAMLMerge -and (Test-Path $UnityYAMLMerge)) {
        return (Resolve-Path $UnityYAMLMerge).Path
    }

    $hub = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path $hub) {
        $candidate = Get-ChildItem $hub -Directory |
            Sort-Object Name |
            ForEach-Object { Join-Path $_.FullName "Editor\Data\Tools\UnityYAMLMerge.exe" } |
            Where-Object { Test-Path $_ } |
            Select-Object -Last 1
        if ($candidate) { return $candidate }
    }

    return $null
}

$exe = Find-UnityYAMLMerge
if (-not $exe) {
    Write-Host "UnityYAMLMerge.exe was not found. Pass -UnityYAMLMerge '<path>'." -ForegroundColor Red
    exit 1
}

$scope = if ($Local) { "--local" } else { "--global" }
# PowerShell 5.1 does not escape embedded quotes for native commands, which would split the value at the
# first space. Escape them as \" so git stores the driver with the quoted executable path intact.
$driver = ('"' + $exe + '" merge -p %O %B %A %A').Replace('"', '\"')

git config $scope merge.unityyamlmerge.name "Unity SmartMerge"
git config $scope merge.unityyamlmerge.driver $driver
git config $scope merge.unityyamlmerge.recursive binary

Write-Host "Configured Unity Smart Merge ($scope):" -ForegroundColor Green
Write-Host ("  " + $exe)
git config $scope --get merge.unityyamlmerge.driver
