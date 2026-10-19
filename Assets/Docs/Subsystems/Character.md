# Character 子系统

> English: [Character.EN.md](Character.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

角色运动机制:拥有刚体、收意图、由物理事实推出运动状态。它不拥有输入、动画资源、死亡语义或玩法规则。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `Motor.cs` | MonoBehaviour | 拥有 `Rigidbody2D`;收意图(`SetMoveX` / `Jump` / `SetJumpHeld`);推出 `MovementState`;`Activate` / `Deactivate` 是唯一启停入口;`LockHorizontal` 供交互锁定 |
| `MovementState.cs` | enum | `Idle` / `Run` / `Jump` / `Fall`;**不含 `Dead`** |
| `CharacterMovementConfigs.cs` | ScriptableObject | 几何参数;由 `Physics2D.gravity` 推导重力与跳跃速度 |
| `CharacterAnimationView.cs` | MonoBehaviour | 观察 `StateChanged` 与朝向驱动 Animator;`TryPlayInteract`;`PlayDeath` 由 `PlayerDeath` 调用 |

## 公共 API

- `Motor.SetMoveX` / `Jump` / `SetJumpHeld` / `LockHorizontal` / `Activate` / `Deactivate`。
- `Motor.State` / `StateChanged` / `IsGrounded` / `Velocity` / `Facing` / `IsControlLocked`。
- `CharacterAnimationView.TryPlayInteract` / `OnInteractionAnimationEnded` / `PlayDeath`。
- `CharacterMovementConfigs` 的只读参数。

## 边界

- `Character → []`:不引用 Level / Player / Core / Game。
- **死亡语义在 `Player` 的 `PlayerDeath`**;`Motor` 不知道"死",只提供启停。
- 物理关机(`body.simulated = false`)与清速度由 `PlayerDeath` 负责。
- 动画事件方法保持 `public`(由 Animator 按名字调用)。
