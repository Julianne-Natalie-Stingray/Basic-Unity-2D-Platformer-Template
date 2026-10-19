# UI Subsystem

> 中文: [UI.md](UI.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

UI displays game state and turns user intent into requests to other subsystems. It does not own game rules and never writes time scale directly.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `UiPanel.cs` | MonoBehaviour | Visibility, focus restore, and optional freezing for one panel |
| `SceneSwitchButton.cs` | MonoBehaviour | Button → `SceneSwitchManager.RequestSwitch(SceneDefinition)` |
| `OpenPanelButton.cs` | MonoBehaviour | Button → `UiPanel.Open()` |
| `QuitGameButton.cs` | MonoBehaviour | Button → quit; stops play mode in the editor |
| `SettingsScreen.cs` | MonoBehaviour | Read, save, and apply volume settings |
| `NextLevelScreen.cs` | MonoBehaviour | `EnvironmentFacade.Level`'s `LevelCleared` → `UiPanel.Open()` |
| `DeathScreen.cs` | MonoBehaviour | `PlayerDeath.PlayerDied` → `UiPanel.Open()`; exposes `Confirmed` |
| `SelectableAudio.cs` | MonoBehaviour | Hover / down / up / submit / slider SFX |

## Conventions

- Panels start inactive; always open/close through `UiPanel.Open()` / `UiPanel.Close()`, never `SetActive`.
- Button behaviour lives in components plus their data; screen scripts contain no button methods.
- Freezing: a panel's `freezeWhileOpen` requests/releases through `FreezeCoordinator`; never write `Time.timeScale`.
- Focus: `UiPanel` selects `defaultSelection` on open and restores the previous selection on close. Keyboard/gamepad navigation comes from `InputSystemUIInputModule` on the `EventSystem`; no UI map is needed in `.inputactions`.
- `Confirmed` is a **C#-only** extension point with no default subscriber; `Confirm()` is invoked by the button's persistent listener.

## Boundaries

- `NextLevelScreen` subscribes to `LevelCleared` through `EnvironmentFacade.Level`; `DeathScreen` subscribes to `PlayerDeath.PlayerDied`. Neither scans the scene.
- `UI → Level` (environment facade) and `UI → Player` (death event); it no longer depends on `Character` directly.
- Canvas roots are named `UiRoot`; it is only a hierarchy anchor and carries no wiring component.
- `Confirmed` remains a **C#-only** extension point with no default subscriber.
