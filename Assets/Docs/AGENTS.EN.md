# AGENTS — Team Collaboration Contract

> This file is the collaboration contract for this project (derived from the 2D Platformer template). AI agents conventionally read the repo-root `AGENTS.md`, which is a pointer to this file.
> 中文: [AGENTS.md](AGENTS.md) · New members start with [Onboarding.EN.md](Onboarding.EN.md).

## 1. Division of labour and permissions

- Code and editor operations may be done by humans or AI; AI uses Unity MCP to operate scenes, prefabs, ScriptableObjects, packages, and input assets.
- Writable: `Assets/`, `Packages/`, `ProjectSettings/` (the latter two: be careful and keep a single owner).
- Forbidden: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `.vs/`, `.idea/`.
- Changes outside `Assets/` (`.gitattributes`, `Tools/` scripts, etc.) should land after review.

## 2. Code standards (summary)

- One type per file; explicit access modifiers; property + backing field; log via `GameLog`; `InitializeInternal` / `ResolveDependencies`.
- Code comments follow [CodeComments.EN.md](CodeComments.EN.md) (Chinese only).
- Full boundaries and rationale: [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md).

## 3. Documentation standards

- All docs live in `Assets/Docs/`; subsystem docs in `Assets/Docs/Subsystems/<Name>.md`.
- Every doc is paired: `NAME.md` (Chinese, **canonical**) + `NAME.EN.md` (English). When they diverge, Chinese wins.
- This is a simple version; responsibilities may be split later (CONTRIBUTING, ADR, etc.).

## 4. Version control

### 4.1 Unity Smart Merge

- The root `.gitattributes` declares `unityyamlmerge` for `*.unity`, `*.prefab`, `*.asset`, `*.controller`, `*.meta`.
- **Each machine must configure the merge driver once** (Unity path differs per machine): run `Tools/setup-git.ps1` (Windows) / `Tools/setup-git.sh`.
- If unconfigured, git **silently falls back** to normal merge and conflicts increase noticeably.
- Smart Merge cannot merge two edits to the same field of the same object, nor binaries.

### 4.2 Merge discipline

- **A scene or prefab has exactly one owner at a time**; never let two people edit the same scene or prefab in parallel.
- Derive shared assets via Prefab Variant instead of editing the base prefab directly.

### 4.3 Git LFS (optional, per project)

The template does **not** enable LFS by default. Enable it when assets grow:

- Install git-lfs on every machine and run `git lfs install`.
- Suggested `git lfs track` list: `*.png *.psd *.aseprite *.tga *.jpg *.wav *.mp3 *.ogg *.ttf *.otf *.fbx *.blend *.mp4 *.mov`.
- Do **not** LFS `*.unity/*.prefab/*.asset/*.meta` — they are text and belong to Smart Merge.
- Mind the quota: GitHub has free LFS storage/bandwidth limits; removing files from history does **not** free remote LFS storage (delete-and-recreate the repo, or contact support).

## 5. Extending

See [EXTENDING.EN.md](EXTENDING.EN.md).
