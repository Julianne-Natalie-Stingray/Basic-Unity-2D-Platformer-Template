# 架构

> 本文档描述模板的系统划分、程序集边界与拓展点。

## 1. 程序集边界

前缀 `Basic2DPlatformer`,依赖严格单向、无环。

可见性约定:

- **类型一律 `public`**:场景/预制体按 GUID 挂载脚本,且 Unity 的序列化不依赖 C# 可见性。
- **成员默认 `internal`**;仅当满足下列之一时用 `public`:被另一程序集使用;被 `Button.onClick` 持久监听;接口实现;文档承诺的扩展点。
- **字段一律 `[SerializeField] private`**,禁止 `public`/`internal` 字段(Unity 不序列化 `internal` 字段)。
- `[InternalsVisibleTo]` 只允许测试/Editor 程序集,且须在此登记。

```
外部包(零项目依赖)   GrignardReagent.Pooling、GrignardReagent.LayeredId
低层(无项目内依赖)   Contracts、Infra、Character
Game    → { Contracts }
Core    → { Infra, Game, Contracts, GrignardReagent.Pooling, GrignardReagent.LayeredId }
Level   → { Game, Contracts }
Player  → { Core, Character, Contracts }
UI      → { Core, Game, Level, Player }
Editor  → { Infra, Core, Game, UI, GrignardReagent.LayeredId, GrignardReagent.LayeredId.Editor }(传递可见全部)
```

| 程序集                           | 覆盖                         | 职责                    |
| ----------------------------- | -------------------------- | --------------------- |
| `Basic2DPlatformer.Infra`     | `Assets/Infra`             | 生成物与底层设置            |
| `Basic2DPlatformer.Game`      | `Assets/Game/Scripts`      | 全局状态、时间缩放、设置数据、进度、存档       |
| `Basic2DPlatformer.Contracts` | `Assets/Contracts/Scripts` | 跨程序集契约                |
| `Basic2DPlatformer.Core`      | `Assets/Core/Scripts`      | 输入 / 音频 / 场景切换 / 存档   |
| `Basic2DPlatformer.Character` | `Assets/Character/Scripts` | 角色运动机制、运动状态、运动参数、动画视图 |
| `Basic2DPlatformer.Player`    | `Assets/Player/Scripts`    | 输入控制器、交互、动画音频、死亡状态    |
| `Basic2DPlatformer.Level`     | `Assets/Level/Scripts`     | 环境门面、关卡内逻辑、通关与失败     |
| `Basic2DPlatformer.UI`        | `Assets/UI/Scripts`        | 面板、HUD、点击层与热键        |
| `Basic2DPlatformer.Editor`    | `Assets/Editor`            | 编辑器工具与校验              |
| `GrignardReagent.LayeredId`   | `Assets/LayeredId/Runtime` | 分层标识与目录基类;无 UnityEngine 依赖 |
| `GrignardReagent.LayeredId.Editor` | `Assets/LayeredId/Editor` | 分层抽屉与目录校验窗口(Editor 限定) |

## 2. 子系统

| 子系统       | 位置                         | 职责                                                                                                                              |
| --------- | -------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Infra     | `Assets/Infra`             | 生成物:`PlayerControls`                                                                                                            |
| Core      | `Assets/Core/Scripts`      | `CoreFacade`,输入、音频、场景切换、`SaveService`、`PassThroughUi` 标记;对象池经 `GrignardReagent.Pooling`                                          |
| Game      | `Assets/Game/Scripts`      | `GameStateManager`, `TimeScaleManager`,Setting 存储,Progress,`SaveSystem`/`GameSaveStore`                                                    |
| Contracts | `Assets/Contracts/Scripts` | 跨程序集契约:`IInteractable`、`InteractionDetails`、`IKillable`、`ISaveContributor`                                                      |
| Character | `Assets/Character/Scripts` | `Motor`、`MovementState`、`CharacterMovementConfigs`、`CharacterAnimationView`                                                     |
| Player    | `Assets/Player/Scripts`    | `PlayerController`(输入→Motor)、`PlayerInteraction`、`PlayerAnimationAudio`、`PlayerDeath`(实现 `IKillable`,广播 `PlayerDied`)           |
| Level     | `Assets/Level/Scripts`     | `EnvironmentFacade`、通关条件(`ILevelGoal`/`LevelGoalAndOperator`/`LevelGoalOrOperator`/`LevelManager`)、失败通道、终点、危险区                       |
| UI        | `Assets/UI/Scripts`        | `UiPanel`、按钮组件、`SettingsScreen`、`NextLevelScreen`、`DeathScreen`、`SelectableAudio`、`UiPointer`、`UiClickAudio`、`UiHotkeys` |
| LayeredId | `Assets/LayeredId`         | `ILayeredId`、`LayeredIdCatalog`、`[LayeredIdCatalog]`、`LayeredIdDrawer`、目录校验窗口(可整体抽包)                                             |
| Editor    | `Assets/Editor`            | 项目工具窗口、自定义 Inspector、`AudioIdDrawer`、`AudioManagerConfigsEditor`                                                                    |

各子系统细节见 [Subsystems/](Subsystems/)。

## 3. 场景 / 关卡系统

- `SceneDefinition`(ScriptableObject,Core):每个场景/关卡自带一份,字段 `Id`、`SceneName`、`DisplayName`。
- **没有中央场景清单**,也没有手写 `SceneId`;新增场景 = 建场景 + 建它的 `SceneDefinition`。
- 编辑器工具窗口(`Tools/Basic2DPlatformer/Project`)负责扫描、一键注册 Build Settings、校验。
- `SceneSwitchManager.RequestSwitch(SceneDefinition)` 是切换的唯一入口。

## 4. 拓展点

| #              | 拓展                                                                                          | 机制      | 是否碰共享文件 |
| -------------- | ------------------------------------------------------------------------------------------- | ------- | ------- |
| ① 加场景/关卡       | 建场景 + 放 `LevelManager`/通关条件 + `EnvironmentFacade` + 建 `SceneDefinition`;工具注册 Build Settings | 否       |         |
| ② 加可交互物        | 实现 `IInteractable`(Contracts)                                                               | 否       |         |
| ③ 加 UI 屏幕      | 新增 `UiPanel` 面板 + 按钮组件/屏幕脚本                                                                 | 否       |         |
| ④ 加输入动作        | 编辑器工具(Phase 2)改 `.inputactions` 并重生成                                                        | 是(工具托管) |         |
| ⑤ 加玩家能力        | 新增组件 + Player 预制体变体(运动经 `Motor` 意图 API;死亡经 `IKillable`)                                     | 否       |         |
| ⑥ 加数据型 SO      | SO 生成器(Phase 2)                                                                             | 否       |         |
| ⑦ 加新 Subsystem | 目录 + asmdef + 本文登记                                                                          | 是(登记)   |         |
| ⑧ 加项目级契约       | 跨程序集接口/载荷放 `Contracts`(零项目依赖)                                                               | 是(登记)   |         |
| ⑨ 加存档 section    | 实现 `ISaveContributor`,key 稳定且唯一                                                             | 否       |         |
| ⑩ 加分层 ID 域      | 新增 ID struct + 目录类 + 3 行抽屉绑定;后续新增条目只加一行 `const`                                                  | 否(各自目录内) |         |
| ⑪ 加关卡失败条件     | 自定义组件持 `LevelManager` 引用并调 `NotifyFailed()`                                                  | 否       |         |
| ⑫ 加空白点击层      | 实例化 `ClickScreen.prefab` 为 `UiRoot` 首个子物体,按需加 `UiClickAudio`                                     | 否       |         |

具体做法见 [EXTENDING.md](EXTENDING.md)。

## 5. 关键约定

- 所有脚本必须落在某个 asmdef 内,禁止回退到 `Assembly-CSharp`。
- 生成物(`PlayerControls.cs`)不手改。
- `Time.timeScale` 只有 `TimeScaleManager` 一个写入者;暂停与速度都按 owner 请求。
- 分层标识一律用 `LayeredId` 派生的值类型;目录只放 `public const string`,路径发布后视为冻结。
- 世界指针玩法判断遮挡使用 `UiPointer.IsPointerOverBlockingUi()`,不使用 `EventSystem.IsPointerOverGameObject()`。
- 文档以中文维护;代码注释见 [CodeComments.md](CodeComments.md)。
