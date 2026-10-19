# Player Subsystem

> 中文: [Player.md](Player.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

Wires input, interaction, animation audio, and death state onto the character. It does not own movement mechanics or environment rules.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `PlayerController.cs` | MonoBehaviour | Reads `CoreFacade.Input` → calls the `Motor` intent API |
| `PlayerInteraction.cs` | MonoBehaviour | Finds and handles `Contracts.IInteractable`; on success calls `Motor.LockHorizontal(true)` |
| `PlayerAnimationAudio.cs` | MonoBehaviour | Animation events → Core audio; skips movement SFX while `PlayerDeath.IsDead` |
| `PlayerDeath.cs` | MonoBehaviour | Implements `Contracts.IKillable`; `Kill()` shuts physics/motion down, plays the death animation, and raises `PlayerDied` |

## Public API

- `PlayerDeath.Kill()` (`IKillable`) / `IsDead` / `PlayerDied`.
- `PlayerInteraction.TryPerformOperation()`.

## Boundaries

- `Player → {Core, Character, Contracts}`; it **does not depend on Level**.
- The environment calls `IKillable.Kill()`; `Motor` has no death semantics.
- `PlayerDied` is a C# event for the UI; `DeathScreen` subscribes directly (`UI → Player`).
