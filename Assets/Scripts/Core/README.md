# Core 子系统

## 职能

Core 是本工程共享运行时基础设施的单一入口.

- 拥有共享服务门面, 其他 Subsystem 由它取得依赖.
- 拥有输入处理, 音频播放与对象池.
- 不负责游戏规则, 玩法状态, 以及任何场景专属行为.
- 不负责生成的输入绑定资产; 那些位于 `Assets/Infra/InputSystem/` 下.

## 构成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `CoreFacade.cs` | `GameCore` 上的 MonoBehaviour | 单一入口; 暴露 `Input` 与 `Audio`; 负责 `DontDestroyOnLoad` |
| `Managers/InputManager.cs` | MonoBehaviour | 输入服务; 只发布设备层, 并提供整套生成输入集合的入口 |
| `Managers/AudioManager.cs` | MonoBehaviour | 音频总线; 拥有 emitter 池, 注册表与两级实例上限 |
| `Audio/AudioBuilder.cs` | struct | 单次播放请求的链式配置凭证, 承载实时效果参数与位置 |
| `Audio/ISoundHandle.cs` | 接口 | 单次播放的作用域契约 |
| `Audio/SoundHandle.cs` | class | `ISoundHandle` 的唯一实现 |
| `Audio/AudioRegistry.cs` | class | 活跃实例的注册, 注销与顺序查询 |
| `Audio/AudioEmitter.cs` | MonoBehaviour | 单个池化声部; 播放, 定位, 跟随, 报告结束 |
| `Audio/SODefinitions/` | ScriptableObject | `AudioClipData` 描述单个 clip; `AudioManagerConfigs` 是音频总线配置 |
| `Audio/Setting/` | `[Serializable]` class | `AudioSettings`, 音频总线的持久化音量 |
| `GameObjectPool/` | class | 复用对象池; 额外提供"能否复用"的硬上限探查 |
| `Input/` | class | `DragAndDropService2D`, 纯可序列化辅助类; 早期代码 |

## 公共API

- `CoreFacade.Input` -> `InputManager`: 输入服务.
- `CoreFacade.Audio` -> `AudioManager`: 音频总线. 启动声音的唯一入口是
  `CoreFacade.Audio.CreateBuilder().With...().Play(audioId)`, 返回 `ISoundHandle`; 请求被拒绝时返回 `null`.
- `AudioBuilder` 的链式项: `WithVolume` / `WithPitch` / `WithRandomPitch` / `WithPosition` / `WithFollowTarget` /
  `WithSurviveFreeze` / `WithAllowWhileFrozen`.
- `AudioManager.Registry` -> `AudioRegistry`: 只读的事实查询, 供需要知道"现在在播什么"的代码使用.
- `InputManager` 与 `AudioManager` 是 `CoreFacade` 的必需组件, 由 `InitializeInternal()` 通过 `GetComponent<>()` 解析.
- `AudioClipData` 与 `AudioManagerConfigs` 是数据定义; `AudioId` 是 clip 查找键.

## 内部实现思路

- 初始化在 `Awake()` 中完成; Subsystem 内部依赖由 `InitializeInternal()` 通过 `GetComponent<>()` 加缓存解决, 不需要 Inspector 接线.
- 管理器引用缓存在私有字段中, 以只读 Property 暴露.
- 需要跨场景存活的数据持久化为 ScriptableObject, 而不是场景内序列化字段.
- `CoreFacade` 是唯一调用 `DontDestroyOnLoad` 的类型; 各管理器自身不调用.
- 跨组件协调的初始化放在 `Start`, 不放 `Awake`: `Awake` 只解析自身依赖, 凡是需要"另一个组件已完成初始化"的判断都等到 `Start`.
- 唯一的例外是音频的**建池**: 它放在 `InitializeInternal` 而非 `Start`, 因为其他组件的 `Start` 里就可能有播放请求, 而那时池必须已经存在. 只有**预热**放在 `Start`, 因为实例化可以推迟.

### 输入的层级边界

输入只做两层, **不做语义层**:

| 层 | 内容 | 是否在模版内 |
| --- | --- | --- |
| L1 设备与绑定 | 物理控件, 控制方案(control schemes), 重绑定, 多设备 | 属 asset, 归使用者; 模版不预设 |
| L2 原始轴 | 把设备映射为无名的模拟轴与按钮 | 模版提供 |
| L3 语义 | 哪一轴是什么, 与重力的关系, 谁拥有哪个自由度 | **刻意排除** |

排除 L3 的理由: 二维输入只有两个自由度, 而"这两个自由度是什么"是玩法信息. 横版与俯视两类玩法在**某一轴上恰好重合**, 因此 asset 里那根已接线的轴与那根留空的轴不是模版的断言, 而是"当下两类玩法的重合面". 模版不对重力方向、轴向命名或额外键位做任何猜测.

由此推出两条边界:

- 输入不耦合重力. 重力方向是工程设置(`Physics2D`)或玩法实现, 输入层无需知道.
- 输入不提供消费者. `MovementInput`, `PointerPosition`, `IsPrimaryPressed` 是**扩展点**, 不是功能; 读取它们并作出响应的代码属于玩法, 不属于模版.

底层类型不外泄: 生成的 `PlayerControls` **不**通过 wrapper 暴露. `InputManager` 的事件与 properties 是唯一 entry point,
因此在 asset 里新增动作时,**改动 `InputManager` 是不可避免的** —— 这一点被明确接受, 而不是用一条访问器去回避.
理由: `controls` 属于底层, 一旦外泄, "输入如何被消费"就不再由这个包装层决定; 而输入设计一旦改变, wrapper 本来就要跟着变.
代价: 新增一个按键需要在此处加一个成员. 这是**每次一个**的一次性成本, 换来的是消费面始终明确且封闭.

### 已知的早期代码

`DragAndDropService2D` 是本工程早期阶段的产物, 目前**没有任何使用者**: 它需要调用方自行 `Initialize(...)` 与 `Enable()`, 并在自己的帧循环里调用 `Drag()`, 而这些都没有发生. 它作为可复用服务保留在原处, 不在模版范围内继续演进.

### 音频的限流与释放链路

音频有**两个互相独立**的实例上限, 都**不是**池大小:

- **每 clip 上限**: `AudioClipData.MaxInstances`, 表达设计意图.
- **全局上限**: `AudioManagerConfigs.MaxSoundInstance`, 保护总声部数.
- **池大小**: `AudioManagerConfigs.MaxPoolSize`, 只界定保留多少 emitter 用于复用.

因此 `MaxPoolSize` 必须不小于 `MaxSoundInstance`, 否则超出的 emitter 会被销毁而不是回收, 池化在最需要它的场景下失效.

请求路径是: 检查冻结期入口 -> 解析 `AudioId` -> 应用每 clip 上限 -> 应用全局上限 -> 预定 emitter -> 注册 -> 配置 -> 定位 -> 启动.
两级上限的抢占都只做一件事: 停止最旧且**非循环**的受害者, 其余交给常规结束路径, 因此池归还, 注册表注销与句柄失效都只发生在一处.

池归还链路有**两个方向**, 必须分开, 否则会互相递归:

- **归还输出**: `OnEmitterRelease` 作为池的归还回调运行, 只做清理(注销, 重置, 重挂父级, 日志), **绝不**调用池的 `Release`.
- **归还输入**: `ReleaseEmitter` 订阅在 `AudioEmitter.onAudioFinished` 上, 只做一件事 —— 转发给 `GameObjectPool.Release`.

`AudioEmitter.onAudioFinished` 是**所有**结束路径的唯一汇合点: 自然播完, 句柄 `Stop()`, 以及被抢占.
这条链一旦断掉, emitter 就会永久滞留在池外.

结束判定本身由一个 Timer 完成, 并使用 `UseUnscaledTime()`: `GameStateManager.Freeze()` 会设
`Time.timeScale = 0`, 而走 scaled time 的判定在暂停期间不会推进 —— 若不清除这一处, 暂停期间所有 emitter
都将永不归还池. 这两处是一对, 不能只保留其一.

订阅是**先退订再订阅**的: emitter 是池化复用的, 若只订阅不退订, 复用会让处理器累积, 导致同一次结束被多次归还.

### 音频与游戏状态

`AudioManager` 在 `OnEnable` / `OnDisable` 中订阅 `GameStateManager.Changed`, 据状态暂停或恢复 `AudioListener.pause`.
依赖方向是**单向**的: 音频认识 `GameState`, 而 `GameState` 对其一无所知.

冻结涉及**两个互相独立**的问题. 把它们合并会得到错误的模型, 因此分开陈述:

| 问题 | 含义 | 机制 |
| --- | --- | --- |
| 进入冻结时, 已在播放的音是否被掐断 | 一次性的保留决定 | `AudioSource.ignoreListenerPause` |
| 冻结期间, 新到达的请求是否被受理 | 一道入口门 | `AudioManager.Play` 的入口检查 |

`SurviveFreeze` 回答第一个问题. 它**与 `Loop` 无关**: 循环音可以被冻结掐断, 一次性音也可以熬过冻结.

| `SurviveFreeze` | 效果 |
| --- | --- |
| `false`(默认) | 进入冻结时该音被 `AudioListener.pause` 切断 |
| `true` | 该音在冻结期间继续播放(常用于音乐) |

`WithAllowWhileFrozen` 回答第二个问题. 默认**拒绝**, 并在日志中记录被拒绝的 `AudioId`;
`WithAllowWhileFrozen(true)` 使该请求在冻结期间也被服务. 它存在的意义是让"冻结期间必须被拒绝"的请求得以表达,
例如由本不该运行的玩法所驱动的音效.

两者**不是同一个开关的正反两面**: 一个已在播放的音可以被保留, 而一个新的请求仍被拒绝; 反之亦然.

参数层级遵循全工程的静态/动态划分:

| 层 | 载体 | 作用 |
| --- | --- | --- |
| 静态初值 | `AudioClipData.DefaultSurviveFreeze`(默认 `false`) | 该 clip 的默认保留行为 |
| 动态覆盖 | `AudioBuilder.WithSurviveFreeze(bool)` | 仅覆盖本次请求的保留行为 |
| 动态覆盖 | `AudioBuilder.WithAllowWhileFrozen(bool)` | 仅覆盖本次请求是否可进入 |

实现落在代码里, 因此无需在 prefab 上逐个勾选, 且 `AudioEmitter.prefab` 未被改动.

### 音频的位置与空间化

位置来源有三种, 优先级明确:

| 来源 | 入口 | 说明 |
| --- | --- | --- |
| 跟随目标 | `WithFollowTarget(Transform)` | 每帧同步到目标; 目标消失后按下一节规则处理 |
| 一次性坐标 | `WithPosition(Vector3)` | 只写一次世界坐标 |
| 都不给 | —— | emitter 停在池留下的位置, 即 `AudioRoot` |

两者同时给出时**跟随胜出**, 因为跟随每帧覆盖位置.

`willFollowTarget` 是本模块的关键状态: 它表示"**本次播放是否请求了跟随**", 而 `followTarget == null` 只表示"当前没有目标".
两者必须分开 —— 否则每个不跟随的声音都会被误判为"跟丢了", 从而把循环音立刻停止.
该标志仅在 `SetFollowTarget()` 中置真, 在 `HandleLostTarget()` 与 `ResetEmitter()` 中置假:
跟随是**每次请求**的特性, 目标一丢失就必须重新请求, 不允许 emitter 开着跟随状态等待目标重新出现.

位置同步发生在 `LateUpdate`, 且**不**改变父子关系: emitter 始终挂在 `AudioRoot` 下, 位置靠写自身世界坐标实现.
因此跟随的声音不会随目标销毁而消失 —— 一次性音在目标最后出现的位置播完,
循环音则在目标消失时停止, 因为循环没有自然结尾, 会永久占住一个池化 emitter.

音量与位置是**互相独立**的两件事, 因此 `LateUpdate` 先写入待应用的音量, 再判断是否需要同步位置:
用"是否跟随"去门控音量写入, 会让每个不跟随的声音静默丢失 `SoundHandle.TrySetVolume` 的改动.

空间化由 `AudioClipData` 的三个字段描述, 并由 `AudioEmitter.Configure()` 写入 `AudioSource`:
`SpatialBlend`(0 为纯 2D), `MinDistance`, `MaxDistance`.

注意 `SpatialBlend == 0` 时位置**完全无影响**; 而 `SpatialBlend > 0` 时若既不跟随也不给坐标,
声音会始终从 `AudioRoot` 发出, 空间化形同无效. 二者必须一起使用才有意义.

## TODO

- TODO: 音频设置尚未被应用. `AudioSettings` 已有 `MasterVolume` / `OstVolume` / `SfxVolume`, 但 `AudioManager` 未读取它们, 因此改动设置不影响实际音量. 未实现原因: 归属 `Setting` 与 `Core` 的交界, 按 §4.1 应作为独立交付; 应用方式已定 —— 由音频自行拉取设置并应用到 mixer.
- TODO: 为音频增加"总线整体缩放"(如全局音量滑条). 当前 `Master.mixer` 虽已有 `SFX` / `OST` 分组, 但 `m_ExposedParameters` 为空数组, 因此代码层面无法调节总线音量; 正确的落点是 AudioMixer 的 exposed parameter, 需要一次独立的资产配置交付.
