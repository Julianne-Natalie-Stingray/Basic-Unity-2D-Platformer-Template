# Game 子系统

> English: [Game.EN.md](Game.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

Game 承载全局状态、持久化设置与（后续的）进度。它不新建一套玩家或关卡模型。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `GameState/GameState.cs` | 枚举 | `Playing` / `Freezed` / `Loading` |
| `GameState/GameStateManager.cs` | 静态类 | 持有 `Current`;`Freeze`/`Resume`/`EnterLoading`/`ExitLoading`;广播 `Changed` |
| `Setting/ISettingData.cs` | 接口 | 持久化数据契约 |
| `Setting/SettingStore.cs` | 泛型基类 | 加载/持有/保存/重置;文件 I/O |
| `Setting/FileSettingStore.cs` | 具体类 | JsonUtility 落盘 |
| `Setting/GameSettings.cs` | 数据类 | 模板设置数据(当前含 `Audio`) |
| `Setting/AudioSettings.cs` | 数据类 | Master/Ost/Sfx 音量(0..1,默认 1/0.5/0.5) |
| `Setting/SettingBootstrap.cs` | 静态类 | 场景加载前建立并加载存储 |

> `AudioSettings.cs` 原先在 `Core/Audio/Setting/`,因 `GameSettings` 消费它而迁入此处(见 [ARCHITECTURE.md](../ARCHITECTURE.md) §1)。

## 公共 API

- `GameStateManager.Current` / `Freeze()` / `Resume()` / `Changed`。
- `SettingBootstrap.Settings` / `Save()` / `ResetToDefault()`。

## 注意

- `GameStateManager` 目前**没有** owner 计数冻结接口;面板冻结由 `FreezeCoordinator`(新增)协调。
- 设置只做存取,不负责应用;应用由拥有该系统的代码完成。
- 进度子系统(顺序解锁)为后续交付,尚未落地。
