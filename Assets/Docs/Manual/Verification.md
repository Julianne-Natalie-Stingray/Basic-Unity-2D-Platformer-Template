# 场景与验收

> 运行步骤见 [GettingStarted.md](GettingStarted.md)；代码结构见 [Development/ARCHITECTURE.md](../Development/ARCHITECTURE.md)。

## 1. 资源与场景清单

| 类别   | 产物                                                               |
| ---- | ---------------------------------------------------------------- |
| 贴图   | `Visuals/Player.png`、`Ground.png`、`Goal.png`（占位）                 |
| 物理   | `Player/Physics/Player.physicsMaterial2D`                        |
| Tile | `Level/SO/GroundTile.asset`                                      |
| SO   | 每个场景一个 `Scene/Samples/<Name>SceneDefinition.asset`               |
| 预制体  | `Player/Prefabs/Player.prefab`、`Core/Prefabs/Camera.prefab`      |
| 场景   | `Scene/Samples/MainMenu.unity`、`SampleLevel.unity`、`Final.unity` |
| 层级   | `Ground`、`Player`                                                |

`SampleLevel` 组成：

- `Core` 预制体
- `SceneRoot`：`LevelManager` + `EnvironmentFacade` + `WorldGeometries` / `Cameras`
- `Grid`
- `Player`
- `Main Camera`：Camera 预制体
- `UiRoot`：Death / NextLevel / Pause / Settings 面板
- 3 个可收集 `LevelGoal` + `LevelGoals`（`LevelGoalAndOperator`）
- `LevelManager.rootCondition`
- `Hazard`
- 水平 `MovingPlatform`
- `Portal` / `PortalExit`（单向 + 冷却状态）
- `EventSystem`

## 2. 验收标准

### 2.1 协作

- 新成员按 [AGENTS.md](../AGENTS.md) 可完成 onboarding（Smart Merge、owner 规则）。
- 加一个场景只需“建场景 + 放 `LevelManager` / `EnvironmentFacade` + 建 `SceneDefinition`”，编辑器工具一键注册，不修改共享代码文件。

### 2.2 验收

#### 基本

- `Console`无报错

#### 游玩

- [ ] 
- [ ] 主菜单 → New Game → 进入 `SampleLevel`；
- [ ] 暂停菜单 Save 后返回主菜单，Continue 可读档进入。
- [ ] 右上角 Menu 打开暂停面板并冻结；Resume 解冻。
- [ ] A/D 移动、Space 跳跃；相机跟随且受边界限制。
- [ ] 收集全部 3 个目标 → 通关面板 → Next → `Final`。
- [ ] 踩 `Hazard` → 死亡面板。
- [ ] 全程 Console 无 Error。
