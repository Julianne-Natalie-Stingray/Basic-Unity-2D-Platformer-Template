# Contracts Layer

> 中文: [Contracts.md](Contracts.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

Holds interfaces and data payloads that are implemented or consumed by **several unrelated assemblies**. It has zero project dependencies and references no `Basic2DPlatformer.*` assembly.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `IInteractable.cs` | interface | Scene interaction contract (`CanInteract` / `OnInteracted` / `Interact`) |
| `InteractionDetails.cs` | class | Interaction payload (`Actor` / `Target`); stays next to `IInteractable` |
| `IKillable.cs` | interface | Contract for things the environment may kill (`Kill`) |

## Rules

- A type used by another assembly's signature must live below that assembly; keep an interface and its payload together.
- `Contracts` must not reference any `Basic2DPlatformer.*` assembly and must not contain `MonoBehaviour`s (interfaces and plain data only).
- Types used inside a single assembly, or only by upper layers as concrete types, stay where they are.
- Run `Validate` in the tool window after adding a contract (it checks asmdef edges).

## Ownership

- `Player` implements `IKillable`; `Level` calls `IKillable`.
- `Player` consumes `IInteractable`; any gameplay assembly may implement `IInteractable`.
