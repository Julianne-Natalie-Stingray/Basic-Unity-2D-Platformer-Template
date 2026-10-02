using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Audio bus of the Core subsystem. It owns the emitter pool, the active registry, and the two concurrency
/// limits, and it is the only entry point for starting a sound.
/// Subsystem: Core (Audio).
/// Where it lives: on the Core GameObject, required by CoreFacade and reached through CoreFacade.Audio.
/// Responsibility: resolve an AudioId to its clip data, enforce the per-clip and global instance limits by
/// preempting the oldest eligible sound, own the emitter pool and the registry, and refuse a request with a
/// log entry when it cannot be served.
/// Does NOT own: the live-effect parameters of a request (AudioBuilder carries them), the static
/// characteristics of a clip (AudioClipData owns them), the playback itself (AudioEmitter), or the start
/// order bookkeeping (AudioRegistry).
/// Lifetime: created with the Core GameObject. The pool is built in InitializeInternal and prewarmed in
/// Start; emitters live until this manager is destroyed.
/// Limits model: two independent limits. The global cap protects the total voice count and comes from
/// AudioManagerConfigs.MaxSoundInstance; the per-clip cap expresses design intent and comes from
/// AudioClipData.MaxInstances. Neither is the pool size: maxPoolSize bounds only how many emitters are kept
/// for reuse, so it must be at least as large as the global cap, otherwise surplus emitters are destroyed
/// instead of recycled. A looping sound is never preempted, because it is usually music or ambience and
/// should not be cut off by ordinary sound effects.
/// Paradigms: none. It is a plain MonoBehaviour service, reached through CoreFacade rather than a Singleton.
/// Core 子系统的音频总线. 它拥有 emitter 池, 活跃注册表与两级并发上限, 并且是启动声音的唯一入口.
/// Subsystem 归属: Core (Audio).
/// 存在位置: Core GameObject 上, 由 CoreFacade 要求, 并通过 CoreFacade.Audio 访问.
/// 职能: 把 AudioId 解析为 clip 数据; 通过抢占最旧且可抢占的声音执行每 clip 与全局两级实例上限;
/// 拥有 emitter 池与注册表; 无法服务某请求时记录日志并拒绝.
/// 不负责: 请求的实时效果参数(由 AudioBuilder 承载), clip 的静态特征(归 AudioClipData),
/// 播放本身(归 AudioEmitter), 以及开始顺序记账(归 AudioRegistry).
/// 生命周期: 随 Core GameObject 创建. 池在 InitializeInternal 中构建, 在 Start 中预热;
/// emitter 存活到本管理器销毁.
/// 上限模型: 两个互相独立的上限. 全局上限保护总声部数, 来自 AudioManagerConfigs.MaxSoundInstance;
/// 每 clip 上限表达设计意图, 来自 AudioClipData.MaxInstances. 两者都不是池大小:
/// maxPoolSize 只界定保留多少 emitter 用于复用, 因此它必须不小于全局上限,
/// 否则多余的 emitter 会被销毁而不是回收. 循环音永不被抢占, 因为它通常是音乐或环境音,
/// 不应被普通音效打断.
/// 使用范式: 无. 它是普通 MonoBehaviour 服务, 通过 CoreFacade 而非单例访问.
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public AudioRegistry Registry => registry;

    /// <summary>
    /// The coroutine host the Timer package needs. Null until Start has verified it, and null forever when
    /// the host is absent, in which case no emitter can ever be returned to the pool.
    /// Timer 包所需的协程宿主. 在 Start 校验之前为空; 宿主不存在时永远为空, 那种情况下没有任何 emitter 能被归还到池.
    /// </summary>


    [SerializeField] private AudioEmitter emitterPrefab;
    [SerializeField] private Transform emitterRoot;
    [SerializeField, Expandable] private AudioManagerConfigs configs;

    private GameObjectPool<AudioEmitter> emitterPool;
    private AudioRegistry registry;
    private TimerRunner timerRunner;

    private const float MinVolume = 0f;
    private const float MaxVolume = 1f;
    private const float MinPitch = 0.1f;
    private const float MaxPitch = 3f;

    private void Awake()
    {
        InitializeInternal();
    }

    /// <summary>
    /// Single entry point for finishing setup that depends on other components being awake.
    /// Implementation approach: the Timer host is verified here rather than in Awake, because AudioManager
    /// and TimerRunner live on the same GameObject and Unity does not order two components' Awake calls.
    /// Start runs after every Awake on the object, so the host has always registered itself by now.
    /// 完成依赖其他组件已 Awake 的装配的单一入口.
    /// 实现思路: Timer 宿主的校验放在这里而不是 Awake, 因为 AudioManager 与 TimerRunner 在同一个 GameObject 上,
    /// 而 Unity 不保证两个组件 Awake 的先后. Start 在物体上所有 Awake 之后运行, 此时宿主必然已注册.
    /// </summary>
    private void Start()
    {
        timerRunner = TimerRunner.Instance;

        if (!timerRunner)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.MissingComponent<TimerRunner>())
                .Action(LogAction.DisableComponent)
                .Write();

            enabled = false;
            return;
        }

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify("TimerRunner is available. "))
            .Write();

        emitterPool = BuildEmitterPool();
        emitterPool.Prewarm(configs.PrewarmAmount);
    }

    private void OnDestroy()
    {
        emitterPool?.Clear();
    }

    /// <summary>
    /// Single entry point for obtaining a request token.
    /// Implementation approach: hands out a fresh value whose parameters are independent of every other
    /// request, so no caller can inherit state from an earlier playback.
    /// 获取请求凭证的单一入口.
    /// 实现思路: 发放一个全新值, 其参数独立于其他任何请求, 因此没有调用方能继承更早播放的状态.
    /// </summary>
    public AudioBuilder CreateBuilder()
        => new AudioBuilder(this);

    /// <summary>
    /// Single entry point for starting a sound. Called by AudioBuilder.Play, not by gameplay code.
    /// Implementation approach: refuses the request while the pool does not exist, which covers both the
    /// window before Start has run and the case where Start disabled this manager over a missing Timer host.
    /// It then validates the request, applies both instance limits by preempting the oldest eligible sound,
    /// reserves an emitter, registers it, and starts it. Returns null and logs when the request is refused,
    /// rather than silently dropping it.
    /// 启动声音的单一入口. 由 AudioBuilder.Play 调用, 不由玩法代码调用.
    /// 实现思路: 池尚不存在时直接拒绝 —— 这同时覆盖 Start 运行前的时间窗, 以及 Start 因缺少 Timer 宿主
    /// 而禁用本管理器的情况. 之后校验请求, 通过抢占最旧且可抢占的声音执行两级实例上限,
    /// 预定 emitter, 注册它并启动. 请求被拒绝时返回 null 并记录日志, 而不是静默丢弃.
    /// </summary>
    public ISoundHandle Play(AudioId requested, float volume, float pitch, Transform followTarget)
    {
        if (!enabled || !configs || emitterPool == null)
            return null;

        if (!configs.TryGetClip(requested, out AudioClipData data))
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.CannotFind(nameof(requested), nameof(configs)))
                .Action(LogAction.Ignore)
                .Write();

            return null;
        }

        ApplyInstanceLimits(requested, data.MaxInstances);

        AudioEmitter emitter = ReserveEmitter();
        if (!emitter)
            return null;

        emitter.Volume = Mathf.Clamp(volume, MinVolume, MaxVolume);
        emitter.Pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);

        registry.Register(emitter);
        emitter.gameObject.SetActive(true);
        emitter.onAudioFinished -= ReleaseEmitter;
        emitter.onAudioFinished -= OnEmitterRelease;
        emitter.onAudioFinished += ReleaseEmitter;
        emitter.onAudioFinished += OnEmitterRelease;
        emitter.Configure(data);

        if (followTarget)
            emitter.SetFollowTarget(followTarget);

        SoundHandle handle = new SoundHandle(emitter, requested);

        emitter.Play();

        return handle;
    }

    /// <summary>
    /// Single entry point for preempting whatever the instance limits require.
    /// Implementation approach: applies the per-clip limit first and the global limit second, and lets the
    /// ordinary completion path do the rest of the work, so pool release, registry removal and handle
    /// invalidation all happen in one place. A looping sound is never chosen.
    /// 按实例上限执行抢占的单一入口.
    /// 实现思路: 先应用每 clip 上限, 再应用全局上限, 其余工作交给常规结束路径,
    /// 因此池归还, 注册表注销与句柄失效都只发生在一处. 循环音永不被选中.
    /// </summary>
    private void ApplyInstanceLimits(AudioId requested, int maxInstances)
    {
        if (registry.CountOf(requested) >= maxInstances
            && registry.TryGetOldest(requested, out AudioEmitter sameClipVictim))
        {
            Preempt(sameClipVictim, requested);
        }

        if (registry.Count >= configs.MaxSoundInstance
            && registry.TryGetOldest(out AudioEmitter globalVictim))
        {
            Preempt(globalVictim, requested);
        }
    }

    /// <summary>
    /// Single entry point for stopping one sound so its slot can serve a new request.
    /// Implementation approach: a looping victim is refused rather than preempted, so a looping clip that
    /// happens to be the oldest is passed over instead of being cut off.
    /// 停止一个声音以便其槽位服务新请求的单一入口.
    /// 实现思路: 拒绝抢占循环音的受害者, 因此恰好最旧的循环 clip 会被跳过, 而不是被切断.
    /// </summary>
    private void Preempt(AudioEmitter victim, AudioId requested)
    {
        if (!victim || victim.IsLooping)
            return;

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Preempted {victim.AudioId} for {requested}. "))
            .Write();

        victim.Stop();
    }

    /// <summary>
    /// Single entry point for reserving an emitter without letting the pool grow past maxPoolSize.
    /// Implementation approach: Unity's ObjectPool creates a new instance instead of refusing when it is
    /// empty, so the cap is enforced through GameObjectPool.CanReuse: when nothing is idle and maxPoolSize
    /// emitters already exist, the request is refused with a warning rather than silently expanding the pool.
    /// 预定 emitter 且不让池超过 maxPoolSize 的单一入口.
    /// 实现思路: Unity 的 ObjectPool 在池空时会新建实例而不是拒绝, 因此上限通过
    /// GameObjectPool.CanReuse 强制: 当没有空闲实例且已达到 maxPoolSize 时,
    /// 拒绝该请求并记录警告, 而不是静默扩容.
    /// </summary>
    private AudioEmitter ReserveEmitter()
    {
        if (!emitterPool.CanReuse(configs.MaxPoolSize))
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify(
                    $"Pool exhausted at {configs.MaxPoolSize} emitters; request refused. "))
                .Action(LogAction.Ignore)
                .Write();

            return null;
        }

        return emitterPool.Get();
    }

    private void OnEmitterGet(AudioEmitter emitter)
    {
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify("AudioEmitter is pooled. "))
            .Write();
    }

    /// <summary>
    /// Single entry point for the cleanup that follows a finished playback.
    /// Implementation approach: this method runs as the pool's own release callback, so it must never call
    /// GameObjectPool.Release itself. Doing so re-enters this method through actionOnRelease and recurses until
    /// the stack overflows, because the handler passed to the pool constructor is this very method. Returning
    /// the emitter to the pool is therefore driven from outside: ReleaseEmitter is subscribed to the emitter's
    /// completion callback, so the pool's Release is invoked exactly once per playback. What remains here is
    /// cleanup only -- unsubscribe, drop the registry entry, reset the emitter, reparent it, and log.
    /// 单次播放结束后的清理的单一入口.
    /// 实现思路: 本方法作为池自身的归还回调运行, 因此**绝不能**自己调用 GameObjectPool.Release.
    /// 传给池构造函数的处理器就是这个方法本身, 自调用会经 actionOnRelease 重新进入本方法, 一路递归到栈溢出.
    /// 归还池的动作因此由外部驱动: 把 ReleaseEmitter 订阅到 emitter 的完成回调上, 使池的 Release 每次播放恰好调用一次.
    /// 留在这里的只有清理 —— 退订, 注销注册表项, 重置 emitter, 重挂父级, 记录日志.
    /// </summary>
    private void OnEmitterRelease(AudioEmitter emitter)
    {
        emitter.onAudioFinished -= ReleaseEmitter;
        emitter.onAudioFinished -= OnEmitterRelease;

        registry.Unregister(emitter);

        emitter.ResetEmitter();
        emitter.transform.SetParent(emitterRoot, false);
        emitter.transform.localPosition = Vector3.zero;

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify("AudioEmitter is released. "))
            .Write();
    }

    /// <summary>
    /// Single entry point for the emitter's completion callback.
    /// Implementation approach: a thin forwarder to GameObjectPool.Release, deliberately separate from
    /// OnEmitterRelease so that the pool's release input and its release output can never call each other.
    /// 处理 emitter 完成回调的单一入口.
    /// 实现思路: 一个转发到 GameObjectPool.Release 的薄方法, 刻意与 OnEmitterRelease 分开,
    /// 使池的"归还输入"与"归还输出"永远不会互相调用.
    /// </summary>
    private void ReleaseEmitter(AudioEmitter emitter)
        => emitterPool.Release(emitter);

    private void InitializeInternal()
    {
        registry = new AudioRegistry();

        if (!emitterPrefab)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(emitterPrefab)))
                .Action(LogAction.DisableGameObject)
                .Write();
            gameObject.SetActive(false);
        }

        if (!emitterRoot)
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(emitterRoot)))
                .Action(LogAction.UseFallbackValue("self transform"))
                .Write();
            emitterRoot = transform;
        }

        if (!configs)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(configs)))
                .Action(LogAction.DisableComponent)
                .Write();
            enabled = false;
        }
    }

    /// <summary>
    /// Single entry point for assembling the emitter pool.
    /// Implementation approach: called from Start rather than Awake, so the pool is never built for a manager
    /// that is about to disable itself over a missing Timer host.
    /// 装配 emitter 池的单一入口.
    /// 实现思路: 由 Start 调用而非 Awake, 因此不会为一个即将因缺少 Timer 宿主而自禁的管理器建池.
    /// </summary>
    private GameObjectPool<AudioEmitter> BuildEmitterPool()
        => new(
            emitterPrefab,
            emitterRoot,
            OnEmitterGet,
            OnEmitterRelease,
            configs.CollectionCheck,
            configs.DefaultCapacity,
            configs.MaxPoolSize);


#region Debug

    [SerializeField] private AudioId debugAudioRequest;
    [SerializeField] private int requestTime = 1;

    [Button("Test Audio Request")]
    private void TestAudioRequest()
    {
        if (!Application.isPlaying)
        {
            GameLog.Info(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("Please run in play mode."))
                .Action(LogAction.Return)
                .Write();
            return;
        }

        for (int i = 0; i < requestTime; i++)
            CreateBuilder().Play(debugAudioRequest);
    }

#endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (requestTime <= 0)
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Invalid(nameof(requestTime)))
                .Action(LogAction.UseFallbackValue("1"))
                .Write();
            requestTime = 1;
        }
    }
#endif
}
