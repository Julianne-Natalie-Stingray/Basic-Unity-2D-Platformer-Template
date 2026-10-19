# Contracts 层

> English: [Contracts.EN.md](Contracts.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

承载会被**多个互不相关的程序集**实现或消费的接口与数据载荷。零项目依赖,不引用任何 `Basic2DPlatformer.*`。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `IInteractable.cs` | interface | 场景可交互契约(`CanInteract` / `OnInteracted` / `Interact`) |
| `InteractionDetails.cs` | class | 交互载荷(`Actor` / `Target`);随 `IInteractable` 同层 |
| `IKillable.cs` | interface | 环境可杀死对象契约(`Kill`) |

## 规则

- 一个类型若被另一程序集的签名使用,必须位于该程序集之下;接口与其载荷同层。
- `Contracts` 不得引用任何 `Basic2DPlatformer.*`;不放 `MonoBehaviour`(只放接口/纯数据)。
- 只在单一程序集内使用、或只被上层以具体类型使用的类型,留在原子系统。
- 新增契约后运行工具窗口的 `Validate`(校验 asmdef 边)。

## 归属

- `Player` 实现 `IKillable`;`Level` 调用 `IKillable`。
- `Player` 消费 `IInteractable`;任意玩法程序集可实现 `IInteractable`。
