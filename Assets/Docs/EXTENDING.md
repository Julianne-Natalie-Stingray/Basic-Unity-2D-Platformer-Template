# 拓展指南

> 每个官方拓展点怎么做。English: [EXTENDING.EN.md](EXTENDING.EN.md)。架构背景见 [ARCHITECTURE.md](ARCHITECTURE.md)。

## ① 加场景 / 关卡

1. 新建场景,搭好 Tilemap 地面、`Hazard`、Core 等。
2. 放通关条件并接入 `LevelManager`(场景本地):
   - `LevelManager.rootCondition` 指向根条件(需实现 `ILevelGoal`)。
   - 单条件: 放 `LevelGoal`(触发器),把它拖到 `rootCondition`。
   - 多条件: 放 `LevelGoalAndOperator`(全部达成)或 `LevelGoalOrOperator`(任一达成),把各 `ILevelGoal` 拖进其 `goals`,再把组合器拖到 `rootCondition`(收集型: 给 `LevelGoal` 勾选 `disableWhenFulfilled`)。
   - 自定义条件: 新增组件实现 `ILevelGoal`(`bool Fulfilled` + `event Changed`),例如"击杀计数"。
3. 在场景同目录新建 `SceneDefinition` 资产(右键 `Create/Basic2DPlatformer/Scene Definition`),在 Inspector 里拖入该场景;`Id` 会自动填为场景名。
4. 打开 `Tools/Basic2DPlatformer/Project`,点 `Register All in Build Settings`;`Validate` 应为通过。
5. 需要从别处进入它时,在调用方(如 `NextLevelScreen.nextLevel`)引用这个 `SceneDefinition` 资产。

> 全程不修改任何共享代码文件;新增的是"场景 + 组件 + 一份新资产"。

## ② 加可交互物

1. 在 `Assets/Level/Scripts`(或你的玩法程序集)新增组件,实现 `IInteractable`:
   `bool CanInteract { get; }`、`event Action<IInteractable> OnInteracted;`、`void Interact(InteractionDetails details)`。
2. `Interact` 中先按 `CanInteract` 设门,再施加效果,最后触发 `OnInteracted`。
3. 挂在场景对象上,给一个 Collider2D(Trigger 可被交互半径命中)。

## ③ 加 UI 屏幕

1. 在 `Assets/UI/Scripts` 新增屏幕脚本(`Basic2DPlatformer.UI` 程序集)。
2. 需要冻结游戏时挂 `FreezeWhileVisible`,由 `FreezeCoordinator` 协调;不要直接改 `Time.timeScale`。
3. 新建 UI 预制体或从现有屏幕预制体做 **Prefab Variant**,不去改主预制体。

## ④ 加输入动作

- 用工具窗口 `Tools/Basic2DPlatformer/Project` 的 **Input** 标签页增删 action/binding;它写回 `.inputactions` 并触发 `PlayerControls.cs` 重生成。
- 再在 `Core/Managers/InputManager.cs` 上**手写**成对的事件转发(工具不改 Core 代码)。
- `.inputactions` 是共享文件,改动需知会;`PlayerControls.cs` 是生成物,不手改。

## ⑤ 加玩家能力

1. 在 `Assets/Player/Scripts` 新增组件(读 `CoreFacade.Instance.Input`、`GameStateManager`)。
2. 用 Player 预制体的 **Variant** 挂载,不直接改 `Player.prefab`。
3. 不把玩法规则写进 Env/Core;组件只做自己的能力。

## ⑥ 加数据型 SO

- 用工具窗口的 **SO** 标签页生成 `XxxConfigs` 脚本(含 `[CreateAssetMenu]`)与默认资产;或手写 `[CreateAssetMenu]`(字面量路径)。
- 数据资产只放数据与查找方法,不放逻辑。

## ⑦ 加新 Subsystem

1. 新建 `Assets/<Name>/Scripts/` 与 asmdef(`Basic2DPlatformer.<Name>`),按 [ARCHITECTURE.md](ARCHITECTURE.md) 的依赖方向填引用。
2. 若被更上层使用,更新上层 asmdef 的 references。
3. 在 `Assets/Docs/Subsystems/<Name>.md`(+ `.EN.md`)登记职责与公共 API,并在 `ARCHITECTURE.md` 的表里加一行。
