# 拓展指南

> 每个官方拓展点怎么做。架构背景见 [ARCHITECTURE.md](ARCHITECTURE.md)。

## ① 加场景 / 关卡

1. 新建场景,搭好 Tilemap 地面、`Hazard`、Core 等。
2. 放通关条件并接入 `LevelManager`(场景本地):
   - `LevelManager.rootCondition` 指向根条件(需实现 `ILevelGoal`)。
   - 单条件: 放 `LevelGoal`(触发器),把它拖到 `rootCondition`。
   - 多条件: 放 `LevelGoalAndOperator`(全部达成)或 `LevelGoalOrOperator`(任一达成),把各 `ILevelGoal` 拖进其 `goals`,再把组合器拖到 `rootCondition`(收集型: 给 `LevelGoal` 勾选 `disableWhenFulfilled`)。
   - 自定义条件: 新增组件实现 `ILevelGoal`(`bool Fulfilled` + `event Changed`),例如"击杀计数"。
3. 在场景根(与 `LevelManager` 同 GO)挂 `EnvironmentFacade`;UI 的通关面板经它取 `Level`。需要死亡面板时,场景里必须有带 `PlayerDeath` 的 Player。
4. 在场景同目录新建 `SceneDefinition` 资产(右键 `Create/Basic2DPlatformer/Scene Definition`),在 Inspector 里拖入该场景;`Id` 会自动填为场景名。
5. 打开 `Tools/Basic2DPlatformer/Project`,点 `Register All in Build Settings`;`Validate` 应为通过。
6. 需要从别处进入它时,在调用方(如 `SceneSwitchButton.target`)引用这个 `SceneDefinition` 资产。

> 全程不修改任何共享代码文件;新增的是"场景 + 组件 + 一份新资产"。

## ② 加可交互物

1. 在任意玩法程序集新增组件,实现 `IInteractable`(引用 `Basic2DPlatformer.Contracts`):
   `bool CanInteract { get; }`、`event Action<IInteractable> OnInteracted;`、`void Interact(InteractionDetails details)`。
2. `Interact` 中先按 `CanInteract` 设门,再施加效果,最后触发 `OnInteracted`。
3. 挂在场景对象上,给一个 Collider2D(Trigger 可被交互半径命中)。

## ③ 加 UI 屏幕

1. 面板本身挂 `UiPanel`: 它负责显隐、焦点还原与可选冻结;不要直接 `SetActive`,也不要写 `Time.timeScale`。
2. 需要订阅游戏事件时,在常驻的 Canvas 根(`UiRoot`)下新增脚本;按钮行为优先复用 `SceneSwitchButton` / `OpenPanelButton` / `QuitGameButton` / `NewGameButton` / `ContinueButton` / `SaveButton`,不要在屏幕脚本里写按钮方法。
3. 面板默认未激活;打开用 `UiPanel.Open()`,关闭用 `UiPanel.Close()`(可接到按钮的持久监听)。
4. `UiPanel` 自带打开栈:`TopOpen` 是最上层,`UiHotkeys` 的 Esc 按该顺序关闭。
5. 新增屏幕 = 新脚本 + 场景内面板/预制体,不修改共享脚本。

## ④ 加输入动作

- 用工具窗口 `Tools/Basic2DPlatformer/Project` 的 **Input** 标签页增删 action/binding；它写回 `.inputactions` 并触发 `PlayerControls.cs` 重生成。
- 再在 `Core/Scripts/Persistent/InputManager.cs` 上**手写**成对的事件转发（工具不改 Core 代码）。
- 现有动作：`PrimaryPress`、`PointerPosition`、`Left/Right/Up/Down`、`MovementInput`、`Interact`、`Pause`(空格)、`Cancel`(Esc)。
- `.inputactions` 是共享文件,改动需知会;`PlayerControls.cs` 是生成物,不手改。

## ⑤ 加玩家能力

1. 在 `Assets/Player/Scripts` 新增组件(读 `CoreFacade.Instance.Input`、`GameStateManager`)。
2. 用 Player 预制体的 **Variant** 挂载,不直接改 `Player.prefab`。
3. 不把玩法规则写进 Env/Core;组件只做自己的能力。
4. 死亡语义不要写进 `Motor`:环境调用 `IKillable.Kill()`,由 `PlayerDeath` 关物理/停运动/播动画并广播 `PlayerDied`。

## ⑥ 加数据型 SO

- 用工具窗口的 **SO** 标签页生成 `XxxConfigs` 脚本(含 `[CreateAssetMenu]`)与默认资产;或手写 `[CreateAssetMenu]`(字面量路径)。
- 数据资产只放数据与查找方法,不放逻辑。

## ⑦ 加新 Subsystem

1. 新建 `Assets/<Name>/Scripts/` 与 asmdef(`Basic2DPlatformer.<Name>`),按 [ARCHITECTURE.md](ARCHITECTURE.md) 的依赖方向填引用。
2. 若被更上层使用,更新上层 asmdef 的 references。
3. 在 `Assets/Docs/Subsystems/<Name>.md` 登记职责与公共 API,并在 `ARCHITECTURE.md` 的表里加一行。

## ⑧ 加项目级契约

当接口或数据载荷会被**多个互不相关的程序集**实现或消费时:

1. 放进 `Assets/Contracts/Scripts`(`Basic2DPlatformer.Contracts`,零项目依赖)。
2. 接口与其载荷(`struct`/`class`/`enum`)放同一层,不要拆 `Data`。
3. 上层程序集引用 `Contracts`;`Contracts` **不得**反向引用任何 `Basic2DPlatformer.*`。
4. 只在单一程序集内使用、或只被上层以具体类型使用的类型,留在原子系统。
5. 改完运行 `Tools/Basic2DPlatformer/Project` 的 `Validate`(会校验 asmdef 边)。

> 典型:`IInteractable` / `InteractionDetails` / `IKillable`。

## ⑨ 加存档 section

1. 在玩法程序集新增组件,实现 `Basic2DPlatformer.Contracts` 的 `ISaveContributor`(零项目依赖,无需引用 Game)。
2. `Key` 必须**稳定且唯一**(例如 `"level-a/portal-1"`),不要用 `GetInstanceID()`;同一 key 出现两次时存档会整体失败。
3. `CanCapture()` 的语义是"当前状态可表示",**不是**"没有在动"。菜单冻结时动画会停在半路;若无法表示中间态就返回 false,并接受 Save 失败。
4. `Capture()` 只读自身状态,自行序列化;`Restore(json)` 在对象自身 `Start` 之后由 `SaveService` 调用,失败时保留 pending 以便下次场景加载重试。
5. 场景贡献者由 `SaveService` 在 `sceneLoaded` 后等一帧,用 `FindObjectsByType(..., FindObjectsInactive.Include)` 扫描当前活动场景;`DontDestroyOnLoad` 与 additive 场景不参与。
6. 信封固定为 `version + sections[]`;新增 section 不改变信封。新 section 的 key 一旦发布就不要改名,内部 schema 由贡献者自己维护。
7. 改完跑 `Assets/Tests/EditMode` 的 `SaveSystemTests`,并补一条覆盖 `CanCapture` 失败/恢复的测试。

## ⑩ 加分层 ID 域

> 完整流程与规则见 `Packages/com.grignardreagent.layeredid/README.md`。

1. 在目标子系统新增 ID 值类型:`[Serializable] struct XxxId : ILayeredId`,序列化字段必须叫 `path`。
2. 新增目录类:`[LayeredIdCatalog] public sealed class XxxCatalog : LayeredIdCatalog`,只放 `public const string`。
3. 在 `Assets/Editor` 新增 3 行抽屉:
   `[CustomPropertyDrawer(typeof(XxxId))] class XxxIdDrawer : LayeredIdDrawer { protected override Type CatalogType => typeof(XxxCatalog); }`。
4. asmdef:ID 所属程序集引用 `GrignardReagent.LayeredId`;抽屉所属 `Basic2DPlatformer.Editor` 已引用 `GrignardReagent.LayeredId.Editor`。
5. 新增条目只加一行 `const`;路径发布后视为冻结(改路径等于换 ID)。
6. 打开 `Tools/GrignardReagent/Layered Id Catalogs` 校验重复与空路径。

## ⑪ 加关卡失败条件

1. 在关卡内新增组件,拿到 `LevelManager`(序列化引用或 `FindObjectOfType`)。
2. 条件成立时调用 `NotifyFailed()`;重复调用、以及"已通关后调用"都是空操作。
3. 失败表现订阅 `LevelManager.LevelFailed`;不要在关卡脚本里直接开面板。
4. 纯失败关卡仍需要一个 `ILevelGoal` 占位给 `rootCondition`,否则 `LevelManager` 会禁用自己。

## ⑫ 加空白点击层

1. 把 `Assets/UI/Prefabs/ClickScreen.prefab` 拖到 `UiRoot` 下,设为**第一个子物体**。
2. 需要空白点击音效时,给该实例加 `UiClickAudio`,`clickSfx` 选目录里的 cue。
3. 世界指针玩法判断遮挡用 `UiPointer.IsPointerOverBlockingUi()`;不要用 `EventSystem.current.IsPointerOverGameObject()`。
4. 不需要就不要加:它会成为全屏射线目标。
