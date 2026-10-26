# Core 子系统

## 子系统功能

负责输入、音频、场景切换与存档宿主；不决定玩法。

## 依赖的子系统及具体依赖业务

- Infra：生成的 `PlayerControls`。
- Game：`GameState`、`TimeScaleManager`、进度与存档。
- Contracts：`ISaveContributor` 契约。
- LayeredId：`ILayeredId`、`LayeredIdCatalog`（`AudioId` / `AudioCatalog` 的实现基础）。

## API

### 字段

- `CoreFacade.Instance`：服务入口。
- `CoreFacade.Input` / `Audio` / `SceneSwitch` / `Save`：服务引用。
- `InputManager.MovementInput` / `PointerPosition` / `IsPrimaryPressed`：设备层输入。
- `SceneSwitchManager.IsSwitching`：切换状态。
- `SceneDefinition.Id` / `SceneName` / `DisplayName`：场景定义。
- `SaveService.HasSave`：存档存在。
- `AudioId.Path` / `IsValid`：音频标识。
- `AudioCatalog.MouseClick` / `GenericSfx` / `GenericOst`：合法音频路径。
- `AudioManagerConfigs.Audios`：注册表条目。

### 事件

- `InputManager.PrimaryPressed` / `PrimaryReleased`：主按钮。
- `InputManager.UpPressed` / `UpReleased`：跳跃输入。
- `InputManager.InteractPressed` / `InteractReleased`：交互输入。
- `InputManager.PausePressed` / `CancelPressed`：暂停与取消。

### 公共方法

- `SceneSwitchManager.RequestSwitch`：切换场景；发起前重置时间缩放。
- `SaveService.Save` / `Load` / `NewGame`：存档入口。
- `AudioManager.CreateBuilder`：创建音频请求。
- `AudioManager.ApplyAudioSettings`：应用音量。
- `AudioBuilder.WithVolume` / `WithPitch` / `WithRandomPitch` / `WithPosition` / `WithFollowTarget` / `WithSurviveFreeze` / `WithAllowWhileFrozen` / `Play`：音频配置与播放。
- `ISoundHandle.Stop` / `TrySetVolume` / `TrySetPitch`：句柄控制。

## 注意事项

### 总则

- `Core` 为 `DontDestroyOnLoad`；重复实例销毁。
- 场景切换前调用 `TimeScaleManager.ResetAll()`；新场景从 1 倍、未暂停开始。
- `SaveService` 启动读档；场景段等一帧恢复。
- 自动存档由 `LevelCompleted` 触发。
- `AudioRegistry` / `AudioEmitter` 多为 internal。
- 对象池为 UPM 包；项目内无本地目录。

### Infra

- `PlayerControls` 是生成物，不手改。
- 新增 `Pause` / `Cancel` 动作后必须让 Input System 重新生成包装类。

### Game

- 读取 `GameState`、`TimeScaleManager`、进度与存档。
- 不反向依赖 Core。

### Contracts

- `SaveService` 消费 `ISaveContributor`。

### Audio

- `AudioId` 是只序列化 `path` 的 struct；`AudioCatalog` 只放 `public const string`。
- `AudioManager.Awake` 建立 `Dictionary<AudioId, AudioClipData>`；重复/空 ID 告警并忽略后者。
- `AudioBuilder.WithVolume/WithPitch` 只对本次播放生效；未设置时回落 `AudioClipData` 的 baseline。
- `startupOst` + `playOnStart` 在 `Start` 播放启动音乐。
- `AudioClipData` 的三条硬约束：`AudioId` 非空、`clip` 非空、`mixerGroup` 非空。

## 内容

```text
Core/
  Prefabs/
    Core.prefab — 服务宿主
    Camera.prefab — 相机
    AudioEmitter.prefab — 音频发射器
  SO/
    DefaultAudioManagerConfigs.asset — 音频配置
  Scripts/
    Basic2DPlatformer.Core.asmdef — 程序集边界
    CoreFacade.cs — 服务入口
    Persistent/
      InputManager.cs — 输入服务
      AudioManager.cs — 音频服务
    SceneSwitch/
      SceneSwitchManager.cs — 场景切换
      SceneDefinition.cs — 场景定义
    Save/
      SaveService.cs — 存档宿主
    Visuals/
      PassThroughUi.cs — 全屏点击层标记
    Audio/
      AudioCatalog.cs — 音频路径目录
      AudioBuilder.cs — 音频请求
      ISoundHandle.cs — 句柄契约
      SoundHandle.cs — 句柄实现
      AudioRegistry.cs — 音频注册
      AudioEmitter.cs — 音源
      Enum/AudioId.cs — 音频标识
      SODefinitions/AudioClipData.cs — 音频数据
      SODefinitions/AudioManagerConfigs.cs — 音频配置
```

## 实现思路

### 代码架构

服务门面 + 持久对象 + 异步切换。

### 使用的范式

组合、门面、事件、对象池。

### 内部方法 / 字段 / 事件

- `SceneSwitchManager.operation`：异步操作。
- `AudioManager.emitterPool`：音源池。
- `AudioManager.registry`：音频注册表。
- `AudioManager.cueIndex`：`AudioId` 到 `AudioClipData` 的索引。
- `SaveService.system`：存档编排。