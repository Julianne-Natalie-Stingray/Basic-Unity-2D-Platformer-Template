# Level Subsystem

> 中文: [Level.md](Level.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

Level owns environment-side rules and the single outward entry point: clear conditions, hazards, moving platforms, portals, and the "what the environment may kill" contract. It does **not** depend on Character/Player; the dependency is inverted through `IKillable`.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `EnvironmentFacade.cs` | MonoBehaviour | Single environment entry; exposes `LevelManager` |
| `ILevelGoal.cs` | interface | Passive clear-condition contract |
| `LevelGoal.cs` | MonoBehaviour | Goal trigger; identifies the player by the `Player` tag |
| `LevelGoalAndOperator.cs` / `LevelGoalOrOperator.cs` | MonoBehaviour | Condition composition (all / any) |
| `LevelManager.cs` | MonoBehaviour | Root-condition facade; raises `LevelCleared` |
| `Hazard.cs` | MonoBehaviour | Hazard trigger; calls `IKillable.Kill()` |
| `Portal.cs` / `PortalState.cs` | MonoBehaviour / enum | Portal: only `Active` can send; one-way lock and exit cooldown are applied by the entrance |
| `LevelProgressTracker.cs` | MonoBehaviour | Records completion |
| `MovingPlatform.cs` / `Portal.cs` | MonoBehaviour | Level gadgets |

## Public API

- `EnvironmentFacade.Level` — the scene's `LevelManager`.
- `LevelManager.IsCleared` / `LevelCleared`.
- `ILevelGoal.Fulfilled` / `Changed`.
- `Portal.State` / `ActivatePortal(bool)` / `PortalCooldown()`.

> `IKillable` lives in **Contracts**; Level uses it to invert its dependency on Player.

## Notes

- **Death presentation belongs to Player** (`PlayerDeath`); Level only triggers it and never touches `Motor`.
- `LevelGoal` identifies the player by the `Player` tag; the player root must carry that tag and its Collider2D must sit on the root.
- `EnvironmentFacade` lives on the same GameObject as `LevelManager` (`SceneRoot`); new level scenes must include it.
- Level's asmdef references only `Game` and `Contracts`; a direct reference to Character/Player means the direction is wrong.
- `Portal`: only `Active` can send. `Cooldown` (yellow) and `Inactive` (red) cannot send but **can still receive**. After the entrance teleports: if the exit is one-way (`oneWayPortal`) the entrance calls `ActivatePortal(false)` to lock it; otherwise it calls `PortalCooldown()` for `cooldownTime` seconds (yellow), then the exit returns to `Active` (blue).
