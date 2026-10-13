#!/usr/bin/env bash
# Unity Smart Merge setup (macOS / Linux).
# Locates UnityYAMLMerge and configures the git merge driver used by .gitattributes.
# Run once per machine:
#   Tools/setup-git.sh [path/to/UnityYAMLMerge] [--local]
set -euo pipefail

scope="--global"
exe="${UNITY_YAML_MERGE:-}"
for arg in "$@"; do
    case "$arg" in
        --local) scope="--local" ;;
        *) exe="$arg" ;;
    esac
done

if [ -z "$exe" ]; then
    exe=$(find \
        "/Applications/Unity/Hub/Editor" \
        "/Applications/Unity" \
        "/opt/Unity/Hub/Editor" \
        "$HOME/Unity/Hub/Editor" \
        -name UnityYAMLMerge -type f 2>/dev/null | sort | tail -n 1 || true)
fi

if [ -z "$exe" ] || [ ! -f "$exe" ]; then
    echo "UnityYAMLMerge was not found. Pass its path: Tools/setup-git.sh /path/to/UnityYAMLMerge" >&2
    exit 1
fi

git config "$scope" merge.unityyamlmerge.name "Unity SmartMerge"
git config "$scope" merge.unityyamlmerge.driver "\"$exe\" merge -p %O %B %A %A"
git config "$scope" merge.unityyamlmerge.recursive binary

echo "Configured Unity Smart Merge ($scope):"
echo "  $exe"
git config "$scope" --get merge.unityyamlmerge.driver
