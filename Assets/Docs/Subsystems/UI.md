# UI 子系统

> English: [UI.EN.md](UI.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

UI 把游戏状态显示出来,并把用户意图转成对其它子系统的请求。它不拥有游戏规则,也不直接改时间缩放。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `UiPanel.cs` | MonoBehaviour | 单个面板的显隐、焦点还原与可选冻结 |
| `SceneSwitchButton.cs` | MonoBehaviour | 按钮 → `SceneSwitchManager.RequestSwitch(SceneDefinition)` |
| `OpenPanelButton.cs` | MonoBehaviour | 按钮 → `UiPanel.Open()` |
| `QuitGameButton.cs` | MonoBehaviour | 按钮 → 退出;编辑器内停止播放 |
| `SettingsScreen.cs` | MonoBehaviour | 音量读取、保存与即时应用 |
| `NextLevelScreen.cs` | MonoBehaviour | `EnvironmentFacade.Level` 的 `LevelCleared` → `UiPanel.Open()` |
| `DeathScreen.cs` | MonoBehaviour | `PlayerDeath.PlayerDied` → `UiPanel.Open()`;暴露 `Confirmed` |
| `SelectableAudio.cs` | MonoBehaviour | 悬停 / 按下 / 抬起 / 提交 / 滑块音效 |

## 约定

- 面板默认未激活;开关一律走 `UiPanel.Open()` / `UiPanel.Close()`,不要直接 `SetActive`。
- 按钮的行为由按钮上的组件与其数据决定;屏幕脚本不写按钮方法。
- 冻结:面板的 `freezeWhileOpen` 向 `FreezeCoordinator` 请求/释放;绝不直接写 `Time.timeScale`。
- 焦点:`UiPanel` 打开时选中 `defaultSelection`,关闭时还给打开前的控件。键盘/手柄导航由 `EventSystem` 上的 `InputSystemUIInputModule` 提供,不需要在 `.inputactions` 里新增 UI map。
- `Confirmed` 是**仅在 C# 中的**预留接口,没有默认订阅者;`Confirm()` 由按钮的持久监听调用。

## 边界

- `NextLevelScreen` 通过 `EnvironmentFacade.Level` 订阅 `LevelCleared`;`DeathScreen` 订阅 `PlayerDeath.PlayerDied`。两者都不再扫描场景。
- `UI → Level`(环境门面)与 `UI → Player`(死亡事件);**不再直接依赖 `Character`**。
- Canvas 根对象统一命名为 `UiRoot`;它只是层级锚点,不承载"环境接线"组件。
- `Confirmed` 仍是**仅在 C# 中的**预留接口,没有默认订阅者。
