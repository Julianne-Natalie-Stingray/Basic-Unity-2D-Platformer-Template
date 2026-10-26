# 场景与验收

> 运行步骤见 [GettingStarted.md](GettingStarted.md)；代码结构见 [Development/ARCHITECTURE.md](../Development/ARCHITECTURE.md)。

## 1. 资源与场景清单

| 类别   | 产物                                                               |
| ---- | ---------------------------------------------------------------- |
| 贴图   | `Visuals/Player.png`、`Ground.png`、`Goal.png`（占位）                 |
| 物理   | `Player/Physics/Player.physicsMaterial2D`                        |
| Tile | `Level/SO/GroundTile.asset`                                      |
| SO   | 每个场景一个 `Scene/Samples/<Name>SceneDefinition.asset`;音频 cue 在 `Sounds/SO/` |
| 预制体  | `Player/Prefabs/Player.prefab`、`Core/Prefabs/Camera.prefab`、`UI/Prefabs/ClickScreen.prefab`（可选） |
| 场景   | `Scene/Samples/MainMenu.unity`、`SampleLevel.unity`、`Final.unity` |
| 层级   | `Ground`、`Player`                                                |
| 包    | `Assets/LayeredId`（Runtime / Editor / Tests）                      |

`SampleLevel` 组成：

- `Core` 预制体
- `SceneRoot`：`LevelManager` + `EnvironmentFacade` + `WorldGeometries` / `Cameras`
- `Grid`
- `Player`
- `Main Camera`：Camera 预制体
- `UiRoot`：Death / NextLevel / Pause / Settings 面板 + `UiHotkeys`
- 3 个可收集 `LevelGoal` + `LevelGoals`（`LevelGoalAndOperator`）
- `LevelManager.rootCondition`
- `Hazard`
- 水平 `MovingPlatform`
- `Portal` / `PortalExit`（单向 + 冷却状态）
- `EventSystem`

> `ClickScreen.prefab` 是 opt-in，默认不在任何场景中。

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
- [ ] A/D 移动、W/↑ 跳跃；相机跟随且受边界限制。
- [ ] 收集全部 3 个目标 → 通关面板 → Next → `Final`。
- [ ] 踩 `Hazard` → 死亡面板。
- [ ] 全程 Console 无 Error。

#### 音频

- [ ] 启动即播放 `Ost/Generic`（暂停菜单期间不中断）。
- [ ] 主菜单 New Game / Continue 按钮按下有 `Sfx/UI/MouseClick`。
- [ ] 设置面板拖动 SFX / OST / Master 滑块能即时影响对应总线。
- [ ] 暂停后点击按钮仍有声音（`AllowWhileFrozen + SurviveFreeze`）。

#### 时间缩放与热键

- [ ] 空格打开暂停（`timeScale=0`、`AudioListener.pause=true`）；再按空格恢复。
- [ ] Esc 逐层关闭面板；没有面板时 Esc 打开暂停。
- [ ] 暂停状态下切换场景，新场景 `timeScale=1`、未暂停、音频恢复。

#### 可选：空白点击层

- [ ] 把 `ClickScreen.prefab` 拖到 `UiRoot` 首位，加 `UiClickAudio` 并选 `Sfx/UI/MouseClick`。
- [ ] 点空白处有声音；点按钮只出按钮声（不双响）。
- [ ] `UiPointer.IsPointerOverBlockingUi()` 在点击层上为 false，在按钮上为 true。

#### 关卡失败通道

- [ ] EditMode 测试全绿（当前 26 条，含 `LevelManagerTests` 的失败互斥用例）。
- [ ] 自定义失败条件：组件拿到 `LevelManager` 后调用 `NotifyFailed()`，只广播一次。
