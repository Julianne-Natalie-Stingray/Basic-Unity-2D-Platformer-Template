# UI 子系统

## 子系统功能

负责面板显隐、焦点、按钮意图、点击层与热键路由；不负责规则与时间缩放。

## 依赖的子系统及具体依赖业务

- Core：输入、音频、场景切换与存档入口；`PassThroughUi` 标记。
- Game：`GameState`、`TimeScaleManager`。
- Level：通关事件与环境门面。
- Player：死亡事件。

## API

仅列跨子系统公共。

### 字段

- `UiPanel.TopOpen`：最后打开且仍激活的面板。
- `UiPointer.IsPointerOverBlockingUi()`：指针是否被真正的 UI 挡住。

### 事件

- `DeathScreen.Confirmed`：面板确认广播。

### 公共方法

- `UiPanel.Open`：打开面板。
- `UiPanel.Close`：关闭面板。
- `UiPointer.IsPointerOverBlockingUi(Vector2)`：供自定义输入源与测试使用。

## 注意事项

### 总则

- 面板默认未激活；开关只走 `UiPanel`。
- 暂停由 `UiPanel` 通过 `TimeScaleManager.PushPause/PopPause` 申请，不直接写 `timeScale`。
- `UiPanel` 维护打开栈：`OnEnable` 入栈、`OnDisable` 出栈；`Esc` 关闭栈顶。
- 按钮行为由组件携带；屏幕脚本不写按钮方法。
- 焦点由 `EventSystem` 与 `defaultSelection` 处理。

### Core

- 场景切换与存档经 `CoreFacade`。
- 不直接读写存档文件。
- 输入与音频由 Core 提供。
- 键盘热键由 `UiHotkeys` 订阅 `InputManager.PausePressed` / `CancelPressed`。

### Game

- `GameState.Loading` 时 `UiPanel` 拒绝打开。
- 暂停判定统一走 `TimeScaleManager.IsPaused`。

### Level

- `NextLevelScreen` 订阅 `LevelCleared`。
- 失败表现（若有）订阅 `LevelFailed`。
- 不扫描场景查找关卡。

### Player

- `DeathScreen` 订阅 `PlayerDied`。
- 不处理死亡表现与物理。

### 点击层

- `ClickScreen.prefab` 是 opt-in：全屏透明 `Image`（`raycastTarget` 开启）+ `PassThroughUi` 标记，不含音效。
- 必须放在 `UiRoot` 的第一个子物体，保证按钮优先消费点击。
- 世界指针玩法用 `UiPointer.IsPointerOverBlockingUi()`；`EventSystem.IsPointerOverGameObject()` 会被点击层永久置真。
- `UiClickAudio` 挂在点击层上播放空白点击；不要同时挂在按钮上，否则会与 `SelectableAudio` 叠加。

## 内容

```text
UI/
  Prefabs/
    SettingsPanel.prefab — 设置面板
    ClickScreen.prefab — 可选的全屏空白点击层
  Scripts/
    Basic2DPlatformer.UI.asmdef — 程序集边界
    Panels & Screens/
      UiPanel.cs — 面板开关与打开栈
      SettingsScreen.cs — 音量设置
      DeathScreen.cs — 死亡面板
      NextLevelScreen.cs — 通关面板
      ClosePanelButton.cs — 关闭面板
    ScriptComponents/
      SceneSwitchButton.cs — 场景切换
      OpenPanelButton.cs — 打开面板
      QuitGameButton.cs — 退出游戏
      NewGameButton.cs — 新游戏
      ContinueButton.cs — 读档继续
      SaveButton.cs — 存档
      SelectableAudio.cs — 交互音效
      UiClickAudio.cs — 空白点击音效
      UiHotkeys.cs — 空格/Esc 路由
    UiPointer.cs — UI 命中查询
```

## 实现思路

### 代码架构

面板门面 + 打开栈 + 组件化按钮 + 事件接线。

### 使用的范式

组合、门面、事件订阅、持久监听。

### 内部方法 / 字段 / 事件

- `UiPanel.previousSelection`：焦点还原。
- `UiPanel.defaultSelection`：默认焦点。
- `UiPanel.freezeWhileOpen`：冻结开关。
- `UiPanel.OpenPanels`：打开顺序栈。
- `UiHotkeys.pausePanel`：空格/Esc 的默认目标。
- `DeathScreen.player`：死亡源。
- `NextLevelScreen.level`：通关源。