# Player 子系统

> English: [Player.EN.md](Player.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

把输入、交互、动画音频与死亡状态接到角色上;不拥有运动机制或环境规则。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `PlayerController.cs` | MonoBehaviour | 读 `CoreFacade.Input` → 调 `Motor` 意图 API |
| `PlayerInteraction.cs` | MonoBehaviour | 搜索并处理 `Contracts.IInteractable`;成功后 `Motor.LockHorizontal(true)` |
| `PlayerAnimationAudio.cs` | MonoBehaviour | 动画事件 → Core 音频;移动音效在 `PlayerDeath.IsDead` 时跳过 |
| `PlayerDeath.cs` | MonoBehaviour | 实现 `Contracts.IKillable`;`Kill()` 关物理、停运动、播死亡动画并广播 `PlayerDied` |

## 公共 API

- `PlayerDeath.Kill()`(`IKillable`)/ `IsDead` / `PlayerDied`。
- `PlayerInteraction.TryPerformOperation()`。

## 边界

- `Player → {Core, Character, Contracts}`;**不依赖 Level**。
- 死亡由环境调用 `IKillable.Kill()`;`Motor` 不含死亡语义。
- `PlayerDied` 是给 UI 的 C# 事件;`DeathScreen` 直接订阅(`UI → Player`)。
