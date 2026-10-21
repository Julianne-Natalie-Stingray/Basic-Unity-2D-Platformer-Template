# UI 子系统

## 子系统功能

负责面板显隐、焦点与按钮意图；不负责规则与时间缩放。

## 依赖的子系统及具体依赖业务

- Core：输入、音频、场景切换与存档入口。
- Game：游戏状态与存档结果。
- Level：通关事件与环境门面。
- Player：死亡事件。

## API

仅列跨子系统公共。

### 字段

暂无。

### 事件

- `DeathScreen.Confirmed`：面板确认广播。

### 公共方法

- `UiPanel.Open`：打开面板。
- `UiPanel.Close`：关闭面板。

## 注意事项

### 总则

- 面板默认未激活；开关只走 `UiPanel`。
- 冻结由 `UiPanel` 申请，不直接写 `timeScale`。
- 按钮行为由组件携带；屏幕脚本不写按钮方法。
- 焦点由 `EventSystem` 与 `defaultSelection` 处理。

### Core

- 场景切换与存档经 `CoreFacade`。
- 不直接读写存档文件。
- 输入与音频由 Core 提供。

### Game

- `GameState.Loading` 时 `UiPanel` 拒绝打开。
- 存档结果由 `SaveService` 返回。

### Level

- `NextLevelScreen` 订阅 `LevelCleared`。
- 不扫描场景查找关卡。

### Player

- `DeathScreen` 订阅 `PlayerDied`。
- 不处理死亡表现与物理。

## 内容

```text
UI/
  Prefabs/
    SettingsPanel.prefab — 设置面板
  Scripts/
    Basic2DPlatformer.UI.asmdef — 程序集边界
    UiPanel.cs — 面板开关
    SettingsScreen.cs — 音量设置
    DeathScreen.cs — 死亡面板
    NextLevelScreen.cs — 通关面板
    SceneSwitchButton.cs — 场景切换
    OpenPanelButton.cs — 打开面板
    ClosePanelButton.cs — 关闭面板
    QuitGameButton.cs — 退出游戏
    NewGameButton.cs — 新游戏
    ContinueButton.cs — 读档继续
    SaveButton.cs — 存档
    SelectableAudio.cs — 交互音效
```

## 实现思路

### 代码架构

面板门面 + 组件化按钮 + 事件接线。

### 使用的范式

组合、门面、事件订阅、持久监听。

### 内部方法 / 字段 / 事件

- `UiPanel.previousSelection`：焦点还原。
- `UiPanel.defaultSelection`：默认焦点。
- `UiPanel.freezeWhileOpen`：冻结开关。
- `DeathScreen.player`：死亡源。
- `NextLevelScreen.level`：通关源。
