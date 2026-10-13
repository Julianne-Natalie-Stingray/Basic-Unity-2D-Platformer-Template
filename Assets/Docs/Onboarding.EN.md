# Onboarding

> For two audiences: (1) collaborators joining an existing project; (2) people forking the template into a new project. 中文: [Onboarding.md](Onboarding.md)
> Related docs: [AGENTS.EN.md](AGENTS.EN.md) (collaboration contract), [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md) (structure/assemblies), [EXTENDING.EN.md](EXTENDING.EN.md) (how to add things), [CodeComments.EN.md](CodeComments.EN.md) (comment standard).
> Note: the template is still under development; [2DPlatformerTemplate.md](2DPlatformerTemplate.md) is the **build-time design spec** and is not required reading to get started.

---

## 1. Environment and dependencies

- Unity version: **2022.3.43f1c1** (see `ProjectSettings/ProjectVersion.txt`).
- Package sources (`Packages/manifest.json`):
  - Engine packages: `com.unity.feature.2d`, `com.unity.inputsystem` (1.7.0), `com.unity.cinemachine` (2.10.3), `com.unity.textmeshpro`, etc.
  - Third-party (git URLs, fetched on clone): `com.dbrizov.naughtyattributes` (2.1.6), `com.grignardreagent.gamelog`, `com.grignardreagent.timer`.
  - Dev/AI tool (**optional**, may be removed later): `com.coplaydev.unity-mcp`.
- **First open**: let Unity resolve packages (it fetches git packages) → wait for compilation → confirm the Console has no errors.
- Missing packages: check the git URLs are reachable, then re-resolve.

## 2. Version control

- **Unity Smart Merge (once per machine)**: `Tools/setup-git.ps1` (Windows) / `Tools/setup-git.sh`; `.gitattributes` already declares `unityyamlmerge`.
- **Force Text** is on; commit every `.meta` **together with** its file.
- **Git LFS**: off by default; enable it per [AGENTS.EN.md](AGENTS.EN.md) §4.3 when assets grow.
- **One owner per scene/prefab** at a time; never edit the same scene/prefab in parallel.

## 3. Project structure

- Subsystems: `Core` / `Game` / `Character` / `Player` / `Level` / `UI` / `Editor` / `Infra`.
- Code lives under `Assets/<Subsystem>/Scripts/`; assets live with a subsystem (`SO/`, `Prefabs/`, `Physics/`) or by format (`Scene/`, `Visuals/`, `Sounds/`).
- Assembly boundaries and dependency direction: [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md); **every script must live under an asmdef**.

## 4. Run and acceptance

- Open `Assets/Scene/Samples/MainMenu.unity` and press Play → main menu → New Game loads `SampleLevel`.
- Or play `Assets/Scene/Samples/SampleLevel.unity` directly.
- Loop: move/jump/interact; **collect all 3 goals** → clear panel → `Final`; step on a `Hazard` → death panel; ride the horizontal moving platform; walk into the portal.
- Build Settings: `MainMenu(0)` / `SampleLevel(1)` / `Final(2)`; register new scenes via the tool window.

## 5. Common tasks

See [EXTENDING.EN.md](EXTENDING.EN.md):

- **Add a scene/level**: create the scene + add `LevelManager`/clear conditions + a `SceneDefinition` (register via the tool).
- **Add an interactable**: implement `IInteractable`.
- **Add a UI screen**: new script + prefab variant.
- **Add input actions / tags / data SO**: use the matching tab in `Tools/Basic2DPlatformer/Project`.
- **Add a player ability**: new component; drive movement via the `Motor` intent API.

## 6. Forking the template into a new project

1. Fork / clone this repo and open it with the matching Unity (**2022.3**).
2. Change **product name / company name**: `productName` / `companyName` in `ProjectSettings/ProjectSettings.asset`.
3. Run the **Smart Merge script** once (see §2).
4. **Clean the sample content**: replace/remove the sample scenes and placeholder `Prefabs`/`SO`/`Visuals` under `Assets/Scene/Samples`; create and register your own scenes with the tool window.
5. (Optional) **Remove MCP**: delete `com.coplaydev.unity-mcp` from `Packages/manifest.json`.
6. Open `Tools/Basic2DPlatformer/Project` and validate: all scenes registered, `Validate` passes.

## 7. Troubleshooting

- **Missing packages / compile failure**: git URLs unreachable or unresolved → re-resolve.
- **Scene won't run**: the scene must contain a `Core` prefab (input/audio/scene switch).
- **Play / tests don't advance**: frames don't advance while the Editor is unfocused → keep the Editor window focused.
- **Camera won't follow**: the virtual camera needs a body (`CinemachineTransposer`) and `Follow` set to the character.
- **Clear never triggers**: `LevelManager.rootCondition` must reference a component that implements `ILevelGoal`.

## 8. Onboarding acceptance checklist (tick each)

> Goal: confirm "this clone is ready and runs". All ticked = onboarding succeeded.

### 8.1 Dependencies (terminal)

Run from the **repo root**:

```powershell
# local-path dependencies should be 0 (no file: deps)
(Select-String -Path Packages/packages-lock.json -Pattern '"source": "local"').Count
# inspect a third-party package's source
Select-String -Path Packages/packages-lock.json -Pattern 'com.dbrizov.naughtyattributes' -Context 0,3
```

- [ ] First command prints **`0`**.
- [ ] Second shows **`"source": "git"`** in the context.
- [ ] `Library/PackageCache/` contains `com.dbrizov.naughtyattributes@*` (the package is connected to git).

### 8.2 Smart Merge (terminal)

```powershell
git config --get merge.unityyamlmerge.driver
```

- [ ] Prints something like `"...\UnityYAMLMerge.exe" merge -p %O %B %A %A`. (**No output = not configured**, see §2.)

### 8.3 Compilation and Console

- [ ] After Unity opens and finishes compiling, the **Console has no red errors** (yellow warnings are fine).

### 8.4 Built-in validation tool

- [ ] Open `Tools/Basic2DPlatformer/Project` → **Scenes** tab → `Validate`; the log shows `[Basic2DPlatformer] 校验通过, 无问题.`
- [ ] Build Settings contains `MainMenu` / `SampleLevel` / `Final`.

### 8.5 Directory tree

- [ ] `Assets/` top level contains: `Docs/ Core/ Game/ Character/ Player/ Level/ UI/ Editor/ Infra/ Scene/ Visuals/ Sounds/`.
- [ ] Each subsystem contains `Scripts/`; `Scene/Samples/` contains `MainMenu / SampleLevel / Final`.

### 8.6 Minimal game flow

Play `Assets/Scene/Samples/MainMenu.unity`:

- [ ] Main menu → **New Game** → loads `SampleLevel`.
- [ ] `A/D` moves, `Space` jumps; the camera follows within bounds.
- [ ] Collect all 3 goals → clear panel appears → **Next** → `Final`.
- [ ] Step on a `Hazard` → death panel appears.
- [ ] No red errors in the Console throughout.

## 9. More

[ARCHITECTURE.EN.md](ARCHITECTURE.EN.md) · [EXTENDING.EN.md](EXTENDING.EN.md) · [CodeComments.EN.md](CodeComments.EN.md) · [AGENTS.EN.md](AGENTS.EN.md) · [2DPlatformerTemplate.md](2DPlatformerTemplate.md) (build-time design spec, not required reading)
