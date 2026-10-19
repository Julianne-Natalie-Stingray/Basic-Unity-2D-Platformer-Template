# 拓展指南

> 每个官方拓展点怎么做。English: [EXTENDING.EN.md](EXTENDING.EN.md)。架构背景见 [ARCHITECTURE.md](ARCHITECTURE.md)。

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
2. 需要订阅游戏事件时,在常驻的 Canvas 根(`UiRoot`)下新增脚本;按钮行为优先复用 `SceneSwitchButton` / `OpenPanelButton` / `QuitGameButton`,不要在屏幕脚本里写按钮方法。
3. 面板默认未激活;打开用 `UiPanel.Open()`,关闭用 `UiPanel.Close()`(可接到按钮的持久监听)。
4. 新增屏幕 = 新脚本 + 场景内面板/预制体,不修改共享脚本。

## ④ 加输入动作

- 用工具窗口 `Tools/Basic2DPlatformer/Project` 的 **Input** 标签页增删 action/binding;它写回 `.inputactions` 并触发 `PlayerControls.cs` 重生成。
- 再在 `Core/Managers/InputManager.cs` 上**手写**成对的事件转发(工具不改 Core 代码)。
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
3. 在 `Assets/Docs/Subsystems/<Name>.md`(+ `.EN.md`)登记职责与公共 API,并在 `ARCHITECTURE.md` 的表里加一行。

## ⑧ 加项目级契约

当接口或数据载荷会被**多个互不相关的程序集**实现或消费时:

1. 放进 `Assets/Contracts/Scripts`(`Basic2DPlatformer.Contracts`,零项目依赖)。
2. 接口与其载荷(`struct`/`class`/`enum`)放同一层,不要拆 `Data`。
3. 上层程序集引用 `Contracts`;`Contracts` **不得**反向引用任何 `Basic2DPlatformer.*`。
4. 只在单一程序集内使用、或只被上层以具体类型使用的类型,留在原子系统。
5. 改完运行 `Tools/Basic2DPlatformer/Project` 的 `Validate`(会校验 asmdef 边)。

> 典型:`IInteractable` / `InteractionDetails` / `IKillable`。
