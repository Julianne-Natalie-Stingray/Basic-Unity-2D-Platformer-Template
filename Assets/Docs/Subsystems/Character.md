# Character 子系统

## 子系统功能

负责刚体运动与状态推导；不负责输入来源、动画资源与死亡。

## 依赖的子系统及具体依赖业务

暂无。

## API

### 字段

- `Motor.State` / `IsGrounded` / `Velocity` / `Facing` / `IsControlLocked`：运动状态。

### 事件

- `Motor.StateChanged`：状态变化。

### 公共方法

- `Motor.SetMoveX` / `Jump` / `SetJumpHeld`：移动与跳跃意图。
- `Motor.LockHorizontal` / `Activate` / `Deactivate`：控制与启停。
- `CharacterAnimationView.TryPlayInteract` / `PlayDeath`：动画请求。

## 注意事项

### 总则

- `Motor` 是唯一启停入口。
- 死亡语义不在本层。
- 物理事实推出状态，不保存输入历史。

### Player

- 物理关机由 `PlayerDeath` 负责, `PlayerDeath` 调用 `Deactivate` 与 `PlayDeath`。
- `PlayerInteraction` 调用 `LockHorizontal`。

## 内容

```text
Character/
  SO/
    CharacterMovementConfigs.asset — 运动参数
  Scripts/
    Basic2DPlatformer.Character.asmdef — 程序集边界
    Motor.cs — 刚体运动
    MovementState.cs — 运动状态
    CharacterAnimationView.cs — 动画视图
    SODefinitions/
      CharacterMovementConfigs.cs — 参数定义
```

## 实现思路

### 代码架构

`Motor` 拥有刚体；动画视图观察状态。

### 使用的范式

组合、事件、状态枚举。

### 内部方法 / 字段 / 事件

- `Motor.body`：刚体。
- `Motor.facing`：朝向。
- `Motor.isControlLocked`：控制锁。
- `CharacterAnimationView.markerPosition`：朝向标记。
