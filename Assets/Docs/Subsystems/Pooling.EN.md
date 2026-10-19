# Pooling Package

> 中文: [Pooling.md](Pooling.md). Back to [ARCHITECTURE.EN.md](../ARCHITECTURE.EN.md).

## Responsibility

A general component pool: creates/reuses `Component` instances from a prefab. It is the standalone asmdef `GrignardReagent.Pooling` and depends on no `Basic2DPlatformer.*` assembly and no `GameLog`.

## Composition

| Path | Type | Responsibility |
| --- | --- | --- |
| `ComponentPool.cs` | class | Component pool: total cap, active bookkeeping, idempotent release, prewarm, clear |
| `GameObjectPool.cs` | class | GameObject adapter over `ComponentPool<Transform>` |

## API / semantics

- `Get()`: returns `null` when the pool is empty and `CountAll >= MaxTotal`; otherwise reuses an inactive instance or creates one.
- `Release(item)`: idempotent; `null`/destroyed/foreign/double release → `Debug.LogWarning` and ignore.
- `ReleaseAll()`: returns every active instance to inactive (no destroy).
- `Clear(bool destroyActive = false)`: destroys inactive instances; with `true`, active ones too. Both paths run `onReturn` first.
- `Dispose()`: `Clear(true)`.
- `Prewarm(count)`: creates inactive instances only; does not invoke `onGet`/`onReturn`; clamped by `MaxTotal`.
- `CountAll` / `CountActive` / `CountInactive` / `MaxTotal`.
- `GameObjectPool`: same semantics as `ComponentPool<T>`, but `Get()` returns `GameObject` and `Release(GameObject)`.

## Conventions

- No namespace; asmdef `GrignardReagent.Pooling`; `MaxTotal <= 0` means unbounded.
- `onGet` / `onReturn` are supplied by the caller; reset the object in `onReturn`.
- Never `Destroy` a pooled instance externally; use `Release` / `Clear` / `Dispose`.
- Destruction uses `Destroy` in play mode and `DestroyImmediate` in edit mode.

## Status

- Currently a project asmdef; planned to become a standalone UPM package.
- EditMode tests: `Assets/Pooling/Tests` (15 cases: 9 for `ComponentPool`, 6 for `GameObjectPool`, covering cap, reuse, double/foreign release, `Clear`, `ReleaseAll`, `Prewarm`).
