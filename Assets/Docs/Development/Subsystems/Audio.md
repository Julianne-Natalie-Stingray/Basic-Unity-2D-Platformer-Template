# Audio 子系统

> 代码位于 `Core/Scripts/Audio` 与 `Core/Scripts/Persistent/AudioManager.cs`，程序集归属 `Basic2DPlatformer.Core`。

## 子系统功能

管理音频请求、池、并发上限与暂停门；提供按分层路径播放入口。

## 依赖的子系统及具体依赖业务

- LayeredId：`ILayeredId`、`LayeredIdCatalog`、`LayeredIdDrawer`。
- Game：`TimeScaleManager.IsPaused`（暂停门）、`SettingBootstrap`（音量初值）。
- GameLog / Pooling / Timer / NaughtyAttributes（外部包）。

## API

### 标识与目录

- `AudioId`：`[Serializable] struct`，只序列化 `path`；`None` / `From` / `Path` / `IsValid`。
- `AudioCatalog`：`[LayeredIdCatalog]`，合法路径为 `public const string`。
- `AudioManagerConfigs.Audios`：`AudioClipData` 注册表。

### 播放

- `AudioManager.CreateBuilder()`：发放一次请求凭证。
- `AudioBuilder.WithVolume` / `WithPitch` / `WithRandomPitch` / `WithPosition` / `WithFollowTarget` / `WithSurviveFreeze` / `WithAllowWhileFrozen` / `Play`。
- `ISoundHandle`：`AudioId` / `IsPlaying` / `IsFinished` / `Finished` / `Stop` / `TrySetVolume` / `TrySetPitch`。

### 数据

- `AudioClipData`：`AudioId`、`Clip`、`MixerGroup`、`Loop`、`MaxInstances`、`Volume`、`Pitch`、`SpatialBlend`、`MinDistance`、`MaxDistance`、`DefaultSurviveFreeze`。

## 注意事项

### 总则

- `AudioManager.Awake` 建立 `cueIndex`；重复 ID 保留第一个并告警，空 ID 跳过。
- 池化、prewarm、单 cue 与全局实例上限、最旧优先抢占都在 `AudioManager`；**循环 clip 不会被抢占**。
- 2D / 3D 由 `AudioClipData.SpatialBlend` 决定；3D 必须提供位置或跟随目标。
- `AudioBuilder` 的音量/音高只覆盖本次播放；未设置时回落 `AudioClipData` 的 baseline。
- `WithRandomPitch` 以 1 为基准，会覆盖 clip 的 baseline pitch。
- 自然播完与显式 `Stop` 都只报告一次结束；`IsFinished` 为真表示自然播完。
- 缺 clip 时也会报告结束，池租约不会泄漏。

### 暂停门

- 暂停判定统一读 `TimeScaleManager.IsPaused`。
- `WithSurviveFreeze(true)`：已播放的声音在暂停期间继续。
- `WithAllowWhileFrozen(true)` 且会保留播放：暂停期间仍受理新请求。
- `AudioListener.pause` 由 `AudioManager` 单一写入。

### 启动音乐

- `AudioManager.startupOst` + `playOnStart` 在 `Start` 播放；`Core.prefab` 预置 `Ost/Generic`。
- 启动音乐用 `SurviveFreeze + AllowWhileFrozen`，暂停菜单期间不中断。

### 编辑器

- `AudioIdDrawer`：按 `/` 分层下拉；值不在目录时画红底线。
- `AudioManagerConfigsEditor`：按路径首段分组，标出空 ID / 目录外 / 重复 / 空引用，支持增删。
- `Tools/GrignardReagent/Layered Id Catalogs`：查看所有目录的成员与校验问题。

## 新增一个音效

1. 在 `Assets/Sounds/SO/` 新建 `AudioClipData`，填 `clip` / `mixerGroup` / `id`。
2. 在 `AudioCatalog` 加一行 `public const string Jump = "Sfx/Player/Jump";`。
3. 把该 `AudioClipData` 拖进 `DefaultAudioManagerConfigs`。
4. 任意 `AudioId` 字段的 Inspector 里选择该路径。

路径发布后不要改名；成员名可以随意重命名。

## 内容

```text
Core/Scripts/Audio/
  AudioCatalog.cs — 路径目录
  AudioBuilder.cs — 请求凭证
  AudioRegistry.cs — 活跃声部
  AudioEmitter.cs — 单个池化声部
  ISoundHandle.cs — 句柄契约
  SoundHandle.cs — 句柄实现
  Enum/AudioId.cs — 标识
  SODefinitions/AudioClipData.cs — 单个 cue
  SODefinitions/AudioManagerConfigs.cs — 注册表 + 池参数
Core/Scripts/Persistent/AudioManager.cs — 服务
Assets/Editor/AudioIdDrawer.cs — ID 抽屉
Assets/Editor/AudioManagerConfigsEditor.cs — 注册表 Inspector
```

## 待办

- `variants` / `RandomMode` / `PitchJitter` 尚未实现；均为 `AudioClipData` 的纯增量字段。
- `SurviveFreeze` / `AllowWhileFrozen` 的命名沿用“冻结”语义，运行时对应 `TimeScaleManager.IsPaused`。