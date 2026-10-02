# Core 子系统

## 职能

Core 是本工程共享运行时基础设施的单一入口.

- 拥有共享服务门面, 其他 Subsystem 由它取得依赖.
- 拥有输入处理, 音频播放与对象池.
- 不负责游戏规则, 玩法状态, 以及任何场景专属行为.
- 不负责生成的输入绑定资产; 那些位于 `Assets/Infra/InputSystem/` 下.

## 构成

| 路径 | 类型 | 职责 |
|---|---|---|
| `CoreFacade.cs` | `GameCore` 上的 MonoBehaviour | 单一入口; 暴露 `Input` 与 `Audio`; 负责 `DontDestroyOnLoad` |
| `Managers/InputManager.cs` | MonoBehaviour | 输入服务; 消费生成的 InputSystem 动作 |
| `Managers/AudioManager.cs` | MonoBehaviour | 音频总线; 拥有 emitter 池, 注册表与两级实例上限 |
| `Audio/AudioBuilder.cs` | struct | 单次播放请求的链式配置凭证, 承载实时效果参数 |
| `Audio/ISoundHandle.cs` | 接口 | 单次播放的作用域契约 |
| `Audio/SoundHandle.cs` | class | `ISoundHandle` 的唯一实现 |
| `Audio/AudioRegistry.cs` | class | 活跃实例的注册, 注销与顺序查询 |
| `Audio/AudioEmitter.cs` | MonoBehaviour | 单个池化声部; 播放, 跟随位置, 报告结束 |
| `Audio/SODefinitions/` | ScriptableObject | `AudioClipData` 描述单个 clip; `AudioManagerConfigs` 是音频总线配置 |
| `GameObjectPool/` | class | 复用对象池; 额外提供"能否复用"的硬上限探查 |
| `Input/` | class | `DragAndDropService2D`, 纯可序列化辅助类 |

## 公共API

- `CoreFacade.Input` -> `InputManager`: 输入服务.
- `CoreFacade.Audio` -> `AudioManager`: 音频总线. 启动声音的唯一入口是
  `CoreFacade.Audio.CreateBuilder().With...().Play(audioId)`, 返回 `ISoundHandle`; 请求被拒绝时返回 `null`.
- `AudioManager.Registry` -> `AudioRegistry`: 只读的事实查询, 供需要知道"现在在播什么"的代码使用.
- `InputManager` 与 `AudioManager` 是 `CoreFacade` 的必需组件, 由 `InitializeInternal()` 通过 `GetComponent<>()` 解析.
- `AudioClipData` 与 `AudioManagerConfigs` 是数据定义; `AudioId` 是 clip 查找键.

## 内部实现思路

- 初始化在 `Awake()` 中完成; Subsystem 内部依赖由 `InitializeInternal()` 通过 `GetComponent<>()` 加缓存解决, 不需要 Inspector 接线.
- 管理器引用缓存在私有字段中, 以只读 Property 暴露.
- 需要跨场景存活的数据持久化为 ScriptableObject, 而不是场景内序列化字段.
- `CoreFacade` 是唯一调用 `DontDestroyOnLoad` 的类型; 各管理器自身不调用.

### 音频的限流与释放链路

音频有**两个互相独立**的实例上限, 都**不是**池大小:

- **每 clip 上限**: `AudioClipData.MaxInstances`, 表达设计意图.
- **全局上限**: `AudioManagerConfigs.MaxSoundInstance`, 保护总声部数.
- **池大小**: `AudioManagerConfigs.MaxPoolSize`, 只界定保留多少 emitter 用于复用.

因此 `MaxPoolSize` 必须不小于 `MaxSoundInstance`, 否则超出的 emitter 会被销毁而不是回收, 池化在最需要它的场景下失效.

请求路径是: 解析 `AudioId` -> 应用每 clip 上限 -> 应用全局上限 -> 预定 emitter -> 注册 -> 配置 -> 启动.
两级上限的抢占都只做一件事: 停止最旧且**非循环**的受害者, 其余交给常规结束路径, 因此池归还, 注册表注销与句柄失效都只发生在一处.

池归还后处理运行在 `AudioEmitter.onAudioFinished` 上, 它是**所有**结束路径的唯一汇合点:
自然播完, 句柄 `Stop()`, 以及被抢占. 这条链一旦断掉, emitter 就会永久滞留在池外, 所以
`AudioManager` 会在初始化时确认 Timer 包的协程宿主存在, 缺失则禁用自身而不是静默泄漏.

`AudioEmitter` 的位置同步发生在 `LateUpdate`, 且**不**改变父子关系: emitter 始终挂在 `AudioRoot` 下,
位置靠写自身世界坐标实现. 因此跟随的声音不会随目标销毁而消失 —— 一次性音在目标最后出现的位置播完,
循环音则在目标消失时停止, 因为循环没有自然结尾, 会永久占住一个池化 emitter.

## TODO

- TODO: 为音频增加"总线整体缩放"(如全局音量滑条). 当前 `Master.mixer` 未暴露任何参数, 因此正确的落点是 AudioMixer 的 exposed parameter, 需要一次独立的资产配置交付.
