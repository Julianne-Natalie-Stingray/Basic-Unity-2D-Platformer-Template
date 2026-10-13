# 2D Platformer 模板规格

> 本次工作的**唯一**设计契约。所有旧 `README.md`/`CHANGELOG.md`/`PENDING_INSPECTOR.md` 已移除;文档改由 `Assets/Docs/` 集中承载(见 §9)。
> 实现与本文件冲突时以本文件为准;变更先改本文件。
> 语言:模板文档为**中英成对**(`NAME.md` 中文基准 + `NAME.EN.md` 英文)。

---

## 1. 目标与范围

### 1.1 目标

在 `Basic Unity 2D Template` 内,参考 `PluggerHead`,总结并实现一套
**对团队协作友好、拓展点清晰、可直接开跑的 2D 横版平台模板**。

### 1.2 包含

- **协作地基**:程序集边界(asmdef)、去中心化场景/关卡注册、编辑器工具、文档机制、版本控制策略。
- **玩法**:Player 移动/跳跃/交互、Cinemachine 相机、通用交互框架、Tilemap 示例关卡。
- **流程**:主菜单 / 暂停设置 / 死亡 / 下一关 / 结算 5 屏(uGUI + TMP,英文占位文案)、内存进度。
- **音频**:音量设置持久化后**应用到 AudioMixer** 的 Master / OST / SFX 总线。
- 占位美术。

### 1.3 明确排除

| 排除项 | 原因 |
| --- | --- |
| Env 全系(电线/绕线/电压/极性/Anchor/插座/Portal) | PluggerHead 专属解谜玩法 |
| 背包系统 | PluggerHead 专属 |
| 测试程序集 | 暂缓 |
| 存档持久化 | 仅预留扩展点 |
| 默认启用 Git LFS | 每个项目自行决定(见 §10.3) |

---

## 2. 全局约束

- **写入方式**:Unity MCP 全权(场景 / 预制体 / SO / `.inputactions` / Package / ProjectSettings / Build Settings / `Assets/` 下任意文件)。
- **既有代码**:默认不重写既有实现。**获授权的 Core 重构有两处**:`Core/SceneSwitch`(见 §5)与 `Core/Audio`(音量应用,见 §7.7);其余新增能力走新文件。
- **目录**:子系统代码 `Assets/<Subsystem>/Scripts/`;工具 `Assets/Editor/`;文档 `Assets/Docs/`;共享资源 `Assets/{Scene,Visuals,Sounds}`,子系统资源 `Assets/<Subsystem>/{Prefabs,SO,Physics}`。
- **代码风格**(沿用既有):一文件一类型、`public` / `[SerializeField] private` / `private` 显式访问词、property + backing field、`GameLog` 日志、`InitializeInternal` / `ResolveDependencies`。代码注释见 [CodeComments.md](CodeComments.md);文档**中英分成两份 `.md`**(中文基准)。

---

## 3. 目标目录结构

```
Assets/
  Docs/                              (所有文档集中于此;见 §9)
    AGENTS / ARCHITECTURE / EXTENDING / CodeComments (+ .EN)
    Subsystems/<Name>.md (+ .EN.md)
  Core/       Scripts/ · SO/ · Prefabs/
  Game/       Scripts/
  Character/  Scripts/ · SO/
  Player/     Scripts/ · Prefabs/ · Physics/
  Level/      Scripts/ · SO/
  UI/         Scripts/
  Editor/     Basic2DPlatformer.Editor.asmdef · Scripts/
  Infra/      Basic2DPlatformer.Infra.asmdef · InputSystem/
  Scene/      Samples/   (测试场景放 Debug/)
  Visuals/    (贴图)
  Sounds/     Clips/ · Mixers/ · SO/
```

> **修订**:`SettingsScreen` 从 `Game/Setting` 移到 `UI`(见 §7.5),避免 Game 依赖 TMP/UI。

---

## 4. 程序集边界(asmdef)

前缀 `Basic2DPlatformer`。**依赖严格单向、无环**:

```
低层(无项目内依赖)      Infra、Game、Character
Core    → { Infra, Game }
Player  → { Core, Character }
Level   → { Game, Character }
UI      → { Core, Game, Level, Character }
Editor  → 全部
```

| 程序集 | 覆盖 | 主要引用 |
| --- | --- | --- |
| `Basic2DPlatformer.Infra` | `Assets/Infra/InputSystem`(生成物) | Unity.InputSystem |
| `Basic2DPlatformer.Core` | `Assets/Core/Scripts` | **Infra、Game**、InputSystem、GameLog、Timer、NaughtyAttributes |
| `Basic2DPlatformer.Game` | `Assets/Game/Scripts` | 无项目内引用(GameLog 为 DLL,自动引用) |
| `Basic2DPlatformer.Character` | `Assets/Character/Scripts` | 无项目内引用 |
| `Basic2DPlatformer.Player` | `Assets/Player/Scripts` | Core、Character |
| `Basic2DPlatformer.Level` | `Assets/Level/Scripts` | Game、Character |
| `Basic2DPlatformer.UI` | `Assets/UI/Scripts` | Core、Game、Level、Character、TMP、UnityEngine.UI |
| `Basic2DPlatformer.Editor` | `Assets/Editor`(Editor 平台限定) | 全部 + UnityEditor |

**硬约束**:

- 所有脚本都必须落在某个 asmdef 内,避免回退到 `Assembly-CSharp`。
- 生成物 `PlayerControls.cs` 归属 `Infra` 程序集,**不手改**;每次改动后验证生成器仍正常。
- 程序集落地**随子系统增量引入**(先 `Infra`/`Core`/`Game`/`Editor`,Player/Level/UI 落地时再加),避免引用尚不存在的程序集。

> **实现时发现的既有循环耦合(已解决)**:`AudioManager`/`SceneSwitchManager`(Core)引用 `GameState`,而 `GameSettings`(Game)引用 `AudioSettings` —— 两者互为依赖,无法两立。因 `AudioSettings` 仅被 Game 使用、Core 自身不用,已将其从 `Core/Audio/Setting/` **迁入 `Game/Setting/`**(保留 GUID),从而确立 `Game` 为低层、`Core → Game` 的真实方向。故本规格 §4 的依赖方向与最早版本相反,以本节为准。

---

## 5. 场景 / 关卡系统(去中心化)

**目标**:消除"人人改同一个 `SceneId.cs` 与中央清单"。

- **`SceneDefinition`**(ScriptableObject,放 `Basic2DPlatformer.Core`):
  - `Id`(string,稳定标识)、`SceneName`(string,运行时加载用)、可选 `DisplayName`。
  - **编辑器自定义 Inspector**:拖入 `SceneAsset` 反填 `SceneName`(避免运行时程序集引用 `UnityEditor.SceneAsset`)。
  - 每个场景/关卡一个 `SceneDefinition` 资产;谁建场景谁建它 → **新建文件,零共享文件编辑**。
  - 固定场景(MainMenu / SampleLevel / Final)与后续关卡一律走它。
- **删除** `SceneId` enum。
- **删除** `SceneSwitchConfigs`(+ `DefaultSceneSwitchConfigs.asset`):其"白名单"职责由(编辑器扫描到的)`SceneDefinition` 集合 + Build Settings 校验承担。
- **`SceneSwitchManager` 重构**(已授权):
  - `RequestSwitch(SceneDefinition)`;校验改为"定义非空 + 目标场景已在 Build Settings";
  - 运行时**不依赖中央 registry 资产**;Build Settings 注册由编辑器工具负责。
- **稳定 ID**:引用一律用 `SceneDefinition` 资产引用(GUID 稳定)或场景名,不用序号,杜绝新增/重排错位。

---

## 6. 官方拓展点

| # | 拓展 | 机制 | 是否碰共享文件 |
| --- | --- | --- | --- |
| ① 加场景/关卡 | 新建 `SceneDefinition` + 场景;编辑器工具一键注册 Build Settings | 否 |
| ② 加可交互物 | 实现 `IInteractable`(`CanInteract` / `OnInteracted` / `Interact(InteractionDetails)`) | 否 |
| ③ 加 UI 屏幕 | 新增 UI 脚本 + 预制体变体 | 否(接口在 UI 程序集) |
| ④ 加输入动作 | **编辑器工具**改 `.inputactions` 并触发重生成 + 追加 `InputManager` 事件 | 是(工具托管,Phase 2) |
| ⑤ 加玩家能力/组件 | 新增组件 + Player 预制体**变体**,不改主预制体 | 否 |
| ⑥ 加数据型 SO | SO 生成器(Phase 2) | 否 |
| ⑦ 加新 Subsystem | 目录 + asmdef + `ARCHITECTURE.md` 登记 | 是(登记) |

---

## 7. Subsystem 设计

### 7.1 Player(`Basic2DPlatformer.Player`)

移植剥离 PluggerHead:

- `Motor`(Character 子系统):拥有 Rigidbody2D,收意图(`SetMoveX`/`Jump`/`SetJumpHeld`),由物理事实推出 `MovementState`;`LockControl` 供交互锁定;可整体 `enabled=false` 移交物理。
- `CharacterMovementConfigs`(SO,几何量):`moveSpeed`/`jumpHeight`/`timeToApex`/`fallGravityMult`/`coyoteTime`/`jumpBuffer`/`variableJumpCutoff`;重力由 `Physics2D.gravity` 与推导值决定。
- `CharacterAnimationView`(Character):观察 `StateChanged`/朝向驱动 Animator;`TryPlayInteract` 播交互动画,动画事件回调解锁;不查询 Animator。
- `PlayerController`(Player):读 Core 输入 → 调 Motor 意图 API。
- `PlayerInteraction`(Player):接收 `Interact`(F),处理 `IInteractable`;成功后 `Motor.LockControl(true)`。
- `PlayerAnimationAudio`(Player):动画事件 → Core 音频。
- 交互键 **F**。

### 7.2 Camera(`Basic2DPlatformer.Core` 用户)

- 包 `com.unity.cinemachine` **2.10.3**(3.x 与项目锁定的 Input System 1.7.0 不兼容);`Camera.prefab` = Brain + VirtualCamera(Follow=Player)+ Confiner2D。
- 每场景一个 `PolygonCollider2D` 边界(Trigger / Ignore Raycast),相机预制体默认不绑定范围。

### 7.3 Level(`Basic2DPlatformer.Level`)

- `ILevelGoal.cs`:通关条件被动契约(`bool Fulfilled` + `event Changed`)。
- `LevelGoal.cs`:终点 Trigger,玩家进入 → `Fulfilled=true` + 广播 `Changed`。
- `LevelGoalAndOperator.cs` / `LevelGoalOrOperator.cs`:组合条件(全部 / 任一达成),自身也实现 `ILevelGoal`。
- `LevelManager.cs`:场景本地门面,持有 `rootCondition`;满足时广播一次 `LevelCleared`。
- `Hazard.cs`:危险 Trigger → 调用 `Motor.Die()`。
- `MovingPlatform.cs`:kinematic 刚体往返,把自身位移带给站在上面的刚体(最小实现)。
- `Portal.cs`:刚体进入触发区即移动到目标点(最小实现)。

### 7.4 Game(`Basic2DPlatformer.Game`)

- `GameStateManager` / `GameState`:**不动**(无 owner 冻结接口)。
- `AudioSettings` 已由 `Core/Audio/Setting/` 迁入此处(见 §4)。
- 新增 `FreezeCoordinator.cs`(静态,owner 集合,自动清除已销毁 owner)+ `FreezeWhileVisible.cs`。
- Progress:`GameProgress`/`LevelProgressStore` 放 Game;`LevelProgressTracker` **放 Level**(订阅同程序集的 `LevelManager`,避免 Game→Level 反向依赖);`LevelCount = 1`。
- Setting:既有 5 文件不动。

### 7.5 UI(`Basic2DPlatformer.UI`)

- `MainMenuScreen` / `SettingsScreen` / `DeathScreen` / `NextLevelScreen` / `FinalSceneScreen` / `SelectableAudio`。
- 文案改**英文占位**;`SelectableAudio` 音效复用 `AudioId.MouseClick`(不改 `AudioId`)。
- `SettingsScreen` 仅持久化音量(不调 `ApplyAudioSettings`,留 TODO)。
- **死亡预留接口(仅 C# 事件)**:

  ```csharp
  public sealed class DeathScreen : MonoBehaviour
  {
      public event Action Confirmed;   // 开发者在代码里订阅,自行接线重开/传送/读档
      public void Confirm();           // 绑定到按钮
  }
  ```

  模板只负责 `Died → 显示面板 → 冷冻`;`Confirm` 后做什么不预设。

### 7.6 数据型 SO

数据型 SO 用 `[CreateAssetMenu]`(字面量路径);自动生成模板 SO 定义见 §8 Phase 2。

### 7.7 音频(音量应用;第二处获授权的 Core 扩展)

- `Master.mixer` 暴露三个参数:`MasterVolume` / `OstVolume` / `SfxVolume`。
- `AudioManager`(Core)新增 `ApplyAudioSettings(float master, float ost, float sfx)`,按 dB 换算写入 mixer。
- `AudioManager.Start` **自行拉取** `SettingBootstrap.Settings.Audio` 并调用它(会话启动即应用);`SettingsScreen` 保存成功后调用 `CoreFacade.Instance.Audio.ApplyAudioSettings(...)`,立即生效。
- **不引入独立 applier 组件**;`AudioManager` 读取设置是既有 `Core → Game` 方向的延伸。
- 校验:改动滑块→保存→即时听到变化;重启后从 JSON 读回并生效。

---

## 8. 编辑器工具窗口(`Assets/Editor`,Phase 分期)

单一窗口 `Tools/Basic2DPlatformer/Project`:

- **Phase 1(先做)**:场景/关卡 —— 列出全部 `SceneDefinition`、一键注册 Build Settings、打开场景、校验(空引用、场景缺失、未注册、重复 Id)。
- **Phase 2(已实现)**:输入动作(增删 action/binding,写回 `.inputactions` 并触发 `PlayerControls.cs` 重生成)、Tag/Layer(查看 + 新增)、**SO 定义生成器**(生成 `XxxConfigs` 脚本 + `[CreateAssetMenu]` + 默认资产)。

> SO 生成器**放在模板自带 Editor 工具**;已移除外部 `MenuTool` 依赖。

---

## 9. 文档机制

- 位置:`Assets/Docs/`(全部集中);子系统文档 `Assets/Docs/Subsystems/<Name>.md`。
- 命名:`NAME.md` = 中文(**基准**),`NAME.EN.md` = 英文;两份**全部成对**;英文落后时以中文为准。
- 简单版内容:`AGENTS.md`、`ARCHITECTURE.md`(子系统 + 拓展点)、`EXTENDING.md`、每子系统一份 README。
- `AGENTS.md` 承载:分工与写入边界、代码/文档规范、**版本控制流程**(含 Smart Merge 与 LFS onboarding)、**一人一场景/预制体 owner**纪律、交付流程。
- **暂不做任何强制**(纯文档约定);预留"编辑器校验器"接口,Phase 2 可接入。
- ⚠️ 注意:AI 代理通常只自动发现**仓库根或工作区根**的 `AGENTS.md`,`Assets/Docs/AGENTS.md` 可能不被自动读取。建议在仓库根放一个**指针 stub**(指向 `Assets/Docs/AGENTS.md`),此项待你确认。

---

## 10. 版本控制策略

### 10.1 Unity Smart Merge

- `.gitattributes`(由我写,提交进仓库)声明:
  ```
  *.unity *.prefab *.asset *.controller *.meta  merge=unityyamlmerge
  ```
- 本机驱动(`merge.unityyamlmerge.driver`)因人而异,**由一键脚本 `Tools/setup-git.*` 检测本机 Unity 路径后写入**,不提交、不代跑。
- 团队要求:每台机器装对应 Unity 版本并跑一次脚本;**Force Text** 已开。
- 边界:同对象同字段冲突仍无法自动合并 → **一人一 owner**。

### 10.2 合并纪律

- 一个场景/预制体同一时刻只有一个 owner;禁止两人并行编辑。
- 大二进制不并行编辑(需 LFS 锁,见下)。

### 10.3 Git LFS(可选,按项目决定)

- 模板**默认不启用**;在 `AGENTS.md` 中提供"如何按项目开启 + 跟踪清单 + 对协作者要求 + 配额/清理注意事项"的说明,由每个项目自行决定。
- 跟踪清单(启用时):`png/psd/aseprite/tga/jpg`、`wav/mp3/ogg`、`ttf/otf`、`fbx/blend`、`mp4/mov`。
- 明确**不要** LFS:`.unity/.prefab/.asset`(文本,交给 Smart Merge)与 `.meta`。

---

## 11. 资源与场景清单

| 类别 | 产物 |
| --- | --- |
| 贴图 | `Sprites/Player.png`、`Ground.png`、`Goal.png`(占位) |
| 物理 | `Physics/Player.physicsMaterial2D`(friction 0 / bounciness 0) |
| Tile | `SO/GroundTile.asset` |
| SO | 每个场景一个 `SceneDefinition` |
| 预制体 | `Prefabs/Player.prefab`、`Prefabs/Camera.prefab` |
| 场景 | `Scenes/MainMenu.unity`、`SampleLevel.unity`、`Final.unity` |
| 层级 | `Ground`、`Player` |
| asmdef | §4 清单 |

`SampleLevel`: `Core` 预制体 + `Grid`(Tilemap + TilemapCollider2D + CompositeCollider2D + 静态 Rigidbody2D) + `Player` + `Main Camera`(Camera 预制体) + `GlobalUI`(DeathScreen/NextLevelScreen) + 3 个可收集 `LevelGoal` + `LevelConditions`(`LevelGoalAndOperator`) + `LevelManager`(`rootCondition` 指向 `LevelGoalAndOperator`) + `Hazard` + 水平 `MovingPlatform` + `Portal`(→ `PortalExit`) + `EventSystem`。

---

## 12. 交付顺序(地基 → 玩法)

1. **文档清理 + 规格**(已完成)。
2. **协作地基**
   1. asmdef:`Infra` / `Core` / `Game` / `Editor` 落地,验证生成物与编译。
   2. `SceneDefinition` + `SceneSwitch` 重构 + 删 `SceneId`/`SceneSwitchConfigs`;`SceneDefinition` 自定义 Inspector。
   3. 编辑器工具窗口 **Phase 1**。
   4. `Ground`/`Player` 层。
   5. 文档骨架(`AGENTS` / `ARCHITECTURE` / `EXTENDING` / `Subsystems`)+ 根 `AGENTS.md` stub(待确认)。
   6. VCS:`.gitattributes` + `Tools/setup-git.*` + AGENTS 相关章节。
3. **玩法**
   7. Player + 输入(加 `Interact`=F)+ 交互框架。
   8. Camera(Cinemachine)。
   9. Level + 占位资源 + `SampleLevel`。
   10. Game 冻结/进度 + UI 5 屏 + `SceneDefinition` + Build Settings + `MainMenu`/`Final`。
   11. 音频:`Master.mixer` 暴露参数 + `AudioManager.ApplyAudioSettings`,并由 `AudioManager.Start` 自行应用;验证即时与重启生效。
4. **二期**
   12. 编辑器工具 Phase 2:输入(增删动作/绑定 + 重生成)、Tag/Layer(查看 + 新增)、SO 生成器;移除 `MenuTool` 依赖。
5. **验收**:跑通闭环 + `read_console` 查错。

每步编译通过、控制台无错误后再进入下一步。

---

## 13. 验收标准

- **协作**:新成员按 `AGENTS.md` 可完成 onboarding(Smart Merge 脚本、owner 规则);加一个场景只需"建场景 + 建 `SceneDefinition`",编辑器工具一键注册,**不碰任何共享代码文件**。
- **玩法**:`MainMenu → SampleLevel`,A/D 移动、Space 跳跃(几何参数手感)、F 交互;相机跟随并受边界限制;踩 `Hazard` → 死亡面板 + 冷冻;可乘坐水平 `MovingPlatform`;走进 `Portal` 传送;收集全部 3 个目标(`LevelGoalAndOperator` 达成 → `LevelManager.LevelCleared`)→ 下一关面板 → `Final`;暂停/设置可开关、冻结/解冻正确、音量可保存。
- **构建**:程序集单向依赖、控制台无报错。

---

## 14. 未决 / TODO

- TODO: 进度持久化。
- TODO: 测试程序集。
- TODO: LFS 按项目启用(说明在 AGENTS)。
