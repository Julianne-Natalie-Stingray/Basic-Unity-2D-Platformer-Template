# Pooling 包

> English: [Pooling.EN.md](Pooling.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

通用组件池:以 prefab 为模板创建/复用 `Component` 实例。独立 asmdef `GrignardReagent.Pooling`,不依赖任何 `Basic2DPlatformer.*`,也不引用 `GameLog`。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `ComponentPool.cs` | class | 组件池;总上限、active 记账、幂等释放、预热、清理 |
| `GameObjectPool.cs` | class | GameObject 适配器;内部复用 `ComponentPool<Transform>` |

## API / 语义

- `Get()`:池空且 `CountAll >= MaxTotal` 时返回 `null`;否则复用 inactive 或创建。
- `Release(item)`:幂等;`null`/已销毁/外来/重复释放 → `Debug.LogWarning` 并忽略。
- `ReleaseAll()`:把所有 active 归还为 inactive(不销毁)。
- `Clear(bool destroyActive = false)`:销毁 inactive;为真时连 active;两者都先跑 `onReturn`。
- `Dispose()`:`Clear(true)`。
- `Prewarm(count)`:只创建 inactive,不触发 `onGet`/`onReturn`;按 `MaxTotal` 截断。
- `CountAll` / `CountActive` / `CountInactive` / `MaxTotal`。
- `GameObjectPool`:与 `ComponentPool<T>` 同语义,但 `Get()` 返回 `GameObject`、`Release(GameObject)`。

## 约定

- 无 namespace;asmdef `GrignardReagent.Pooling`;`MaxTotal <= 0` 表示不设上限。
- `onGet` / `onReturn` 由调用方提供;对象重置放在 `onReturn`。
- 不要从外部 `Destroy` 池化对象;统一用 `Release` / `Clear` / `Dispose`。
- `Clear` / `Release` 的销毁在 play 模式用 `Destroy`,编辑模式用 `DestroyImmediate`。

## 状态

- 当前是项目内 asmdef;计划抽成独立 UPM 包。
- EditMode 测试:`Assets/Pooling/Tests`(15 个用例:`ComponentPool` 9 个 + `GameObjectPool` 6 个,覆盖上限、复用、重复/外来释放、`Clear`、`ReleaseAll`、`Prewarm`)。
