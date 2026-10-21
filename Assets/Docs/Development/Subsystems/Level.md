# Level 子系统

## 子系统功能

负责关卡规则与环境门面；不负责角色、输入与存档文件。

## 依赖的子系统及具体依赖业务

- Game：写入通关进度。
- Contracts：`IKillable` 反转死亡依赖。

## API

### 字段

- `EnvironmentFacade.Level`：关卡门面。
- `LevelManager.IsCleared`：通关状态。
- `ILevelGoal.Fulfilled`：条件满足。
- `Portal.State`：传送门状态。

### 事件

- `LevelManager.LevelCleared`：通关广播。
- `ILevelGoal.Changed`：条件变化。

### 公共方法

- `Portal.ActivatePortal` / `PortalCooldown`：状态切换。

## 注意事项

### 总则

- 不依赖 Character/Player；死亡经 `IKillable`。
- `LevelGoal` 用 `Player` 标签识别玩家。
- `SceneRoot` 同挂 `LevelManager` 与 `EnvironmentFacade`。
- `levelId` 是稳定存档 id；场景改名不变。
- 机关状态不写入当前存档。
- 仅 `Active` 可发送；`Cooldown` / `Inactive` 可接收。
- 单向出口 `ActivatePortal(false)`；双向出口 `PortalCooldown`。

### Game

- `LevelProgressTracker` 写 `GameProgress`。
- 不读写存档文件。

### Contracts

- `Hazard` 调用 `IKillable.Kill`。
- 不直接引用 Player。

## 内容

```text
Level/
  SO/
    GroundTile.asset — 地面瓦片
  Scripts/
    Basic2DPlatformer.Level.asmdef — 程序集边界
    Devices/
      Hazard.cs — 危险区
      MovingPlatform.cs — 移动平台
      Portal.cs — 传送门
      PortalState.cs — 传送门状态
    Goals/
      ILevelGoal.cs — 条件契约
      LevelGoal.cs — 终点条件
      LevelGoalAndOperator.cs — 全部条件
      LevelGoalOrOperator.cs — 任一条件
    Persistent/
      EnvironmentFacade.cs — 环境门面
      LevelManager.cs — 通关管理
      LevelProgressTracker.cs — 进度写入
```

## 实现思路

### 代码架构

门面 + 条件组合 + 传送门状态机。

### 使用的范式

组合、事件、状态机。

### 内部方法 / 字段 / 事件

- `Portal.cooldown`：冷却协程。
- `Portal.exit`：出口门。
- `LevelManager.rootCondition`：根条件。
- `LevelProgressTracker.levelId`：稳定 id。
