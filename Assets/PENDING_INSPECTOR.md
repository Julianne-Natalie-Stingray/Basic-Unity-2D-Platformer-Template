# PENDING INSPECTOR

记录**尚未由用户完成**的 Inspector 操作. 本文件是 §5.1.3 的硬闸门: 只要下列任一项未勾选,
DSH 不得开始任何依赖这些 Inspector 赋值的新工作.

解闸只能由你**用语言显式确认**触发 (例如"已配好"). DSH 不会仅凭勾选状态自行推断闸门已解除.

## 未完成项

### 1. 音频限流的数值关系 — 已完成, 记录判定依据

两个实例上限与池大小是三件不同的事:

| 字段                 | 位置                  | 当前值    | 含义                 |
| ------------------ | ------------------- | ------ | ------------------ |
| `maxSoundInstance` | AudioManagerConfigs | 30     | 全局并发上限: 同时最多几个声部   |
| `maxPoolSize`      | AudioManagerConfigs | **30** | 只保留多少 emitter 用于复用 |
| `defaultCapacity`  | AudioManagerConfigs | 10     | 池的初始容量             |
| `prewarmAmount`    | AudioManagerConfigs | 10     | 启动时预热多少个           |
| `maxInstances`     | 每个 AudioClipData    | 10     | 单个 clip 的并发上限      |

**硬性关系**: `MaxPoolSize >= MaxSoundInstance`, 且 `PrewarmAmount <= DefaultCapacity <= MaxPoolSize`.

**已满足**: `MaxPoolSize` 已由你从 `10` 改为 `30`, `30 >= 30` 成立.
依据: 2026-10-03 的运行日志在**第 31 个并发请求**处出现 `Pool exhausted at 30 emitters`, 证明池上限确实取到了 30.

- [x] 把 DefaultAudioManagerConfigs.asset 的 MaxPoolSize 改为不小于 MaxSoundInstance(当前 30).
- [x] 复核 PrewarmAmount <= DefaultCapacity <= MaxPoolSize 成立.
- [x] 为每个 AudioClipData 资产填写新增的 Volume 与 Pitch(默认 1 / 1 即保持原听感).
- [x] 按每个音效的设计意图复核 MaxInstances: 它此前从未生效, 因此现有的 10 是未经验证的默认值.

### 2. 验证本次音频交付

- [x] Unity Console 无编译错误; 四个新脚本已导入并生成 .meta.

- [x] Core.prefab 上 AudioManager 的 emitterPrefab / emitterRoot / configs 三个引用完好.

- [x] 已确认**正常发声**.

- [x] 日志出现 `AudioEmitter is released`, 且调用栈经过 `Timer/<Run>d__34:MoveNext`, 证明 Timer 完成并归还了 emitter.
  本次 Play 模式验证**失败过一次**, 记录两次修复:
1. **反射假阴性**: 旧实现用 `Type.GetType("TimerRunner, GrignardReagent.Timer")` 在运行时按字符串解析,
   而该程序集在 `AudioManager.Awake()` 时尚未加载 —— 恒为假阴性, 且失败分支什么也不打印.
   已改为直接读取 `TimerRunner.Instance`(Timer 包已将 `Instance` 改为 public).

2. **Awake 顺序竞争**: `AudioManager` 与 `TimerRunner` 同在一个 GameObject 上, Unity 不保证两者 `Awake()` 的先后,
   因此 `AudioManager.Awake()` 可能先跑并读到 `null`. 已把宿主校验移到 `Start`, 池也在校验通过后才建立.

3. **`AudioEmitter.Play()` 的重入守卫**: 池化复用时, 上一次播放尚未结束, `Play()` 因 `isPlaying` 为真而静默返回.
   已改为 `Play()` 首先调用 `Stop()` 终结在飞的播放.(**注意**: 它**不是**池枯竭的原因, 见第 4 条.)

4. **`OnEmitterRelease` 从未归还 emitter**(池枯竭的真正原因): 原始实现用 `emitter.onAudioFinished += emitterPool.Release`
   直接归还池, 我的改写把这行替换成了一个**不含 `Release` 调用**的处理器, 于是池的租约永不回滚.
   已在该处理器末尾补上 `emitterPool.Release(emitter)`.

5. **归还调用与归还回调互相递归**(上一条修复引发的回归): `OnEmitterRelease` 同时是池的归还回调,
   又被我改成了调用 `emitterPool.Release`, 于是 `OnEmitterRelease -> Release -> actionOnRelease -> OnEmitterRelease`
   无限递归, 栈溢出. 已拆成两个方法: `ReleaseEmitter`(只转发给池)与 `OnEmitterRelease`(只做清理, 绝不调用 Release).
   同时修掉一个潜在缺陷: `ReleaseEmitter` 此前只订阅不退订, 复用 emitter 时会累积订阅, 导致同一次结束被多次归还.

6. **空间化交付**: `AudioClipData` 新增 `SpatialBlend` / `MinDistance` / `MaxDistance`,
   `AudioEmitter.Configure()` 写入它们, 并新增 `SetPosition` / `AudioBuilder.WithPosition`.

### 请复验(按顺序)

- [x] **脚本编译已完成并核实**: 首次编译报 `CS0246`(`BoxGroup` 缺少 `using NaughtyAttributes;`), 已修正.
  修正后 `Assembly-CSharp.dll` @ `17:48:01`(`51712` -> `52736` 字节), 产物含 `SetPosition` / `WithPosition` /
  `SpatialBlend` / `MinDistance` / `MaxDistance`, 且导入日志无任何 `CS####` 诊断.

- [x] 在三个 `AudioClipData` 上按需设置 `SpatialBlend`(0 为纯 2D). 默认 `0`, 故现有听感不变;
  只有 `SpatialBlend > 0` 且**同时**给出 `WithPosition` 或 `WithFollowTarget` 时才会听到位置差异.

- [x] 确认 `Main Camera` 的 z 值(`-10`)带来的距离偏移可接受. 你已确认"计算不需太精确, 只做大致 spatial 效果",
  此处仅作提醒: 相机移动会改变音量, 因为唯一 `AudioListener` 在相机上而 `AudioRoot` 在原点.

- [x] **确认不再栈溢出**: 这是本轮的阻塞项. 你此前在 `AudioManager.cs:287` 临时注释掉的那行 `emitterPool.Release(emitter)`
  现在已被正确的两方法结构取代, **请撤掉那处注释** —— 保留它会导致永不归还.

- [x] 日志出现 `TimerRunner is available`(已确认).

- [x] **先用确定性断言检查归还**: 在 `Assets/Scripts/Core/GameObjectPool/GameObjectPool.cs:58` 的
  `actionOnRelease` 委托内打一个断点, 进入 Play 模式并触发一次播放, 确认该断点**在播放结束后命中**.
  这比"看日志里有没有某条消息"可靠得多, 也是最初定位本缺陷的手段.

- [x] **把场景覆盖的 `requestTime` 从 `10` 临时调回 `1`**, 然后确认:`AudioEmitter is released` 的次数与播放次数**一一对应**,
  而不是像此前那样恒为 `2`.
  说明: 此前的恒值 `2` 全部来自 `Prewarm`(`GameObjectPool.cs:99` 内部也会调用 `Release`), 与真实播放无关 ——
  **恒定的计数是可疑信号, 不是正常信号.**

- [x] 连点若干次, 确认不再出现 `Pool exhausted`.

- [x] 确认能正常发声. 注意 `requestTime = 10` 会在同一帧发出 10 个同 clip 请求, 听感上是叠加失真,
  这属于测试强度而非缺陷.

### 3. "Pool exhausted" 警告的正确读法 (此前写错, 特此更正)

**该警告是设计生效的证据, 不是缺陷.** 判据:

- 池上限为 `30`, 而 `Test Audio Request` 每次点击触发 `10` 次请求 —— 连点 3 次即达 30 并发.
- 第 31 个请求被拒绝并记录警告, 正是刻意加的硬上限. Unity 的 `ObjectPool` 自身在池空时**只会新建实例, 从不拒绝**, 因此这个上限必须由代码在 `AudioManager.ReserveEmitter` 里强制.
- 先前的验收条目写成"没有 Pool exhausted 警告"是**错误的**: 以这个测试强度, 该警告出现才是正确行为.

补充: 全局上限(30)与池上限(30)相等时, 抢占会在池耗尽**之前**介入 —— 每来一个新请求就先抢占最旧的**非循环**音, 释放一个槽位. 因此常规玩法下不应看到该警告; 它只在 30 个请求于**同一帧**内全部发出时出现(测试按钮正是这种形态, 因为抢占释放的 emitter 要到下一帧才真正回到池).

### 4. 本次不包含的缺口 (无需你操作, 仅备忘)

- [ ] 淡入淡出未实现: 抢占是硬切. 参见 CHANGELOG 的"本次不做"一节.
- [ ] 总线整体缩放未实现: Master.mixer 未暴露参数, 需要一次独立的 AudioMixer 配置交付.

## 历史

- 交付 (C) 的探针验证与提交清单已全部勾完并合并, 记录见 CHANGELOG 的"探针移除记录".