---
type: note
tags:

- correspondence
  created: 2026-10-25
  modified: 2026-10-26

---

# 1    并入Tower Defence

@grill-me 我把这个模板fork了一份TowerDefence作业, 添加新feature. 需要你比对并拷问: 哪些能被整合进Basic 2D Platformer Template?

## 1.1    Audio子系统

新的`AudioId`实践. 需要写自定义Inspector脚本. 能generalize吗?

## 1.2    Ui子系统

新的空白Passthrough幕布, 可以处理空白处点击的音效等. 怎么整合?

## 1.3    其他子系统

还有哪些功能是值得整合的?

# 2    Entry 1

## Agent · Round 1

**比对结论(事实基线)**

- 模板已具备完整音频运行时(`AudioManager` / `AudioBuilder` / `AudioRegistry` / `AudioEmitter` / `ComponentPool` 池化 / 冻结门 / 实例上限 / 抢占 / mixer / 持久化)。TD 的音频增量只有:`AudioId` 路径化、变体与抖动、`cueIndex`、两个编辑器脚本、一处覆盖参数 bug 的修复。
- "空白 Passthrough 幕布"不是 Canvas,而是 `UiRoot` 的第一个子物体:全屏透明 `Image`(`raycastTarget=1`) + 空标记 `PassThroughUi`(13 行零成员) + `ScreenClickAudio`。`PassThroughUi` 的唯一读取者在 TD 的 `BuildController`,模板内零消费者。
- fork 不是超集:TD 删除了 `Character` / `Player` / Cinemachine / `Portal` / `MovingPlatform` / `Hazard` / `SceneDefinition`;45 个同名文件全部不同,只能挑拣移植。
- 模板在编辑器工具窗口、Cinemachine、`ILevelGoal` 组合器、`SceneDefinition` Inspector 上强于 TD,不可被反向覆盖。
- 模板现有 bug:`AudioBuilder.WithVolume` / `WithPitch` / `WithRandomPitch` 是死参数(`AudioEmitter.Play()` 无条件回写 cue 默认值)。
- 全屏点击层会使 `EventSystem.current.IsPointerOverGameObject()` 恒真,属模板级契约。

**问题**

❓ **Q1** — 模板身份边界:横版平台专用(A)/ 平台优先 + 可选 2.5D 模块(B)/ 多流派通用模板(C)。
➡️ A。

❓ **Q2** — 回灌机制:单向挑拣(A)/ 抽共享本地 UPM 包(B)/ 只采纳设计不取代码(C)。
➡️ A,但移植时按模板命名空间与 asmdef 重写,不复制粘贴。

❓ **Q3** — 兼容性政策:现在允许一次性破坏 + 迁移脚本(A)/ 只允许增量(B)/ 攒到 v2(C)。
➡️ A。

❓ **Q4** — `AudioId` 模型:路径 struct + 静态 `AudioCatalog`(A)/ 路径 struct + 自动目录(B)/ 直接引用 `AudioClipData` SO(C)/ 保持枚举(D)。
➡️ B。A 的致命缺陷是"加一个音效必须改 `AudioCatalog.cs` 共享文件",违反模板"新增内容不碰共享代码"的承诺。

❓ **Q5** — 点击层整合程度:原样并入(A)/ opt-in 预制体 + 统一 API(B)/ 不要全屏 Image(C)。
➡️ B。

❓ **Q6** — 交付物:只出清单(A)/ 直接改模板(B)/ 先做试点(C)。
➡️ C。

## 回复

从现在开始, 每轮grill结束后, 你需要将Agent侧问题补入这个Correspondence归档记录, 我的回复原文不动, 只做插入: "F:\Games\Unity\Unity Projects\Basic Unity 2D Template\Assets\Docs\并入Tower Defence项目中的新功能.md"

1. A
2. A
3. A
4. 我认为应该将这个功能拆成一个`Layered Identifier`包: "无集中化文件" + "扫描成员" + "底层string/路径" +"Editor分层支持". 服务使用者自定义标签并继承基类, 扫描时只扫描这个标签类.
5. 只加空`ClickScreen.prefab`, 不决定点击后具体行为, 它取决于具体项目. 可以先做一个点击音效功能模块, 它较为泛用
6. 采纳

# 3    Entry 2

### Agent · Round 2

**新增事实**

- C# 的 `struct` 不能被继承。"服务使用者自定义标签并继承基类"只能落在**目录类**上,ID 本身无法继承;若 ID 保持值语义,每域必须显式注册自己的 drawer。
- 现有三个 `com.grignardreagent.*` 包在 `manifest.json` 里都是**无 tag 的 git URL**,会跟默认分支漂移。
- TD 的 `AudioCatalog` 是运行时被引用的(`WaveController` 直接读 `AudioCatalog.IncomingWave`),目录类必须落在运行时程序集,只有扫描与抽屉属于 Editor。

**问题**

❓ **Q1** — 包的分发形态:独立 GitHub 仓库 + tag(A)/ 模板内嵌 `Packages/`(B)/ 先放 `Assets/Core`(C)。
➡️ A。与现有三个包同一套心智模型;TD 可复用同一份;包内 `Editor` asmdef 天然隔离 `UnityEditor`。

❓ **Q2** — ID 类型与目录的绑定:单一 `LayeredId` + 每字段特性(A)/ 每域自定义 ID struct + 目录标签 + 每域 3 行 drawer(B)/ `[Serializable] class` 基类真继承(C)。
➡️ B。ID 的正确语义是值语义;类型安全能挡住跨域误填;样板可由现有 SO 生成器代劳。

❓ **Q3** — "扫描成员"与"Editor 分层"语法:纯路径分段(A)/ 嵌套静态类(B)/ 混合(C)。
➡️ A。字符串是唯一真相,重命名成员不改 ID,且与 TD 现有资产格式兼容。

❓ **Q4** — 校验边界:只做 drawer 内联(A)/ 加目录 Inspector(B)/ 再加全工程扫描(C)。
➡️ B。重复 ID 不能拖到运行时才炸;C 代价高,可留作后续独立工具。`AudioManagerConfigsEditor` 留在 Audio,不进包。

❓ **Q5** — ClickScreen 与点击音效模块:预制体内容 / 触发方式(catcher 上的 `IPointerDownHandler` vs 全局 router)/ 是否提供 `UiPointer.IsPointerOverInteractiveUi()` 与场景接线。
➡️ 内容全选;触发选 catcher;a 提供 helper 但不接线。

❓ **Q6** — `Health` / 伤害契约的程序集归属:新建 `Combat`(A)/ 塞 `Level`(B)/ 全塞 `Contracts`(C)。
➡️ A。

❓ **Q7** — `LevelCatalog` 与"去中心化注册"的裁决:收中央目录(A)/ `SceneDefinition` + `Order` + 工具生成只读目录(B)/ 不关卡序(C)。
➡️ B。

### 用户回复

- 独立`GitHub`库, 先在`Assets/LayeredId`下开发, 稳定后再抽包
- 展开讲讲B
- A
- B
- 采纳
- 不加, 并非模版内容
- C

# 4    Entry 3

### Agent · Round 3

**范围收缩(由上轮答案推出)**

- Health / `IDamageable` / `DamageInfo` / `ISlowable` / `Hitscan` 全部出局,不建 `Combat` 程序集。
- `LevelCatalog` / `LevelDefinition` / `LevelProgressTracker` 改动出局,关卡系统保持去中心化,`LevelCount = 1` 不动。
- 4 个 TD 测试只剩 `TimeScale_PauseOverridesSpeed` 值得搬。

**问题**

❓ **Q1** — ID 结构体的序列化形状:字段名约定 `path`(A)/ 包装包内 `LayeredId value`(B)/ 特性指定子字段名(C)。
➡️ A。YAML 扁平、与 TD 资产一致、对 Smart Merge 友好;约定由生成器强制。

❓ **Q2** — 目录扫描与校验默认值:只收 `public const string`、Ordinal 大小写敏感、重复即 Error、Inspector 按 `/` 分组树、不做全工程扫描。
➡️ 全部按默认值。

❓ **Q3** — `LayeredId` 的 asmdef 细节:`Assets/LayeredId/{Runtime,Editor}`、`GrignardReagent.LayeredId` / `.Editor`、`autoReferenced = false`、抽包后 `com.grignardreagent.layeredid` + `v0.1.0`。
➡️ 全部采纳。

❓ **Q4** — `LevelFailed` 去留:加(A)/ 不加(B)/ 由 `LevelManager` 自动转发 `PlayerDied`(C)。
➡️ A。让"输"成为 Level 层的一等概念;C 会让 Level 依赖 Player,破坏单向边界。

❓ **Q5** — 键盘暂停 / Esc 路由:收 `InputRouter` + `UiPanel` 自注册栈(A)/ 只收 `Pause` 热键(B)/ 不碰(C)。
➡️ A。替换 TD 那份按 GameObject 名字 switch 的实现。

❓ **Q6** — `TimeScaleManager` 收到什么程度:全量 + 允许慢动作(A)/ 只收 pause owner(B)/ 不碰(C)。
➡️ A。移植成本是"复制 + 改两个常量";`max(1, speed)` 是塔防快进假设,会堵死平台模板的慢动作。

### 用户回复

1. A. 需要你讲清使用这个包从新建Id到使用的全流程
  
2. 采纳
  
3. 采纳
  
4. 加
  
5. A
  
6. 为什么需要一个专门的`TimeScaleManager`? 帮我理清

# 5    Entry 4

### Agent · Round 4

**新增事实**

- `Time.timeScale` 当前只有 `GameStateManager` 一个写入者;`FreezeCoordinator` 另有一套 owner 集合且只服务 `UiPanel`;`GameStateManager.Freeze/Resume` 是 public,任何代码都能绕过计数。
- `AudioListener.pause` 被写两次(`GameStateManager` 与 `AudioManager.HandleGameStateChanged`)。
- `Resume()` 只判断 `Current == Playing`,不校验自己撤销的是哪次冻结;`Loading` 期间面板关闭会把状态改回 `Playing`,随后 `ExitLoading()` 直接返回。
- `SceneSwitchManager.RequestSwitch` 从不重置时间/冻结。
- `PlayerInteraction` / `PlayerAnimationAudio` 用 `Time.timeScale > 0` 当作"是否暂停"。

**问题**

❓ **Q1** — `TimeScaleManager` 形态:三件套保留(A)/ 合并去重,删 `FreezeCoordinator` 与 `GameState.Freezed`(B)/ 只修 bug 不新增类(C)。
➡️ B。A 会把"两套真相"固化成模板契约;C 治不了非 UI 来源与速度通道。

❓ **Q2** — Audio 首轮范围:ID 迁移 + `cueIndex` + 配置编辑器 + bug 修复 + 启动 OST 序列化(A)/ 再加变体与抖动(B)/ 只做 ID 迁移 + bug 修复(C)。
➡️ A。变体/抖动是纯增量字段,晚加无需迁移;启动 OST 不做成序列化字段等于把音频链留半截。

❓ **Q3** — enum → 路径迁移:手工 + 迁移说明(A)/ 编辑器脚本(B)/ 兼容 shim(C)。
➡️ A。脚本对下游 fork 无效(他们自定义的枚举成员没有路径可映射);shim 会永久留历史包袱。

❓ **Q4** — 生成器时机:现在加 Identifier 页(A)/ 试点先手工三文件,生成器列为 P2(B)/ 不做(C)。
➡️ B。先确认 ID 形状不再变,否则生成器会跟着返工。

❓ **Q5** — 试点验收清单(11 条,含"从暂停状态切场景后 `timeScale == 1`"这一硬指标)。
➡️ 全部保留;建议动手前先按现状复现第 9 条。

❓ **Q6** — 执行顺序:包骨架 → Audio 迁移 → ClickScreen → TimeScale → 输入路由 → `LevelFailed` → 文档 → 抽包。
➡️ 照此顺序。

### 用户回复

- B
  
- A
  
- A
  
- B
  
- 保留
  
- 按此顺序

# 6    Entry 5

### Agent · Round 5

**问题**

❓ **Q1** — `GameStateManager` 终局:保留类,只删 `Freezed` 与 `Changed`(A)/ 整类删除,`UiPanel` 改判 `SceneSwitch.IsSwitching`(B)/ 保留三态,`Freezed` 变派生(C)。
➡️ B。

❓ **Q2** — `LevelFailed` 的示例生产者:只作文档化扩展点 + 测试(A)/ 加 `FailZone` 并在 `SampleLevel` 演示(B)/ `DeathScreen` 改订阅 `LevelFailed`(C)。
➡️ A。示例里已有 `Hazard` 一条"环境杀死玩家",再加一条会制造两种失败语义;C 会让 Player 依赖 Level。

❓ **Q3** — 三个命名/语义默认值:`UiPointer.IsPointerOverBlockingUi()` / `UiHotkeys` + "最后打开的面板"栈 / ID 路径 `Sfx/UI/MouseClick`、`Sfx/Generic`、`Ost/Generic` + `startupOst` + `playOnStart`。
➡️ 全部采纳。

❓ **Q4** — 包自身测试与文档:带最小 EditMode 测试 + README + CHANGELOG(A)/ 只带 README(B)/ 都放模板(C)。
➡️ A。

❓ **Q5** — 实现方式:端到端 + 每阶段跑测试 + 每阶段提交,并顺手修三个 bug(A)/ 只做清单内的事(B)/ 只改不提交(C)。
➡️ A。三个 bug:`SoundHandle.IsFinished` 恒 false、`AudioEmitter` 缺 clip 时泄漏池条目、`MainMenu` 的 `UiRoot` 缺 `CanvasScaler`。

❓ **Q6** — "稳定后抽包"的判定:验收全过即抽(A)/ 等第二个消费方接入再抽(B)/ 不抽(C)。
➡️ B。

### 用户回复

- A. 保留, `GameState`语义不只有是否冻结
  
- A
  
- 全部采纳
  
- A
  
- A
  
- B 

# 7    Phase 1 验收

> 提交:`1ab696c feat(layered-id): 添加分层标识包骨架与 EditMode 测试`

### 交付物

| 文件 | 说明 |
| --- | --- |
| `Assets/LayeredId/Runtime/GrignardReagent.LayeredId.asmdef` | `autoReferenced = false`;`noEngineReferences = true`(运行时不依赖 UnityEngine) |
| `Assets/LayeredId/Runtime/ILayeredId.cs` | `Path` / `IsValid` 值契约 |
| `Assets/LayeredId/Runtime/LayeredIdCatalog.cs` | 反射 `DeclaredOnly` 的 `public const string`,生成排序路径、`HashSet` 查找、成员映射与校验问题;按 `Type` 缓存 |
| `Assets/LayeredId/Runtime/LayeredIdCatalogAttribute.cs` | 目录发现标签 |
| `Assets/LayeredId/Runtime/LayeredIdMember.cs` | 成员名 ↔ 路径 |
| `Assets/LayeredId/Runtime/LayeredIdIssue.cs` | 校验问题载荷 |
| `Assets/LayeredId/Editor/GrignardReagent.LayeredId.Editor.asmdef` | Editor 平台限定,引用 Runtime |
| `Assets/LayeredId/Editor/LayeredIdDrawer.cs` | 读 `path` 子字段;`AdvancedDropdown` 按 `/` 分层;不在目录时画红底线 |
| `Assets/LayeredId/Editor/LayeredIdCatalogEditor.cs` | 按 `/` 缩进展示成员;空路径/重复路径/空目录报 Error;未打标告警 |
| `Assets/LayeredId/Tests/EditMode/LayeredIdCatalogTests.cs` | 5 条:排序、Ordinal 大小写、空路径、重复路径、空目录 |

### 验收

- 编译:通过(无 error / warning)。
- 测试:`GrignardReagent.LayeredId.Tests` 5/5 通过;全量 EditMode 14/14 通过。
- 文件命名与目录为抽包形态:`Runtime/` + `Editor/` + `Tests/` 可整体搬迁。

### 已知边界

- 模板的 `Basic2DPlatformer.Editor` 尚未引用 `GrignardReagent.LayeredId.Editor`;Phase 2 添加 `AudioIdDrawer` 时才需要。
- `LayeredIdCatalogEditor` 目前没有实际目录可展示(第一个消费者是 Phase 2 的 `AudioCatalog`)。
- 生成器(Identifier 页)、包 README / CHANGELOG 按决策留到抽包前。

# 8    Phase 2 验收

> 提交:`1bdbde0 feat(audio): AudioId 路径化并接入 LayeredId 包`、`ff7d120 chore(audio): 迁移 AudioId 资产/场景/预制体字段`、`627203d fix(layered-id): 目录校验改为编辑器窗口`

### 代码

- `AudioId`:enum → `[Serializable] struct` + `ILayeredId`,序列化单个 `path`,Ordinal 相等性,`None` / `From` / `ToString` 保持。
- 新增 `AudioCatalog`:`[LayeredIdCatalog]`,`Sfx/UI/MouseClick`、`Sfx/Generic`、`Ost/Generic`。
- `AudioBuilder`:`volumeSet` / `pitchSet` 标记,只把真正设置过的覆盖值传给 manager;`WithRandomPitch` 以 1 为基准。
- `AudioEmitter`:
  - `Play(float?, float?)`,未覆盖时回落 clip 的 baseline;
  - 缺 clip 时 `Complete(false)` 通知结束,修复池租约泄漏;
  - 自然播完走 `StopNaturally()` → `Complete(true)`,修复 `IsFinished` 恒假;
  - `ResetEmitter` 不再清 `isFinished`,由下一次 `Play` 重置。
- `AudioManager`:`Dictionary<AudioId, AudioClipData>` 索引替代 `FirstOrDefault`;重复/空 ID 在 Awake 告警;新增 `startupOst` + `playOnStart`,替换硬编码。
- `AudioManagerConfigs`:移除 `TryGetClip`,暴露 `Audios`。
- 新增 `Assets/Editor/AudioIdDrawer.cs`(3 行绑定)、`Assets/Editor/AudioManagerConfigsEditor.cs`(分组/去重/空/目录外校验、增删)。
- 新增 `Assets/Tests/EditMode/AudioIdTests.cs`(4 条)。

### 数据迁移(84 处 + 3 个资产)

| 文件 | 字段数 |
| --- | --- |
| `MainMenu.unity` | 20(`downSfx` 2 处 → `Sfx/UI/MouseClick`) |
| `SampleLevel.unity` | 30 |
| `Final.unity` | 5 |
| `SettingsPanel.prefab` | 25 |
| `Player.prefab` | 3 |
| `Core.prefab` | 1(`debugAudioRequest` → `Ost/Generic`) |
| `Sounds/SO/*.asset` | 3(`audioId` → 路径) |

### 验收

- 编译:无 error / warning。
- EditMode:18/18 通过(新增 4 条音频 ID 测试)。
- PlayMode 冒烟(进入实际游戏场景):
  - `CoreFacade.Instance.Audio` 可用,`GameState = Playing`;
  - 启动 OST 自动播放(`Ost/Generic`,`loop=1`);
  - `CreateBuilder().WithVolume(0.5f).WithPitch(1.25f).Play(MouseClick)` → 实际 `AudioSource.volume = 0.5`、`pitch = 1.25`,覆盖参数修复生效;
  - 冻结期间请求被拒(`Request ... refused: the game is frozen`),冻结门仍有效;
  - 点击音自然播完后有 `AudioEmitter is released.`,池租约归还正常。

### 偏差与遗留

- **目录 Inspector 改为编辑器窗口**。`LayeredIdCatalog` 是普通类,不是 `UnityEngine.Object`,`[CustomEditor]` 无法挂载。改为 `Tools/GrignardReagent/Layered Id Catalogs`:按 `[LayeredIdCatalog]` 扫描,列出成员与问题,并对未打标的目录告警。验收清单第 2 条的"目录 Inspector"由该窗口承接。
- `AudioListener.pause` 仍由 `GameStateManager` 与 `AudioManager` 双重写入;Phase 4 合并 `TimeScaleManager` 时收敛为单一归属。
- `variants` / `RandomMode` / `PitchJitter` 按决策延后,均为 `AudioClipData` 的纯增量字段。
- `IsFinished` 通过"自然播完后释放池"间接验证;句柄事件本身由代码路径保证,未写 PlayMode 测试。
- `AudioIdDrawer` / `AudioManagerConfigsEditor` 的实际观感需在 Inspector 中人工确认。

# 9    Phase 3 验收

> 提交:`26ddaca feat(ui): 添加可选的空白点击层与点击音效模块`

### 交付物

| 文件 | 说明 |
| --- | --- |
| `Assets/UI/Prefabs/ClickScreen.prefab` | 全屏拉伸的透明 `Image`(`raycastTarget=1`、`maskable=0`、alpha 0) + `PassThroughUi`;**不含**任何音效组件 |
| `Assets/Core/Scripts/Visuals/PassThroughUi.cs` | 空标记,供世界输入判定忽略该层 |
| `Assets/UI/Scripts/UiPointer.cs` | `IsPointerOverBlockingUi()`:射线命中不在 `PassThroughUi` 之下即视为阻挡 |
| `Assets/UI/Scripts/ScriptComponents/UiClickAudio.cs` | 左键 `IPointerDown` → 序列化 cue;`AllowWhileFrozen + SurviveFreeze` |

### 接线方式(不写进场景)

1. 把 `ClickScreen.prefab` 拖到 `UiRoot` 下,并设为**第一个子物体**(保证按钮优先消费点击)。
2. 需要点击音效时,给该实例加 `UiClickAudio`,`clickSfx` 选 `Sfx/UI/MouseClick`。

世界指针玩法判断遮挡时使用 `UiPointer.IsPointerOverBlockingUi()`,不要用 `EventSystem.current.IsPointerOverGameObject()`——后者会被全屏点击层永久置真。

### 验收

- 编译无 error / warning。
- EditMode:18/18 通过。
- PlayMode 冒烟(临时实例化到 `UiRoot` 首位):
  - 屏幕中心 `blocking@center = False`(只命中点击层);
  - `MenuButton` 位置 `blocking@menuButton = True`(真实 UI 阻挡);
  - 模拟左键 `pointerDown` → `clickPlaying = True`(`click` clip 实际发声);
  - 销毁实例后无残留。

### 边界

- 预制体为 opt-in,未加入任何示例场景。
- `IsPointerOverBlockingUi()` 依赖 `CoreFacade` 提供的指针位置;无 Core 时返回 false。
- `UiClickAudio` 未做双重播放防护:它只应挂在最低层的点击层上;挂在按钮上会与 `SelectableAudio` 叠加。

# 10    Phase 4 验收

> 提交:`ab75d04 refactor(time): 合并暂停与速度到 TimeScaleManager`

### 代码

- 新增 `Game/Scripts/TimeScale/TimeScaleManager.cs`:
  - `PushPause/PopPause` 按 owner 计数;`IsPaused` 读取时清理已销毁 owner;
  - `SetSpeed/ClearSpeed` 按 owner 记录,多 owner **相乘**并钳制 `0.1..4`;
  - `EffectiveScale = IsPaused ? 0 : speed`,是 `Time.timeScale` 的唯一取值来源;
  - `ResetAll()` 清空请求并恢复 1 倍;`Changed` 在暂停/速度实际变化时触发。
- 删除 `FreezeCoordinator`。
- `GameStateManager` 只保留 `Playing` / `Loading` 与 `EnterLoading` / `ExitLoading`;移除 `Freezed`、`Changed`、`Freeze`、`Resume`、`timeScaleBeforeFreeze`。
- `UiPanel` 的 `freezeWhileOpen` 改为 `TimeScaleManager.PushPause(this)` / `PopPause(this)`。
- `AudioManager` 的 `AudioListener.pause` 改为订阅 `TimeScaleManager.Changed` 并读取 `IsPaused`;冻结门改判 `IsPaused`。
- `SceneSwitchManager.RequestSwitch` 在发起加载前调用 `TimeScaleManager.ResetAll()`。

### 验收

- 编译无 error / warning。
- EditMode:23/23 通过(新增 5 条:`PauseOverridesSpeed`、`TwoOwnersRequireBothToRelease`、`SpeedRequestsMultiply`、`DestroyedOwnerIsPurged`、`ResetAllRestoresScale`)。
- PlayMode:
  - 打开 `PausePanel` → `IsPaused=True`、`Time.timeScale=0`、`AudioListener.pause=True`、`GameState=Playing`;
  - 手动 `PushPause` 后 `RequestSwitch(MainMenu)`:`op != null`、`IsSwitching=True`,同一帧 `paused=False / scale=1 / audioPause=False`;
  - 加载完成后:`scene=MainMenu`、`paused=False`、`scale=1`、`audioPause=False`、`state=Playing`、`isSwitching=False`。

### 说明

- 旧的 `Resume()` 不再存在,因此"加载中面板关闭把 `Loading` 改回 `Playing`"的路径被结构性移除;`ResetAll` 也不会回写 `GameState`。
- 速度采用乘积而非 `max`:单个 `0.5` 请求就是慢动作,多个请求相乘;`max` 会让单个慢动作请求被 1 倍基线吞掉。
- `SurviveFreeze` / `DefaultSurviveFreeze` / `AllowWhileFrozen` 的 API 名保留;语义现在对应 `TimeScaleManager.IsPaused`,重命名留待需要时统一做。
- `AudioListener.pause` 现在只有 `AudioManager` 一个写入者;若场景没有 AudioManager,则不会暂停音频。
- 场景切换时仍会出现 "Another TimerRunner already exists" 与 "A second Core was found" 两条既有告警(新场景的 Core 被持久 Core 销毁),与本次改动无关。

# 11    Phase 5 验收

> 提交:`d0722b8 feat(ui): 添加键盘暂停与逐层关闭面板`

### 输入

- `PlayerControls.inputactions` 新增两个按钮动作并重新生成 `PlayerControls.cs`:
  - `Pause` → `<Keyboard>/space`
  - `Cancel` → `<Keyboard>/escape`
- 空格原本未绑定(跳跃是 `Up` 的 `upArrow` / `W`),因此不存在冲突。
- `InputManager` 新增 `PausePressed` / `CancelPressed` 事件,在 `OnEnable` 订阅、`OnDisable` 退订。

### UI

- `UiPanel` 增加静态打开栈:
  - `OnEnable` 把自己移到栈顶,`OnDisable` 移除;
  - `public static UiPanel TopOpen` 返回最后打开的面板。
- 新增 `UiHotkeys`(挂在 `SampleLevel` 的 `UiRoot`,持有 `pausePanel` 引用):
  - 空格:无面板时打开暂停;栈顶是暂停面板时关闭;栈顶是其他面板时不动作。
  - Esc:关闭栈顶面板;无面板时打开暂停。
  - Core 尚未就绪时逐帧重试绑定,直到拿到 `InputManager`。

### 验收

- 编译无 error / warning;EditMode 23/23。
- PlayMode:
  - `UiHotkeys` 绑定成功(`pauseSubscribers=1`);
  - 调用 `InputManager` 的暂停回调 → `PausePanel` 打开、`IsPaused=True`、`timeScale=0`;
  - 再次暂停 → 关闭并恢复 `scale=1`;
  - 先打开 `SettingsPanel`,再按暂停 → 栈顶仍是 `SettingsPanel`,不动作;
  - Esc → 关闭 `SettingsPanel`;无面板时 Esc → 打开 `PausePanel`;
  - 面板栈与 `TimeScaleManager` 的 owner 计数同步。

### 边界

- 真实键盘注入未跑通:编辑器窗口失焦时 Input System 不处理注入的设备状态。绑定路径已在 `.inputactions` 与生成的 `PlayerControls` 中核对,`InputManager` 事件到 `UiHotkeys` 的链路用回调实测。
- 只有 `SampleLevel` 有 `PausePanel`,因此只在该场景接线;`MainMenu` / `Final` 未挂 `UiHotkeys`。
- `UiHotkeys` 用 `FindObjectOfType<InputManager>()` 兜底,因此即使 `CoreFacade` 尚未初始化也能绑定;不依赖脚本执行顺序。
   
   ### 回复
   
   