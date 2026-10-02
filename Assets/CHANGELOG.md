# CHANGELOG

## 2026-10-03 — 修正: 归还调用与归还回调互相递归导致栈溢出

**本条记录的是上一条修复所引发的回归.** 上一条补上了缺失的 `emitterPool.Release(emitter)`,
但插入的位置使它调用了自己, 造成无限递归. 现已改为把"归还输入"与"归还输出"彻底分开.

### 症状

Play 模式下栈溢出 (`StackOverflowException`). 用户已在 `AudioManager.cs:287` 临时注释掉该行止血,
并定位到 `AudioManager.cs:287` / `GameObjectPool.cs:57` / `GameObjectPool.cs:81` 三处互相调用.

### 根因 (DSH 的实现缺陷)

`GameObjectPool` 的构造函数接收一个 `onRelease` 回调, 而传进去的正是 `AudioManager.OnEmitterRelease`:

```csharp
emitterPool = new(..., OnEmitterRelease, ...);   // onRelease == OnEmitterRelease
```

于是上一条补的那行形成了闭环:

```
OnEmitterRelease(emitter)          // AudioManager.cs:271
  -> emitterPool.Release(emitter)  // AudioManager.cs:287   <- 上一条我加的那行
    -> pool.Release(item)          // GameObjectPool.cs:81
      -> actionOnRelease(item)     // GameObjectPool.cs:57
        -> onRelease?.Invoke(item) // 就是 OnEmitterRelease 本身
          -> (回到第一行)
```

**同一个方法身兼两职**: 它既是池的"归还输出"(清理), 又因为上一条的改动成了"归还输入"(发起归还).
方法开头的 `-= OnEmitterRelease` 挡不住它, 因为递归并不经过 `onAudioFinished`.

### 修法: 拆成两个方法, 各自单向

```csharp
// 归还输出: 池归还时执行的清理, 绝不调用 Release
private void OnEmitterRelease(AudioEmitter emitter) { ...清理与日志... }

// 归还输入: 订阅到 emitter 完成回调, 只负责转发给池
private void ReleaseEmitter(AudioEmitter emitter) => emitterPool.Release(emitter);
```

订阅点:

```csharp
emitter.onAudioFinished += ReleaseEmitter;      // 先: 触发池归还
emitter.onAudioFinished += OnEmitterRelease;    // 后: 清理与日志
```

订阅顺序是**有意义的**: C# 多播委托按订阅顺序调用, 因此池的归还(含停用 GameObject)先发生,
清理中的 `ResetEmitter` 与重挂父级随后执行. 若顺序颠倒, 清理会先改动一个随后才被停用的对象.

### 由此确立的工程规则

**注册为某个池"归还回调"的方法, 自身绝不能调用该池的 Release.**
当同一个位置既需要"被通知已归还"又需要"发起归还"时, 必须是两个方法.
这条规则与上一条("开始方法必须先终结上一次使用")合起来, 构成池化对象两侧的边界:
-- 取用侧: `Play` 必须先终结上一次使用;
-- 归还侧: 归还回调不得再发起归还.

### 验证

- 编译通过: `Assembly-CSharp.dll` 于 `03:40:27` 重建.
- 代码边界已逐行核对: `OnEmitterRelease`(270-285) 内不含任何 `emitterPool.Release`;
  全文件仅 `ReleaseEmitter`(296) 调用 `emitterPool.Release`, 且其唯一订阅者是 emitter 的完成回调.
- **尚未在 Play 模式下复验.** 请见 `Assets/PENDING_INSPECTOR.md`.

### 诚实记录 (第五次, 且是第二次自我回归)

上一条我修好了一个"缺失调用", 却因此引入了"无限递归". 这两件事同源:
我每次只盯着单个方法看, 没有画出**回调图**. 这次的教训具体到可执行:

**当 A 调用 B 之前, 必须确认 B 是否会回调 A.** 本缺陷只需画一张三个节点的图就能发现, 不需要运行. 至此五次缺陷中,
有两次(第 4 与第 5)完全可以在提交前靠"注解逐句对照代码"与"回调图"抓住.

---

## 2026-10-03 — 修正: OnEmitterRelease 从未归还 emitter (池枯竭的真正原因)

> **后续**: 本条补上的那行 `emitterPool.Release(emitter)` 因插入位置错误而引发无限递归,
> 见本文件更靠前的一条. 缺调用是真的, 但那次的插入方式是错的.

**本条更正上一条的结论.** 上一条把池枯竭归因于 `Play()` 的重入守卫, 那是错的 ——
守卫确实是个缺陷(已修), 但它不是池枯竭的原因. 真正的原因在本条.

### 症状

`AudioEmitter is released` 的日志会出现, 但池从不真正回收, 用尽后开始拒绝请求.
用户在 `GameObjectPool.cs:58` 的 `actionOnRelease` 上打断点发现: **该委托从未被调用**.

### 根因 (DSH 的实现缺陷)

对比原始实现与我的改写:

```csharp
// 原始实现 (HEAD)
emitter.onAudioFinished += emitterPool.Release;   // emitter 播完 -> 直接归还池

// 我的改写
private void OnEmitterRelease(AudioEmitter emitter)
{
    emitter.onAudioFinished -= OnEmitterRelease;
    registry.Unregister(emitter);
    emitter.ResetEmitter();
    emitter.transform.SetParent(emitterRoot, false);
    ...日志...
    // ← 这里没有 emitterPool.Release(emitter), 也没有任何其他地方调用它
}
```

整个 `AudioManager` 里只有 `Prewarm` / `CanReuse` / `Get`, **没有任何一处调用 `Release`**.
emitter 被交出后, 租约永不回滚: 池一直以为它们在外面, 直到 30 个名额用尽, 然后拒绝一切.

更糟的是, 我写的注解原文是"…so registry removal, the follow target reset and **the return to the pool**
happen exactly once per playback" —— **我把自己没有实现的行为写进了文档.**

### 为什么它被掩盖了两次

1. `GameObjectPool.Prewarm` 内部会调用 `pool.Release(...)`(`GameObjectPool.cs:99`),
   因此 `actionOnRelease` 在启动时**确实会触发两次**. 这就是为什么每一份日志里 `released` 恒为 `2` ——
   那两次全部来自预热, 与真实播放无关. 恒定值看起来像"机制在工作", 掩盖了它其实只对预热生效.
2. 上一条的诊断被这个假象带偏, 去修了一个真实但无关的守卫缺陷.

### 修法

在 `OnEmitterRelease` 末尾补上归还:

```csharp
emitterPool.Release(emitter);
```

位置在最后, 理由: 池自身的归还回调(`ResetEmitter` / 停用 GameObject)应当面对一个已经干净的 emitter;
而日志放在它之前, 以免那条消息看起来是由一个已入池的对象发出的.

### 验证

- 编译通过: `Assembly-CSharp.dll` 于 `03:20:05` 重建, Bee/Tundra 后端在同刻运行.
- **尚未在 Play 模式下复验**. 请见 `Assets/PENDING_INSPECTOR.md`.

### 本条的方法论教训

前三次我都说"编译通过不代表运行时正确". 这一次的教训不同, 更具体也更尴尬:

**我文档里承诺的行为, 必须在代码里找得到对应语句.** 这条注解明确写了"return to the pool",
而代码里没有 `Release` 调用 —— 这是一次纯粹的读写不一致, 不需要运行时就能发现,
只要在提交前把注解逐句对照代码就能抓住.

另有两点: 恒定的日志计数(此处恒为 2)是**可疑信号**, 不是正常信号;
以及"改写既有实现时, 必须逐条核对被替换掉的每一行原始行为" —— 我删掉了一行 `+= emitterPool.Release`,
并用一个不含归还的处理器替代了它.

---

## 2026-10-03 — 修正: AudioEmitter.Play 的重入守卫 (结论已被更正)

> **更正**: 本条原文声称重入守卫是池枯竭的原因. 那是误判.
> 枯竭的真正原因是 `OnEmitterRelease` 从未调用 `emitterPool.Release`, 见本文件更靠前的一条.
> 守卫本身确为缺陷, 已修, 但它不是枯竭的成因.

本次修掉第三个缺陷, 也是前两次修复之后**依然存在**的那个. 三者性质各不相同, 因此分三条记录.

### 症状

播放正常, `TimerRunner is available` 正常出现, 但 emitter 几乎不归还, 池被撑到上限后开始拒绝请求.

### 证据 (来自运行日志)

| 事件                              | 次数             |
| ------------------------------- | -------------- |
| `AudioEmitter is pooled`        | 2              |
| `Audio MouseClick played`       | 30             |
| `AudioEmitter is released`      | 2 (其中一次来自预热自身) |
| `Timer/<Run>d__34:MoveNext` 帧路径 | 1              |

场景覆盖 `requestTime` 为 `10`(由 `5` 改来), 因此 30 次播放 = 点击 3 次.

**矛盾点**: 30 次请求走到了 `Play()`, 但全生命周期只存在过 **2** 个 emitter. 结论只能是那 2 个被反复复用,
而每次复用都撞上了 `Play()` 开头的守卫:

```csharp
if (isPlaying)
    return;          // 上一次播放尚未结束, 于是静默返回
```

于是: 不启动播放, 不装载 Timer, 不触发 `Complete`, 不归还池. 池里因此堆着 30 个"在外面"的 emitter,
它们的租约永远不回滚.

### 根因 (DSH 的实现缺陷)

`Play()` 被写成了**重入守卫**, 而这个方法真实的语义应当是**替换当前播放**.
池化复用的 emitter 在"上一次播放尚未结束"时被再次交给 `Play()` 是**正常且必然**的,
静默返回等于让池的单向租约断裂.

### 修法

`Play()` 的第一件事改为 `Stop()`, 用它结束仍在进行中的播放:

```csharp
public void Play()
{
    Stop();          // 先结束在飞的播放, 让完成回调执行, 池才能记账

    InitializeInternal();
    source.Stop();

    isPlaying = true;
    ...
}
```

`Stop()` 本身幂等, 因此"新 emitter 首次播放"时它只是 `return`, 无副作用.
这条改动**不会**带来重复归还: 抢占与句柄 `Stop` 仍受同一幂等守卫保护.

### 由此确立的工程规则

**池化组件的"开始"方法必须先终结上一次使用.** 池化对象只被交出一次租约,
若它在被复用时因状态残留而拒绝对新请求提供服务, 池必然枯竭. 这不是防御性编程, 是池化成立的前提.

### 验证

- 编译通过: `Assembly-CSharp.dll` 于 `03:01:48` 重建, Unity 的 Bee/Tundra 构建后端在同刻运行.
- **尚未在 Play 模式下复验**. 请见 `Assets/PENDING_INSPECTOR.md`.

### 诚实记录 (第三次)

这是 DSH 连续第三个交付后暴露出、且都能通过编译的运行时缺陷. 三次的共性非常明确:

| #   | 缺陷                 | 为什么编译抓不到       |
| --- | ------------------ | -------------- |
| 1   | 反射解析尚未加载的程序集       | 运行时字符串, 非编译期符号 |
| 2   | 同物体组件 `Awake` 顺序竞争 | 生命周期顺序, 非类型错误  |
| 3   | `Play()` 重入守卫      | 复用路径, 首次播放永远正常 |

前两次我给出的教训是"要给时序论证"; 这一次的教训更具体: **池化路径必须专门验证"复用"这一条分支**,
只在对象上调用一次的方法永远不会暴露这类缺陷. 本工程后续任何池化组件的交付, 都应在验收清单里
显式包含"同一实例被复用"的用例.

---

## 2026-10-03 — 修正: AudioManager 与 TimerRunner 的 Awake 顺序竞争

上一条把反射改成直接读取之后, 立刻暴露出第二个缺陷. 两者是不同的问题, 因此分开记录.

### 症状

```
[Core: AudioManager] "TimerRunner" component is missing. Component disabled.
AudioManager:VerifyTimerRunner () (at Assets/Scripts/Core/Managers/AudioManager.cs:318)
AudioManager:InitializeInternal () (at Assets/Scripts/Core/Managers/AudioManager.cs:278)
AudioManager:Awake () (at Assets/Scripts/Core/Managers/AudioManager.cs:60)
```

### 根因 (DSH 的实现缺陷)

`AudioManager` 与 `TimerRunner` 挂在**同一个 GameObject** 上, 而 Unity **不保证同一物体上两个组件 `Awake()` 的先后**.
`TimerRunner.Instance` 只在自己的 `Awake()` 里赋值, 因此当 `AudioManager.Awake()` 先跑时, 读到的是 `null`.

这条与上一条的关键区别: 上一条是"程序集未加载", 这一条是"同物体组件初始化顺序" —— **直接引用并不能解决它**,
它独立存在, 只是此前被反射的假阴性掩盖了.

### 修法

- Timer 宿主的校验从 `Awake` 移到 **`Start`**: `Start` 在该物体上所有 `Awake()` 之后运行, 此时宿主必然已注册.
- `InitializeInternal()` 不再建池; 池改由 `Start` 在**校验通过之后**通过 `BuildEmitterPool()` 建立,
  因此不会为一个即将自禁的管理器建池.
- `Play()` 增加 `emitterPool == null` 守卫, 同时覆盖"`Start` 尚未运行"的时间窗与该自禁情形.
- 字段 `isTimerRunnerAvailable`(bool) 换成 `timerRunner`(引用) 并暴露为只读属性:
  它持有真正需要的依赖, 不再是只能读一次的布尔快照.

### 由此确立的工程规则

**跨组件协调的初始化一律放在 `Start`, 不放 `Awake`.** `Awake` 只解析自身依赖 (`GetComponent<>()` 等);
凡是需要另一个组件已经完成初始化的判断, 都必须等到 `Start`.

### 验证

- 编译通过: `Assembly-CSharp.dll` 于 `02:54:49` 重建, Unity 的 Bee/Tundra 构建后端在同刻运行.
- 编译产物中含 `BuildEmitterPool`, 且 `GetProperty` / `BindingFlags` 已消失, 证明反射路径彻底移除.

### 诚实记录

这是 DSH 连续第二次在"编译通过"之后仍无法保证运行时行为. 两次都不是编译错误, 而是**执行时序**问题.
因此本条目起, 涉及跨组件初始化的改动必须给出时序论证, 而不只是贴编译产物.

---

## 2026-10-03 — 修正: 移除 Timer 宿主的反射检查

本次改动修掉了音频交付里由 **DSH** 引入的一个缺陷. 它不是 Timer 包的问题.

### 症状

运行日志中从不出现 `TimerRunner is available`, 但同一次运行的调用栈显示
`Timer/<Run>d__34:MoveNext -> ForwardToCompletion -> Complete -> AudioEmitter.Stop`,
即 Timer 确实完成过并归还了 emitter. 两者矛盾, 说明检查本身在说假话.

### 根因 (DSH 的实现缺陷)

原实现用字符串在运行时解析类型:

```csharp
System.Type.GetType("TimerRunner, GrignardReagent.Timer")
```

`Type.GetType` 解析程序集限定名时要求该程序集**已经加载**. `GrignardReagent.Timer` 只在首次用到 `Timer` 时才加载,
而 `AudioManager.Awake()` 发生在任何 `Timer` 被实例化之前 —— 于是解析失败, 函数直接返回,
`isTimerRunnerAvailable` 恒为 `false`, 且那条 `false` 分支**什么也不打印**.

这正是"用反射绕开 internal"这一手段的固有代价: 它把一个编译期问题变成了运行时时序问题.

### 修法

- Timer 包已将 `TimerRunner.Instance` 从 `internal` 改为 `public`(包版本 `2e596da7b1` -> `621fd79a7a`).
- `AudioManager.VerifyTimerRunner` 改为**直接读取** `TimerRunner.Instance`, 反射与 `using System.Reflection` 一并删除.
  依赖因此回到编译期可见, 并且该读取本身会加载 Timer 程序集, 时序陷阱消失.
- 检查现在**两种结果都记录**: 可用时 `GameLog.Info`, 不可用时 `GameLog.Error` + `LogIssue.MissingComponent<TimerRunner>()` + 禁用组件.
  此前那条静默的 `false` 分支本身就是坏设计 —— 它让"永不归还 emitter"看起来像一个安静运行的系统.

### 验证

- 编译通过: `Assembly-CSharp.dll` 于 `02:47:11` 重建(Unity 的 Bee/Tundra 构建后端在同刻运行),
  体积由 `50176` 降为 `49664` 字节 —— 减少的正是被删掉的反射代码.
- 直接引用能编译, 本身即证明 `TimerRunner.Instance` 现在跨程序集可见.
- **尚未在 Play 模式下复验** `TimerRunner is available` 是否出现; 见 `Assets/PENDING_INSPECTOR.md`.

### 同时更正的一处文档错误

音频交付条目里把"没有 Pool exhausted 警告"写成了验收条件. 那是错的: 以 `Test Audio Request` 的测试强度
(每次点击 10 次请求, 池上限 30), 第 31 个请求被拒绝并告警**才是**设计生效的证据.
Unity 的 `ObjectPool` 在池空时只会新建实例而从不拒绝, 因此该硬上限必须由代码强制. 详见 `Assets/PENDING_INSPECTOR.md` 第 3 节.

---

## 2026-10-03 — 探针移除记录

本次改动是**记账**, 不是代码改动. 交付 (C) 的探针已在 `f14b94c` 之后的提交中移除, 但当时没有留下记录.

- `Assets/Scripts/Core/Diagnostics/` 整个目录已删除, 其中包含 `DshWriteProbe.cs` 与其 `.cs.meta`.
- 因为 `.cs` 与它的 `.meta` 当初是**成对提交**的, git 把两者记为同一次删除, 没有留下孤儿 `.meta`.
- 交付 (C) 建立的链路 (`DSH 写盘 -> Unity 生成 .meta -> 编译通过 -> git 可见 -> 提交`) 已在此前被端到端验证通过, 因此探针没有继续保留的价值.

无 Inspector 操作, 无新增文件.

---

## 2026-10-03 — 交付: Core 音频协调层 (AudioBuilder / SoundHandle / AudioRegistry)

### 范围

补齐 Core 音频子系统缺失的"协调"层: 单次播放的链式参数, 播放句柄, 以及两级实例上限与抢占.
参考实现 `adammyhre/Unity-Audio-Pooling` 只提供了链式位置与随机音高两项能力, 本交付按本工程的规范重新设计, 并修掉了它的若干缺陷 (见下).

### 被测/新增行为

- **逐次播放的参数**: 新增 `AudioBuilder` (struct), 提供 `WithVolume` / `WithPitch` / `WithRandomPitch` / `WithFollowTarget`, 终结符 `Play(AudioId)` 返回 `ISoundHandle`.
- **播放句柄**: 新增 `ISoundHandle` 与唯一实现 `SoundHandle`. 句柄在本次播放结束时**失效**, 因此池化 emitter 被下一次播放复用时, 旧句柄不会误报新播放的状态. 写入类操作是 `TrySetVolume` / `TrySetPitch`, 失败由返回值表达, 不静默吞掉.
- **两级实例上限与抢占**: 每 clip 上限来自 `AudioClipData.MaxInstances`, 全局上限来自 `AudioManagerConfigs.MaxSoundInstance`. 触顶时抢占**最旧且非循环**的声音. 循环音永不被抢占.
- **活跃注册表**: 新增 `AudioRegistry`, 负责注册, 注销, 按 `AudioId` 计数, 以及按序号回答"谁最旧". 顺序用序号而不是时间戳, 因为 `Time.time` 在同一帧内会重复.
- **延迟移除**: 注销只把槽位标记为空, 压实发生在下一次注册. 这是因为停止一个声音会同步触发注销, 若直接移除就会在抢占遍历中改动列表 —— 参考实现正是在这里崩过一次 (`StopAll` 的 `InvalidOperationException`).
- **位置跟随**: `AudioEmitter` 在 `LateUpdate` 中同步到 `WithFollowTarget` 传入的 `Transform`, 且**不改变父子关系**. 因此跟随的声音不随目标销毁而消失: 一次性音在目标最后位置播完, 循环音在目标消失时停止.
- **幂等 Stop**: `AudioEmitter.Stop()` 现在幂等, 使"句柄 Stop"与"被抢占"这两条会竞争的路径无法把同一次播放报告两次 (参考实现依赖 Editor-only 的 `collectionCheck` 掩盖此问题).
- **启动自检**: `AudioManager` 在初始化时确认 Timer 包的协程宿主存在. 该宿主缺失时 `onAudioFinished` 永不触发, emitter 会静默永久滞留在池外. (注: 本条的**初版**用反射读取该宿主, 那是 DSH 的实现缺陷, 已在本文件顶部"移除 Timer 宿主的反射检查"一条中修正.)
- **池上限硬约束**: `GameObjectPool` 新增 `CanReuse(maxSize)`. Unity 的 `ObjectPool` 在池空时会新建实例而**不会拒绝**, 因此"池达到 `MaxPoolSize` 后拒绝新请求并记警告"必须由我们自己判定.

### 修掉的两个既有缺陷

- `AudioManager.RequestAudio` 在 `TryGetClip` 失败时**缺少 return**: 它会继续 `emitter.Configure(null)` 并访问 `data.AudioId`, 必然抛 `NullReferenceException`. 新实现直接拒绝并返回 `null`.
- `AudioManagerConfigs.MaxSoundInstance` 与 `AudioClipData.MaxInstances` 此前**写入但从未被读取**. 现在两者都生效.

### 与参考实现的已知差异 (刻意为之)

- 参考实现的 `maxPoolSize` 只管空闲实例, 因此并发数不受它约束, 非 frequent 音效完全无上限. 本交付把"池大小"与"并发上限"显式分离, 并要求 `MaxPoolSize >= MaxSoundInstance`.
- 参考实现没有每 clip 上限, 没有句柄, 没有移动跟随, 也没有淡入淡出. 本交付补了前三项, **淡入淡出明确不在本次范围**.

### 本次不做 (已知缺口)

- **淡入淡出**: 抢占是硬切. `SoundHandle.TrySetVolume` 已经提供了驱动器, 但淡入淡出属于一次独立交付.
- **总线整体缩放**: `Master.mixer` 未暴露任何参数, 因此代码层面无法调总线音量. 正确落点是 AudioMixer 的 exposed parameter, 属于一次需要你在 Inspector 配置的独立交付.
- **每 clip 上限对循环音无效**: 循环音既不被抢占, 也不能靠上限回收. 若某循环 clip 的 `MaxInstances` 被设为 1 而它已在播放, 后续请求仍会启动第二个实例 (因为唯一候选受害者是循环音, 被跳过). 这是策略的直接后果, 记录在此以免被当作 bug.

### 需要你在 Inspector 里手动做的操作

- [x] 把 `Assets/SO/DefaultAudioManagerConfigs.asset` 的 `MaxPoolSize` 从 `10` 改为不小于 `MaxSoundInstance`(当前 `30`)的值. 当前 `10 < 30`, 会让第 11 个并发 emitter 播放后被销毁而不是回收.
- [x] 建议同时核对 `PrewarmAmount <= DefaultCapacity <= MaxPoolSize`; 当前三者都是 `10`.
- [x] 为每个 `AudioClipData` 资产填写新增的 `Volume` 与 `Pitch` 字段 (默认 `1` / `1`, 保持原听感即可).
- [x] 打开 Unity Console 确认没有编译错误, 尤其确认 `AudioBuilder` / `SoundHandle` / `AudioRegistry` / `ISoundHandle` 已成功导入 (它们的 `.meta` 是否已生成).
- [x] 进入 Play 模式, 用 `AudioManager` 上的 `Test Audio Request` 按钮验证播放与回收, 确认日志中没有 "Pool exhausted" 或缺失 TimerRunner 的报错.
- [x] 确认 `Assets/Prefabs/Core.prefab` 上 `AudioManager` 的 `emitterPrefab` / `emitterRoot` / `configs` 三个引用未被本次改动破坏 (它们没有动, 但值得复核一次).

### 新增文件及其待提交的 `.meta`

DSH 不得写入这些 `.meta` (§2.1.1); 它们由 Unity 生成, 且必须与所属文件**一起提交**.

- [x] `Assets/Scripts/Core/Audio/ISoundHandle.cs` + `ISoundHandle.cs.meta` (GUID `a2eb9ceb01910184fb16974c3ba1700b`)
- [x] `Assets/Scripts/Core/Audio/SoundHandle.cs` + `SoundHandle.cs.meta`
- [x] `Assets/Scripts/Core/Audio/AudioRegistry.cs` + `AudioRegistry.cs.meta`
- [x] `Assets/Scripts/Core/Audio/AudioBuilder.cs` + `AudioBuilder.cs.meta`

### 修改的文件

- [x] `Assets/Scripts/Core/Audio/AudioEmitter.cs` (跟随, 幂等 Stop, 实时音量音高, 类注解)
- [x] `Assets/Scripts/Core/Managers/AudioManager.cs` (两级上限, 抢占, Builder 工厂, 启动自检, 修 return 缺陷)
- [x] `Assets/Scripts/Core/Audio/SODefinitions/AudioClipData.cs` (新增 `Volume` / `Pitch`)
- [x] `Assets/Scripts/Core/Audio/SODefinitions/AudioManagerConfigs.cs` (暴露 `MaxSoundInstance`)
- [x] `Assets/Scripts/Core/GameObjectPool/GameObjectPool.cs` (新增 `CanReuse`)
- [x] `Assets/Scripts/Core/README.md` (纯中文重写 + 音频限流章节)
- [x] `Assets/CHANGELOG.md` (本文件)
- [x] `Assets/PENDING_INSPECTOR.md`
