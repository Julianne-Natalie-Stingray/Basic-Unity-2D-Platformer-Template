# Game 子系统

## 子系统功能

负责全局状态、时间缩放、设置、进度与存档编排；不负责场景与玩法。

## 依赖的子系统及具体依赖业务

- Contracts：`ISaveContributor` 契约。

## API

### 字段

- `GameStateManager.Current`：当前状态（`Playing` / `Loading`）。
- `TimeScaleManager.IsPaused` / `Speed` / `EffectiveScale`：时间缩放。
- `SettingBootstrap.Settings` / `GameSettings.Audio`：设置数据。
- `AudioSettings.MasterVolume` / `OstVolume` / `SfxVolume`：音量。
- `GameProgress.Store`：内存进度。
- `LevelProgressStore.CompletedLevels` / `LastLevelId`：进度数据。
- `GameProgressContributor.Instance`：进度贡献者。
- `SaveSystem.HasSave` / `PendingCount`：存档状态。
- `GameSaveStore.FilePath` / `Exists`：存档文件。

### 事件

- `TimeScaleManager.Changed`：暂停或速度变化。
- `GameProgress.LevelCompleted`：进度推进。

### 公共方法

- `GameStateManager.EnterLoading` / `ExitLoading`：加载状态迁移。
- `TimeScaleManager.PushPause` / `PopPause`：按 owner 申请/释放暂停。
- `TimeScaleManager.SetSpeed` / `ClearSpeed`：按 owner 设置/清除速度倍率。
- `TimeScaleManager.ResetAll`：清空所有请求并恢复 1 倍。
- `SettingBootstrap.Save` / `ResetToDefault`：设置存取。
- `GameProgress.Reset` / `NotifyLevelCompleted`：进度重置与通知。
- `LevelProgressStore.SetLastLevel` / `Restore` / `CompleteLevel`：进度写入。
- `SaveSystem.Save` / `TryLoad` / `TryRestore` / `RestoreAll` / `NewGame`：存档编排。
- `GameSaveStore.TryLoad` / `TrySave` / `Delete`：文件操作。

## 注意事项

### 总则

- `TimeScaleManager` 是 `Time.timeScale` 的唯一写入者；暂停优先于速度。
- 暂停与速度都按 owner 计数：暂停是集合，速度是“多 owner 相乘后钳制 0.1–4”。
- 场景切换前调用 `ResetAll()`；`GameStateManager` 不再回写时间。
- 设置只做存取，不负责应用。
- 存档只含 `progress`；机关状态不写入。
- `lastLevelId` 用稳定 `levelId`，不用场景名。
- 信封固定为 `version + sections[]`。
- 任一 section 失败时不写正式文件。
- 自动存档由 `LevelCompleted` 触发，延迟一帧。
- 退出仅在已有存档且未刚 New Game 时覆盖。
- `SaveSystem` 不扫场景；场景恢复由 `SaveService` 负责。

### Contracts

- `GameProgressContributor` 实现 `ISaveContributor`。
- `SaveSystem` 只消费该接口。

## 内容

```text
Game/
  Scripts/
    Basic2DPlatformer.Game.asmdef — 程序集边界
    GameState/
      GameState.cs — 状态枚举
      GameStateManager.cs — 状态管理
    TimeScale/
      TimeScaleManager.cs — 暂停与速度
    Setting/
      ISettingData.cs — 设置契约
      SettingStore.cs — 存储基类
      FileSettingStore.cs — JSON 存储
      GameSettings.cs — 设置数据
      AudioSettings.cs — 音量数据
      SettingBootstrap.cs — 设置入口
    Progress/
      GameProgress.cs — 进度入口
      LevelProgressStore.cs — 进度数据
      GameProgressData.cs — 进度载荷
      GameProgressContributor.cs — 进度贡献者
    Save/
      SaveSystem.cs — 存档编排
      SaveEnvelope.cs — 存档信封
      SaveSection.cs — 存档段
      SaveResult.cs — 存档结果
      GameSaveStore.cs — 文件存取
```

## 实现思路

### 代码架构

静态状态 + 请求集合 + 数据存储 + 存档编排。

### 使用的范式

状态机、owner 计数、贡献者、原子写。

### 内部方法 / 字段 / 事件

- `TimeScaleManager.PauseOwners` / `SpeedOwners`：请求集合。
- `TimeScaleManager.Purge`：清理已销毁 owner。
- `SaveSystem.pending`：待恢复段。
- `SettingStore.data`：设置实例。