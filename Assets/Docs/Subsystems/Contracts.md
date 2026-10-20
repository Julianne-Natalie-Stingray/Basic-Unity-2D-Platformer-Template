# Contracts 子系统

## 子系统功能

承载跨程序集接口与纯数据；不负责实现与运行时状态。

## 依赖的子系统及具体依赖业务

暂无。

## API

### 字段

- `InteractionDetails.Actor` / `Target`：交互双方。
- `IInteractable.CanInteract`：可交互条件。
- `ISaveContributor.Key`：存档 section 标识。

### 事件

- `IInteractable.OnInteracted`：交互完成。

### 公共方法

- `IInteractable.Interact`：执行交互。
- `IKillable.Kill`：请求死亡。
- `ISaveContributor.CanCapture` / `Capture` / `Restore`：存档快照。

## 注意事项

### 总则

- 零项目依赖；只放接口与纯数据。
- 接口与其载荷同层。
- 新增契约后运行 `Validate`。

### Player

- 实现 `IKillable`；消费 `IInteractable`。

### Level

- 调用 `IKillable`。

### Game

- 消费 `ISaveContributor`。

## 内容

```text
Contracts/
  Scripts/
    Basic2DPlatformer.Contracts.asmdef — 程序集边界
    Interfaces/
      IInteractable.cs — 交互契约
      IKillable.cs — 死亡契约
      ISaveContributor.cs — 存档契约
    Payloads/
      InteractionDetails.cs — 交互载荷
```
