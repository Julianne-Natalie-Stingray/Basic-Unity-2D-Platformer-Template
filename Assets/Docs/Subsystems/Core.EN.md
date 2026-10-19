# Core Subsystem

> 中文: [Core.md](Core.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

Core is the shared runtime infrastructure: input, audio, scene switch, object pool. It does **not** decide gameplay rules or the next scene.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `CoreFacade.cs` | MonoBehaviour | Single entry point; exposes `Input` / `Audio` / `SceneSwitch`; `DontDestroyOnLoad` + duplicate guard |
| `Managers/InputManager.cs` | MonoBehaviour | Input service; wraps generated `PlayerControls`, publishes the raw device layer |
| `Managers/AudioManager.cs` | MonoBehaviour | Audio requests, pool, registry, limits, mixer application |
| `SceneSwitch/SceneSwitchManager.cs` | MonoBehaviour | Only switch entry `RequestSwitch(SceneDefinition)`; owns `Loading` |
| `SceneSwitch/SceneDefinition.cs` | ScriptableObject | Stable definition for each scene/level |
| `Audio/*` | — | `AudioBuilder` / `ISoundHandle` / `SoundHandle` / `AudioRegistry` / `AudioEmitter` / `AudioId` |
| `GrignardReagent.Pooling` | standalone asmdef | `ComponentPool<T>`; Core pools `AudioEmitter` with it |

## Public API

- `CoreFacade.Instance` — the only access point that survives a scene switch.
- `CoreFacade.Input` / `.Audio` / `.SceneSwitch`.
- `SceneSwitchManager.RequestSwitch(SceneDefinition)` — returns `AsyncOperation`, `null` when refused.
- `AudioManager.CreateBuilder().With...().Play(audioId)` — returns `ISoundHandle`.

## Boundaries and notes

- Dependency direction is **Core → Game** (`AudioManager`/`SceneSwitchManager` read `GameState`), not the reverse.
- Generated files under `Assets/Infra` (`PlayerControls.cs`) belong to the Infra assembly and are **never hand-edited**.
- A scene switch applies no mechanism: entering `Loading` does not change `timeScale` or pause audio.
- `SceneSwitchConfigs` and `SceneId` were removed, replaced by `SceneDefinition`.
- Visibility: members of `AudioRegistry` / `AudioEmitter` / `SoundHandle` are mostly `internal`; the outside uses `ISoundHandle` and `AudioManager.CreateBuilder()`.
- Object pooling moved to `GrignardReagent.Pooling` (zero project dependencies); it is planned to become a standalone UPM package.
