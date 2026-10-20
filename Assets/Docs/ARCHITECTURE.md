# 架构

> 本文档描述模板的系统划分、程序集边界与拓展点。
> 设计规格的完整推导见 [2DPlatformerTemplate.md](2DPlatformerTemplate.md)。

## 1. 程序集边界

前缀 `Basic2DPlatformer`,依赖严格单向、无环。

可见性约定:

- **类型一律 `public`**:场景/预制体按 GUID 挂载脚本,且 Unity 的序列化不依赖 C# 可见性。
- **成员默认 `internal`**;仅当满足下列之一时用 `public`:被另一程序集使用;被 `Button.onClick` 持久监听;接口实现;文档承诺的扩展点。
- **字段一律 `[SerializeField] private`**,禁止 `public`/`internal` 字段(Unity 不序列化 `internal` 字段)。
- `[InternalsVisibleTo]` 只允许测试/Editor 程序集,且须在此登记。

```
外部包(零项目依赖)   GrignardReagent.Pooling
低层(无项目内依赖)   Contracts、Infra、Character
Game    → { Contracts }
Core    → { Infra, Game, Contracts, GrignardReagent.Pooling }
Level   → { Game, Contracts }
Player  → { Core, Character, Contracts }
UI      → { Core, Game, Level, Player }
Editor  → { Infra, Core, Game, UI }(传递可见全部)
```

| 程序集                           | 覆盖                         | 职责                    |
| ----------------------------- | -------------------------- | --------------------- |
| `Basic2DPlatformer.Infra`     | `Assets/Infra`             | 生成的输入包装;**不手改**       |
| `Basic2DPlatformer.Game`      | `Assets/Game/Scripts`      | 全局状态、设置数据、进度、存档编排 |
| `Basic2DPlatformer.Contracts` | `Assets/Contracts/Scripts` | 跨程序集契约(`IInteractable`/`InteractionDetails`/`IKillable`/`ISaveContributor`) |
| `GrignardReagent.Pooling`     | UPM 包(`Packages/manifest.json`) | 组件池(`ComponentPool<T>` / `GameObjectPool`);独立包,不依赖项目程序集 |
| `Basic2DPlatformer.Core`      | `Assets/Core/Scripts`      | 输入 / 音频 / 场景切换 / 存档宿主;对象池经 `GrignardReagent.Pooling` |
| `Basic2DPlatformer.Character` | `Assets/Character/Scripts` | 角色运动机制、运动状态、运动参数、动画视图 |
| `Basic2DPlatformer.Player`    | `Assets/Player/Scripts`    | 输入控制器、交互、动画音频、死亡状态(`PlayerDeath`) |
| `Basic2DPlatformer.Level`     | `Assets/Level/Scripts`     | 环境门面、通关条件、危险区、移动平台、传送门 |
| `Basic2DPlatformer.UI`        | `Assets/UI/Scripts`        | 面板、按钮组件、屏幕;订阅 `PlayerDied` 与 `LevelCleared` |
| `Basic2DPlatformer.Editor`    | `Assets/Editor`            | 编辑器工具与校验(Editor 平台限定) |

> 注意:`Core → Game`(而非相反)——`AudioManager`/`SceneSwitchManager` 依赖 `GameState`。`AudioSettings` 因此位于 `Game/Setting/`。`Game → Contracts` 只为 `ISaveContributor`;`Contracts` 仍零项目依赖。

## 2. 子系统

| 子系统       | 位置                         | 职责                                                                                                                                     |
| --------- | -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Infra     | `Assets/Infra`             | 生成物:`PlayerControls`                                                                                                                   |
| Core      | `Assets/Core/Scripts`      | `CoreFacade`,输入、音频、场景切换、`SaveService`;对象池经 `GrignardReagent.Pooling` |
| Game      | `Assets/Game/Scripts`      | `GameStateManager`,Setting 存储,Progress,`SaveSystem`/`GameSaveStore`                                                                     |
| Contracts | `Assets/Contracts/Scripts` | 跨程序集契约:`IInteractable`、`InteractionDetails`、`IKillable`、`ISaveContributor`                                                               |
| Pooling   | UPM 包 | `ComponentPool<T>` / `GameObjectPool`;独立 asmdef,不依赖项目程序集 |
| Character | `Assets/Character/Scripts` | `Motor`、`MovementState`、`CharacterMovementConfigs`、`CharacterAnimationView`                                                            |
| Player    | `Assets/Player/Scripts`    | `PlayerController`(输入→Motor)、`PlayerInteraction`、`PlayerAnimationAudio`、`PlayerDeath`(实现 `IKillable`,广播 `PlayerDied`) |
| Level     | `Assets/Level/Scripts`     | `EnvironmentFacade`、通关条件(`ILevelGoal`/`LevelGoalAndOperator`/`LevelGoalOrOperator`/`LevelManager`)、终点、危险区 |
| UI        | `Assets/UI/Scripts`        | `UiPanel`、按钮组件(场景切换 / 开面板 / 退出 / New Game / Continue / Save)、`SettingsScreen`、`NextLevelScreen`、`DeathScreen`、`SelectableAudio` |
| Editor    | `Assets/Editor`            | 项目工具窗口、自定义 Inspector                                                                                                                   |

各子系统细节见 [Subsystems/](Subsystems/)。

## 3. 场景 / 关卡系统

- `SceneDefinition`(ScriptableObject,Core):每个场景/关卡自带一份,字段 `Id`、`SceneName`、`DisplayName`。
- **没有中央场景清单**,也没有手写 `SceneId`;新增场景 = 建场景 + 建它的 `SceneDefinition`。
- 编辑器工具窗口(`Tools/Basic2DPlatformer/Project`)负责扫描、一键注册 Build Settings、校验。
- `SceneSwitchManager.RequestSwitch(SceneDefinition)` 是切换的唯一入口。

## 4. 拓展点

| #              | 拓展                                                                    | 机制      | 是否碰共享文件 |
| -------------- | --------------------------------------------------------------------- | ------- | ------- |
| ① 加场景/关卡       | 建场景 + 放 `LevelManager`/通关条件 + `EnvironmentFacade` + 建 `SceneDefinition`;工具注册 Build Settings | 否       |         |
| ② 加可交互物        | 实现 `IInteractable`(Contracts)                                          | 否       |         |
| ③ 加 UI 屏幕      | 新增 `UiPanel` 面板 + 按钮组件/屏幕脚本                                            | 否       |         |
| ④ 加输入动作        | 编辑器工具(Phase 2)改 `.inputactions` 并重生成                                  | 是(工具托管) |         |
| ⑤ 加玩家能力        | 新增组件 + Player 预制体变体(运动经 `Motor` 意图 API;死亡经 `IKillable`)                 | 否       |         |
| ⑥ 加数据型 SO      | SO 生成器(Phase 2)                                                       | 否       |         |
| ⑦ 加新 Subsystem | 目录 + asmdef + 本文登记                                                    | 是(登记)   |         |
| ⑧ 加项目级契约      | 跨程序集接口/载荷放 `Contracts`(零项目依赖)                                        | 是(登记)   |         |

具体做法见 [EXTENDING.md](EXTENDING.md)。

## 5. 关键约定

- 所有脚本必须落在某个 asmdef 内,禁止回退到 `Assembly-CSharp`。
- 生成物(`PlayerControls.cs`)不手改。
- 文档以中文维护;代码注释见 [CodeComments.md](CodeComments.md)。
