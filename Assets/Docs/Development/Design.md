# 2D Platformer 模板设计

> 本文记录当前代码架构与子系统设计。运行与验收见 [Manual/Verification.md](../Manual/Verification.md)，代码规范见 [CodeComments.md](CodeComments.md)。

---

## 1. 目标与范围

### 1.1 目标

在 `Basic Unity 2D Template` 内,参考 `PluggerHead`,总结并实现一套
**对团队协作友好、拓展点清晰、可直接开跑的 2D 横版平台模板**。

### 1.2 包含

- **协作地基**:程序集边界(asmdef)、去中心化场景/关卡注册、编辑器工具、文档机制、版本控制策略。
- **玩法**:Player 移动/跳跃/交互、Cinemachine 相机、通用交互框架、Tilemap 示例关卡;关卡可在任意时刻判定失败。
- **流程**:主菜单 / 设置 / 暂停 / 死亡 / 下一关 / 结算;键盘空格暂停、Esc 逐层关闭;主菜单提供 New Game / Continue,游戏内暂停菜单提供 Save(uGUI + TMP,英文占位文案)。
- **音频**:音量设置持久化后**应用到 AudioMixer** 的 Master / OST / SFX 总线;音频标识为分层路径,由 `LayeredId` 包支撑,新增音效不触碰代码分支。
- **音频/UI 可选件**:全屏空白点击层(`ClickScreen.prefab`)与点击音效模块。
- 占位美术。

### 1.3 明确排除

| 排除项 | 原因 |
| --- | --- |
| Env 全系(电线/绕线/电压/极性/Anchor/插座) | PluggerHead 专属解谜玩法 |
| 背包系统 | PluggerHead 专属 |
| 其余子系统的测试程序集 | Game 存档已有 EditMode 测试;其余暂缓 |
| 机关状态存档 | 不做:只存 `progress`;`MovingPlatform` / `Portal` 等动画中间态不作为稳定数据 |
| 默认启用 Git LFS | 每个项目自行决定(见 [AGENTS.md](../AGENTS.md)) |

---

## 2. 全局约束

- **目录**:子系统代码 `Assets/<Subsystem>/Scripts/`;外部包经 UPM;工具 `Assets/Editor/`;文档 `Assets/Docs/`;共享资源 `Assets/{Scene,Visuals,Sounds}`,子系统资源 `Assets/<Subsystem>/{Prefabs,SO,Physics}`。
- **代码风格**:一文件一类型，显式访问词，property + backing field，`GameLog` 日志，`InitializeInternal` / `ResolveDependencies`。
- **代码注释**:见 [CodeComments.md](CodeComments.md)。

---

## 3. 目标目录结构

```
Assets/
  Docs/
    AGENTS.md
    DocumentationRules.md
    Development/
      ARCHITECTURE / EXTENDING / CodeComments / Packages / Onboarding
      Design.md
      Subsystems/<Name>.md
    Manual/
      GettingStarted.md
      Verification.md
  Core/       Scripts/ · SO/ · Prefabs/
  Game/       Scripts/
  Contracts/  Scripts/
  Character/  Scripts/ · SO/
  Player/     Scripts/ · Prefabs/ · Physics/
  Level/      Scripts/ · SO/
  UI/         Scripts/ · Prefabs/
  Editor/     Basic2DPlatformer.Editor.asmdef · Scripts/
  Infra/      Basic2DPlatformer.Infra.asmdef · InputSystem/
  Scene/      Samples/   (测试场景放 Debug/)
  Visuals/    (贴图)
  Sounds/     Clips/ · Mixers/ · SO/
Packages/
  com.grignardreagent.layeredid/  Runtime/ · Editor/ · Tests/
```

---

## 4. 程序集边界(asmdef)

前缀 `Basic2DPlatformer`。**依赖严格单向、无环**:

```
外部包(零项目依赖)      GrignardReagent.Pooling、GrignardReagent.LayeredId
低层(无项目内依赖)      Contracts、Infra、Character
Game    → { Contracts }
Core    → { Infra, Game, Contracts, GrignardReagent.Pooling, GrignardReagent.LayeredId }
Level   → { Game, Contracts }
Player  → { Core, Character, Contracts }
UI      → { Core, Game, Level, Player }
Editor  → { Infra, Core, Game, UI, GrignardReagent.LayeredId, GrignardReagent.LayeredId.Editor }(传递可见全部)
```

| 程序集 | 覆盖 | 主要引用 |
| --- | --- | --- |
| `Basic2DPlatformer.Infra` | `Assets/Infra`（生成物） | Unity.InputSystem |
| `Basic2DPlatformer.Core` | `Assets/Core/Scripts` | Infra、Game、Contracts、Pooling、LayeredId、InputSystem、GameLog、Timer、NaughtyAttributes |
| `Basic2DPlatformer.Game` | `Assets/Game/Scripts` | Contracts；GameLog |
| `Basic2DPlatformer.Contracts` | `Assets/Contracts/Scripts` | 无项目内引用 |
| `GrignardReagent.Pooling` | UPM 包 | 无项目内引用 |
| `GrignardReagent.LayeredId` | `Packages/com.grignardreagent.layeredid/Runtime` | 无项目内引用，且 `noEngineReferences` |
| `GrignardReagent.LayeredId.Editor` | `Packages/com.grignardreagent.layeredid/Editor` | LayeredId、UnityEditor |
| `Basic2DPlatformer.Character` | `Assets/Character/Scripts` | 无项目内引用 |
| `Basic2DPlatformer.Player` | `Assets/Player/Scripts` | Core、Character、Contracts |
| `Basic2DPlatformer.Level` | `Assets/Level/Scripts` | Game、Contracts |
| `Basic2DPlatformer.UI` | `Assets/UI/Scripts` | Core、Game、Level、Player、TMP、UnityEngine.UI |
| `Basic2DPlatformer.Editor` | `Assets/Editor`（Editor 平台限定） | Infra、Core、Game、UI、LayeredId、LayeredId.Editor、UnityEditor |

**硬约束**：

- 所有脚本都必须落在某个 asmdef 内，避免回退到 `Assembly-CSharp`。
- 生成物 `PlayerControls.cs` 归属 `Infra` 程序集，不手改。
- 新增程序集须在 [ARCHITECTURE.md](ARCHITECTURE.md) 登记，并通过工具窗口的 `Validate`。
- `AudioSettings` 位于 `Game/Setting/`，`Game` 不反向依赖 Core。

---

## 5. 场景 / 关卡系统(去中心化)

**目标**:消除"人人改同一个 `SceneId.cs` 与中央清单"。

- **`SceneDefinition`**（ScriptableObject，Core）：
  - `Id`（稳定标识）、`SceneName`（运行时加载）、可选 `DisplayName`。
  - 自定义 Inspector：拖入 `SceneAsset` 反填 `SceneName`。
  - 每个场景/关卡一个资产；新建场景时同时新建定义。
  - MainMenu / SampleLevel / Final 与后续关卡统一使用。
- **`SceneSwitchManager`**：
  - `RequestSwitch(SceneDefinition)` 是唯一入口；校验定义非空且目标已在 Build Settings。
  - 运行时无中央 registry 资产；Build Settings 注册由编辑器工具负责。
- **稳定 ID**：引用使用 `SceneDefinition` 资产（GUID 稳定）或场景名，不使用序号。

---

## 6. 官方拓展点

| # | 拓展 | 机制 | 是否碰共享文件 |
| --- | --- | --- | --- |
| ① 加场景/关卡 | 新建 `SceneDefinition` + 场景;编辑器工具一键注册 Build Settings | 否 |
| ② 加可交互物 | 实现 `IInteractable`(`CanInteract` / `OnInteracted` / `Interact(InteractionDetails)`,Contracts) | 否 |
| ③ 加 UI 屏幕 | 新增 `UiPanel` 面板 + 按钮组件/屏幕脚本 | 否(接口在 Contracts/UI) |
| ④ 加输入动作 | 编辑器工具改 `.inputactions` 并触发重生成 + 追加 `InputManager` 事件 | 是（工具托管） |
| ⑤ 加玩家能力/组件 | 新增组件 + Player 预制体**变体**,不改主预制体;死亡经 `IKillable` | 否 |
| ⑥ 加数据型 SO | SO 生成器 | 否 |
| ⑦ 加新 Subsystem | 目录 + asmdef + `ARCHITECTURE.md` 登记 | 是(登记) |
| ⑧ 加项目级契约 | 接口/载荷会被多个程序集使用 → 放 `Contracts` | 是(登记) |
| ⑨ 加存档 section | 实现 `ISaveContributor`,key 稳定且唯一 | 否 |
| ⑩ 加分层 ID 域 | 新 ID struct + 目录类 + 3 行抽屉;新增条目只加一行 `const` | 否(各自目录内) |
| ⑪ 加关卡失败条件 | 自定义组件持 `LevelManager` 并调 `NotifyFailed()` | 否 |
| ⑫ 加空白点击层 | 实例化 `ClickScreen.prefab` 为 `UiRoot` 首个子物体,按需加 `UiClickAudio` | 否 |

---

## 7. Subsystem 设计

### 7.1 Player(`Basic2DPlatformer.Player`)

- `Motor`（Character 子系统）：拥有 Rigidbody2D，收意图（`SetMoveX` / `Jump` / `SetJumpHeld`），由物理事实推出 `MovementState`；`LockHorizontal` 供交互锁定；`Activate` / `Deactivate` 是唯一启停入口；不含死亡语义。
- `CharacterMovementConfigs`(SO,几何量):`moveSpeed`/`jumpHeight`/`timeToApex`/`fallGravityMult`/`coyoteTime`/`jumpBuffer`/`variableJumpCutoff`;重力由 `Physics2D.gravity` 与推导值决定。
- `CharacterAnimationView`(Character):观察 `StateChanged`/朝向驱动 Animator;`TryPlayInteract` 播交互动画,动画事件回调解锁;`PlayDeath` 供 `PlayerDeath` 调用;不查询 Animator。
- `PlayerController`(Player):读 Core 输入 → 调 Motor 意图 API。
- `PlayerInteraction`(Player):接收 `Interact`(F),处理 `IInteractable`(Contracts);成功后 `Motor.LockHorizontal(true)`。
- `PlayerAnimationAudio`(Player):动画事件 → Core 音频;移动音效在 `PlayerDeath.IsDead` 时跳过。
- `PlayerDeath`(Player):实现 Contracts 的 `IKillable`;`Kill()` 关物理、停运动、播死亡动画并广播 `PlayerDied`。
- 交互键 **F**。

### 7.2 Camera(`Basic2DPlatformer.Core` 用户)

- 包 `com.unity.cinemachine` **2.10.3**(3.x 与项目锁定的 Input System 1.7.0 不兼容);`Camera.prefab` = Brain + VirtualCamera(Follow=Player)+ Confiner2D。
- 每场景一个 `PolygonCollider2D` 边界(Trigger / Ignore Raycast),相机预制体默认不绑定范围。

### 7.3 Level(`Basic2DPlatformer.Level`)

- `ILevelGoal.cs`:通关条件被动契约(`bool Fulfilled` + `event Changed`)。
- `IKillable.cs`(Contracts):环境可杀死对象的命令契约(`void Kill()`);Level **不依赖** Character/Player。
- `EnvironmentFacade.cs`:环境唯一入口,暴露 `LevelManager`(放在 `SceneRoot`,与 `LevelManager` 同 GO)。
- `LevelGoal.cs`:终点 Trigger,**用 `Player` 标签**识别玩家;进入 → `Fulfilled=true` + 广播 `Changed`。
- `LevelGoalAndOperator.cs` / `LevelGoalOrOperator.cs`:组合条件(全部 / 任一达成),自身也实现 `ILevelGoal`。
- `LevelManager.cs`:场景本地门面,持有 `rootCondition`;满足时广播一次 `LevelCleared`。另有 `NotifyFailed()` / `LevelFailed` 供关卡自定义失败条件使用;通关与失败互斥,先到者胜。
- `Hazard.cs`:危险 Trigger → `GetComponentInParent<IKillable>()?.Kill()`。
- `LevelProgressTracker.cs`:与 `LevelManager` 同 GO,未连线时 `GetComponent`。
- `MovingPlatform.cs`:kinematic 刚体往返,把自身位移带给站在上面的刚体(最小实现)。
- `Portal.cs` / `PortalState.cs`:传送门;只有 `Active`(蓝)能作为入口发送,`Cooldown`(黄)/`Inactive`(红)不能发送但仍可接收。`exit` 是 `Portal` 类型;入口传送后对出口调用 `ActivatePortal(false)`(单向)或 `PortalCooldown()`(双向冷却)。

### 7.4 Game(`Basic2DPlatformer.Game`)

- `GameStateManager` / `GameState`：只维护 `Playing` / `Loading`;不施加时间或音频机制。
- `TimeScaleManager`：暂停与速度都按 owner 计数,是 `Time.timeScale` 的唯一写入者;`ResetAll()` 供场景切换。
- `AudioSettings` 位于 `Game/Setting/`。
- Progress：`GameProgress` / `LevelProgressStore` 在 Game；`LevelProgressTracker` 在 Level；`LevelCount = 1`。
- Save：`ISaveContributor` 在 Contracts；`SaveSystem` / `GameSaveStore` / `SaveEnvelope` 在 Game；`GameProgressContributor` 固定 key `progress`。
- 存档流程：扫描贡献者 → `CanCapture` → `Capture` → 临时文件 → 替换 `GameSave.json`；失败不触碰正式文件。
- 触发点：手动 Save、`GameProgress.LevelCompleted` 延迟一帧、退出。
- `lastLevelId` 来自 `LevelProgressTracker.levelId`；Continue 固定 `SampleLevel`。

### 7.5 UI(`Basic2DPlatformer.UI`)

- 面板统一挂 `UiPanel`（显隐 + 焦点还原 + 可选冻结 + 打开栈）；按钮行为由按钮组件携带数据完成。
- `UiPanel.TopOpen` 返回最后打开的面板；`freezeWhileOpen` 通过 `TimeScaleManager.PushPause/PopPause` 生效。
- `SettingsScreen`：音量存取与保存后应用。
- `NextLevelScreen`：订阅 `EnvironmentFacade.Level.LevelCleared`，打开通关面板。
- `DeathScreen`：订阅 `PlayerDeath.PlayerDied`，打开死亡面板，广播 `Confirmed`。
- `UiHotkeys`：空格开关暂停面板，Esc 关闭最上层面板（没有时打开暂停）。
- `UiPointer.IsPointerOverBlockingUi()`：区分“被真正的 UI 挡住”与“只命中全屏点击层”。
- `UiClickAudio`：挂在 `ClickScreen.prefab` 上时可播放空白点击音效；面板本身不含音效。
- `UI → Player`（死亡事件）与 `UI → Level`（环境门面）；不直接依赖 Character。
- 文案为英文占位；`SelectableAudio` 处理交互音效。
- 面板默认未激活；开关走 `UiPanel`。

### 7.6 数据型 SO

数据型 SO 用 `[CreateAssetMenu]`（字面量路径）；自动生成模板见 §8。

### 7.7 音频

- `Master.mixer` 暴露 `MasterVolume` / `OstVolume` / `SfxVolume`。
- `AudioManager.ApplyAudioSettings(float master, float ost, float sfx)` 按 dB 换算写入 mixer。
- `AudioManager.Start` 读取 `SettingBootstrap.Settings.Audio` 并应用；`SettingsScreen` 保存后调用 `ApplyAudioSettings` 立即生效。
- `AudioId` 是只序列化 `path` 的值类型，实现 `LayeredId` 的 `ILayeredId`；`AudioCatalog` 用 `public const string` 声明合法路径。
- `AudioManager.BuildClipIndex` 在 `Awake` 建立 `Dictionary<AudioId, AudioClipData>`；重复/空 ID 会告警。
- 新增音效 = 建 `AudioClipData` + 在 `AudioCatalog` 加一行 `const` + 拖进 `AudioManagerConfigs`；需求见 [Subsystems/Audio.md](Subsystems/Audio.md)。
- `AudioBuilder` 的 `WithVolume/WithPitch` 只覆盖本次播放；`WithRandomPitch` 以 1 为基准。
- `startupOst` + `playOnStart` 在 `Start` 播放启动音乐，替代硬编码。

---

## 8. 编辑器工具窗口（`Assets/Editor`）

单一窗口 `Tools/Basic2DPlatformer/Project`:

- 场景 / 关卡：列出全部 `SceneDefinition`、注册 Build Settings、打开场景、校验空引用 / 场景缺失 / 未注册 / 重复 Id / asmdef 依赖边 / UI 必填引用。
- 输入：增删 action / binding，写回 `.inputactions` 并触发 `PlayerControls.cs` 重生成。
- Tag / Layer：查看与新增。
- SO 生成器：生成 `XxxConfigs` 脚本、`[CreateAssetMenu]` 与默认资产。

---

## 9. 未决 / TODO

- TODO: 关卡顺序与 `lastLevelId` 解析;当前 Continue 固定进入 `SampleLevel`。
- TODO: Setting 原子写入。
- TODO: 其余子系统的测试程序集(Game 存档已有 EditMode 测试)。
