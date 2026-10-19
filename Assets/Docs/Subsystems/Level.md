# Level 子系统

> English: [Level.EN.md](Level.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

Level 拥有环境侧的规则与对外的唯一入口:通关条件、危险区、移动平台、传送门,以及"环境可以杀死什么"的契约。它**不依赖** Character/Player,通过 `IKillable` 反转依赖。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `EnvironmentFacade.cs` | MonoBehaviour | 环境唯一入口;暴露 `LevelManager` |
| `ILevelGoal.cs` | interface | 通关条件被动契约 |
| `LevelGoal.cs` | MonoBehaviour | 终点触发器;用 `Player` 标签识别玩家 |
| `LevelGoalAndOperator.cs` / `LevelGoalOrOperator.cs` | MonoBehaviour | 条件组合(全部 / 任一) |
| `LevelManager.cs` | MonoBehaviour | 根条件门面;广播 `LevelCleared` |
| `Hazard.cs` | MonoBehaviour | 危险区;调用 `IKillable.Kill()` |
| `Portal.cs` / `PortalState.cs` | MonoBehaviour / enum | 传送门:只有 `Active` 能发送;单向锁与出口冷却由入口调用出口 |
| `LevelProgressTracker.cs` | MonoBehaviour | 通关写入进度 |
| `MovingPlatform.cs` / `Portal.cs` | MonoBehaviour | 关卡机关 |

## 公共 API

- `EnvironmentFacade.Level` —— 本场景的 `LevelManager`。
- `LevelManager.IsCleared` / `LevelCleared`。
- `ILevelGoal.Fulfilled` / `Changed`。
- `Portal.State` / `ActivatePortal(bool)` / `PortalCooldown()`。

> `IKillable` 属于 **Contracts** 程序集;Level 通过它反转对 Player 的依赖。

## 注意

- **死亡表现归 Player**(`PlayerDeath`);Level 只负责触发,不碰 `Motor`。
- `LevelGoal` 用 `Player` 标签识别玩家;玩家根节点必须带该标签,且 Collider2D 在根上。
- `EnvironmentFacade` 与 `LevelManager` 同 GO(`SceneRoot`);新增关卡场景要一并放置。
- `Level` 的 asmdef 只引用 `Game` 与 `Contracts`;新增对角色/玩家的直接引用意味着方向错了。
- `Portal`:`Active` 才能作为入口发送;`Cooldown`(黄)与 `Inactive`(红)不能发送,但**仍可作为出口接收**。入口传送后:出口是单向门(`oneWayPortal`)时调用 `ActivatePortal(false)` 锁死;双向门调用 `PortalCooldown()` 闪黄 `cooldownTime` 秒后回到 `Active`(蓝)。
