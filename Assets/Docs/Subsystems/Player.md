# Player 子系统

## 子系统功能

负责输入、交互、动画音频与死亡状态；不负责运动机制与环境规则。

## 依赖的子系统及具体依赖业务

- Core：输入与音频。
- Character：`Motor` 与动画视图。
- Contracts：`IInteractable` 与 `IKillable`。

## API

### 字段

- `PlayerDeath.IsDead`：死亡状态。

### 事件

- `PlayerDeath.PlayerDied`：死亡广播。

### 公共方法

- `PlayerDeath.Kill`：进入死亡。
- `PlayerInteraction.TryPerformOperation`：执行交互。

## 注意事项

### 总则

- 死亡由环境调用 `Kill`；调用幂等。
- 死亡时清速度、关物理并播动画。
- `PlayerDied` 供 UI 订阅。
- 输入与音频依赖场景 `Core`。

### Core

- 输入与音频经 `CoreFacade`。
- 缺少 `Core` 时控制器禁用。

### Character

- 移动经 `Motor` 意图 API。
- 死亡调用 `CharacterAnimationView.PlayDeath`。

### Contracts

- 实现 `IKillable`；消费 `IInteractable`。
- 

## 内容

```text
Player/
  Prefabs/
    Player.prefab — 玩家预制体
  Physics/
    Player.physicsMaterial2D — 物理材质
  Scripts/
    Basic2DPlatformer.Player.asmdef — 程序集边界
    PlayerController.cs — 输入转发
    PlayerInteraction.cs — 交互处理
    PlayerAnimationAudio.cs — 动画音效
    PlayerDeath.cs — 死亡状态
```

## 实现思路

### 代码架构

控制器、交互、音频、死亡分工。

### 使用的范式

组合、事件、契约。

### 内部方法 / 字段 / 事件

- `PlayerDeath.motor`：运动。
- `PlayerDeath.body`：刚体。
- `PlayerInteraction.hits`：命中缓存。
- `PlayerAnimationAudio.deathHandle`：死亡音效。
