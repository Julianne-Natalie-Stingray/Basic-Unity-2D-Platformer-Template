# Character Subsystem

> 中文: [Character.md](Character.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

Character movement mechanics: owns the body, accepts intent, and derives movement state from physics facts. It does not own input, animation assets, death semantics, or gameplay rules.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `Motor.cs` | MonoBehaviour | Owns `Rigidbody2D`; accepts intent (`SetMoveX` / `Jump` / `SetJumpHeld`); derives `MovementState`; `Activate` / `Deactivate` is the only on/off entry; `LockHorizontal` supports interaction locking |
| `MovementState.cs` | enum | `Idle` / `Run` / `Jump` / `Fall`; **no `Dead`** |
| `CharacterMovementConfigs.cs` | ScriptableObject | Geometry parameters; gravity and jump velocity derived from `Physics2D.gravity` |
| `CharacterAnimationView.cs` | MonoBehaviour | Drives Animator from `StateChanged` and facing; `TryPlayInteract`; `PlayDeath` is called by `PlayerDeath` |

## Public API

- `Motor.SetMoveX` / `Jump` / `SetJumpHeld` / `LockHorizontal` / `Activate` / `Deactivate`.
- `Motor.State` / `StateChanged` / `IsGrounded` / `Velocity` / `Facing` / `IsControlLocked`.
- `CharacterAnimationView.TryPlayInteract` / `OnInteractionAnimationEnded` / `PlayDeath`.
- Read-only parameters on `CharacterMovementConfigs`.

## Boundaries

- `Character → []`: it references no Level / Player / Core / Game assembly.
- **Death semantics live in `PlayerDeath` (Player)**; `Motor` does not know "dead" and only offers on/off.
- Physics shutdown (`body.simulated = false`) and velocity reset belong to `PlayerDeath`.
- Animation-event methods stay `public` (the Animator invokes them by name).
