# Game Subsystem

> 中文: [Game.md](Game.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

Game holds global state, persisted settings, and (later) progress. It does not invent a second player or level model.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `GameState/GameState.cs` | enum | `Playing` / `Freezed` / `Loading` |
| `GameState/GameStateManager.cs` | static class | Holds `Current`; `Freeze`/`Resume`/`EnterLoading`/`ExitLoading`; raises `Changed` |
| `Setting/ISettingData.cs` | interface | Persisted-data contract |
| `Setting/SettingStore.cs` | generic base | Load/hold/save/reset; file I/O |
| `Setting/FileSettingStore.cs` | concrete | JsonUtility persistence |
| `Setting/GameSettings.cs` | data class | Template settings data (currently `Audio`) |
| `Setting/AudioSettings.cs` | data class | Master/Ost/Sfx volumes (0..1, defaults 1/0.5/0.5) |
| `Setting/SettingBootstrap.cs` | static class | Builds and loads the store before any scene |

> `AudioSettings.cs` was moved here from `Core/Audio/Setting/` because `GameSettings` consumes it (see [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md) §1).

## Public API

- `GameStateManager.Current` / `Freeze()` / `Resume()` / `Changed`.
- `SettingBootstrap.Settings` / `Save()` / `ResetToDefault()`.

## Notes

- `GameStateManager` currently has **no** owner-counted freeze API; panel freezing is coordinated by `FreezeCoordinator`, requested by the UI's `UiPanel` (`FreezeWhileVisible` was removed).
- The store only reads/writes; applying belongs to the system that owns the value.
- The progress subsystem (sequential unlock) is a later delivery and is not in place yet.
