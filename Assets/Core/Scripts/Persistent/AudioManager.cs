using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Core 的音频总线: 拥有 emitter 池, 活跃注册表与两级并发上限, 是启动声音的唯一入口.
/// 归属: Core (Audio).
/// 不负责: 请求的实时效果参数(归 AudioBuilder), clip 静态特征(归 AudioClipData)与播放本身(归 AudioEmitter).
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    internal AudioRegistry Registry => registry;

    [SerializeField] private AudioEmitter emitterPrefab;
    [SerializeField] private Transform emitterRoot;
    [SerializeField, Expandable] private AudioManagerConfigs configs;
    [SerializeField] private AudioMixer mixer;

    private ComponentPool<AudioEmitter> emitterPool;
    private AudioRegistry registry;
    private readonly Dictionary<AudioId, AudioClipData> cueIndex = new();

    private const string MasterVolumeParameter = "MasterVolume";
    private const string OstVolumeParameter = "OstVolume";
    private const string SfxVolumeParameter = "SfxVolume";
    private const float MinDecibelInput = 0.0001f;

    private void Awake()
    {
        InitializeInternal();
    }

    private void Start()
    {
        emitterPool.Prewarm(configs.PrewarmAmount);
        ApplySettingsFromStore();
        PlayStartupOst();
    }

    private void OnEnable()
    {
        GameStateManager.Changed += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.Changed -= HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        emitterPool?.Dispose();
    }

    /// <summary>发放一个全新凭证, 其参数独立于其他任何请求, 因此调用方不会继承更早播放的状态.</summary>
    public AudioBuilder CreateBuilder()
        => new AudioBuilder(this);

    /// <summary>把 0..1 归一化音量换算为分贝并写入混音器; 接收普通浮点以避免 Core 引用 Setting 子系统.</summary>
    public void ApplyAudioSettings(float masterVolume, float ostVolume, float sfxVolume)
    {
        if (mixer == null)
            return;

        mixer.SetFloat(MasterVolumeParameter, ToDecibels(masterVolume));
        mixer.SetFloat(OstVolumeParameter, ToDecibels(ostVolume));
        mixer.SetFloat(SfxVolumeParameter, ToDecibels(sfxVolume));
    }

    private static float ToDecibels(float volume)
    {
        return Mathf.Log10(Mathf.Clamp(volume, MinDecibelInput, 1f)) * 20f;
    }

    private void ApplySettingsFromStore()
    {
        var audio = SettingBootstrap.Settings.Audio;
        ApplyAudioSettings(audio.MasterVolume, audio.OstVolume, audio.SfxVolume);
    }

    /// <summary>启动声音的唯一入口, 由 AudioBuilder.Play 调用; 池不存在, 无 clip, 冻结门未放行或池耗尽时返回 null 并记录日志.</summary>
    internal ISoundHandle Play(
        AudioId requested,
        float? volume,
        float? pitch,
        Vector3? position,
        Transform followTarget,
        bool? surviveFreeze,
        bool? allowWhileFrozen)
    {
        if (!enabled || !configs || emitterPool == null)
            return null;

        AudioClipData data = ResolveClip(requested);

        if (!data)
            return null;

        bool willSurviveFreeze = surviveFreeze ?? data.DefaultSurviveFreeze;

        if (!IsFrozenEntryAllowed(requested, allowWhileFrozen, willSurviveFreeze))
            return null;

        ApplyInstanceLimits(requested, data.MaxInstances);

        AudioEmitter emitter = ReserveEmitter();
        if (!emitter)
            return null;

        registry.Register(emitter);
        emitter.gameObject.SetActive(true);
        emitter.onAudioFinished -= ReleaseEmitter;
        emitter.onAudioFinished -= OnEmitterRelease;
        emitter.onAudioFinished += ReleaseEmitter;
        emitter.onAudioFinished += OnEmitterRelease;
        emitter.Configure(data);

        emitter.SurviveFreeze = willSurviveFreeze;

        if (position.HasValue)
            emitter.SetPosition(position.Value);

        if (followTarget)
            emitter.SetFollowTarget(followTarget);

        SoundHandle handle = new SoundHandle(emitter, requested);

        emitter.Play(volume, pitch);

        return handle;
    }

    private AudioClipData ResolveClip(AudioId requested)
    {
        if (cueIndex.TryGetValue(requested, out AudioClipData data))
            return data;

        GameLog.Error(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.CannotFind(nameof(requested), nameof(configs)))
            .Action(LogAction.Ignore)
            .Write();

        return null;
    }

    private void BuildClipIndex()
    {
        cueIndex.Clear();

        if (!configs)
            return;

        foreach (AudioClipData data in configs.Audios)
        {
            if (!data)
            {
                GameLog.Warning(this)
                    .Subsystem("Core")
                    .Name(LogName.Class)
                    .Issue(LogIssue.NotAssigned(nameof(data)))
                    .Action(LogAction.Ignore)
                    .Write();
                continue;
            }

            AudioId id = data.AudioId;

            if (!id.IsValid)
            {
                GameLog.Warning(this)
                    .Subsystem("Core")
                    .Name(LogName.Class)
                    .Issue(LogIssue.Invalid($"{data.name}.{nameof(AudioId)}"))
                    .Action(LogAction.Ignore)
                    .Write();
                continue;
            }

            if (!cueIndex.TryAdd(id, data))
            {
                GameLog.Warning(this)
                    .Subsystem("Core")
                    .Name(LogName.Class)
                    .Issue(LogIssue.Specify($"Duplicate AudioId {id}; later entry ignored. "))
                    .Action(LogAction.Ignore)
                    .Write();
            }
        }
    }

    private void PlayStartupOst()
    {
        if (!playOnStart || !startupOst.IsValid)
            return;

        CreateBuilder()
            .WithSurviveFreeze(true)
            .WithAllowWhileFrozen(true)
            .Play(startupOst);
    }

    private bool IsFrozenEntryAllowed(AudioId requested, bool? allowWhileFrozen, bool willSurviveFreeze)
    {
        if (GameStateManager.Current != GameState.Freezed)
            return true;

        if ((allowWhileFrozen ?? false) && willSurviveFreeze)
            return true;

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify(willSurviveFreeze
                ? $"Request for {requested} refused: the game is frozen and the request did not allow entry. "
                : $"Request for {requested} refused: the game is frozen and the playback would not be kept. "))
            .Action(LogAction.Ignore)
            .Write();

        return false;
    }

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

    private AudioEmitter ReserveEmitter()
    {
        AudioEmitter emitter = emitterPool.Get();
        if (emitter != null)
            return emitter;

        GameLog.Warning(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify(
                $"Pool exhausted at {configs.MaxPoolSize} emitters; request refused. "))
            .Action(LogAction.Ignore)
            .Write();

        return null;
    }

    private void OnEmitterGet(AudioEmitter emitter)
    {
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify("AudioEmitter is pooled. "))
            .Write();
    }

    private void OnEmitterRelease(AudioEmitter emitter)
    {
        // 本方法即池的归还回调: 绝不能自己调用 ComponentPool.Release, 否则会经 actionOnRelease 递归到栈溢出.
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

    private void ReleaseEmitter(AudioEmitter emitter)
        => emitterPool.Release(emitter);

    private void HandleGameStateChanged(GameState state)
    {
        bool shouldPause = state == GameState.Freezed;

        if (AudioListener.pause == shouldPause)
            return;

        AudioListener.pause = shouldPause;

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify(shouldPause
                ? "Audio paused with the game. "
                : "Audio resumed with the game. "))
            .Write();
    }

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
            return;
        }

        emitterPool = BuildEmitterPool();
        BuildClipIndex();
    }

    private ComponentPool<AudioEmitter> BuildEmitterPool()
        => new(
            emitterPrefab,
            emitterRoot,
            configs.MaxPoolSize,
            configs.DefaultCapacity,
            configs.CollectionCheck,
            OnEmitterGet,
            OnEmitterRelease);

    [SerializeField] private AudioId startupOst = AudioId.None;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private AudioId debugAudioRequest;
    [SerializeField] private int requestTime = 1;
    [SerializeField] private bool DebugSurviveFreeze;
    [SerializeField] private bool DebugAllowWhileFrozen;

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
            CreateBuilder().WithSurviveFreeze(DebugSurviveFreeze)
                .WithAllowWhileFrozen(DebugAllowWhileFrozen)
                .Play(debugAudioRequest);
        
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Manual requested Audio: {debugAudioRequest}. \n" +
                                    $"Request time: {requestTime}\n" +
                                    $"Survive freeze: {DebugSurviveFreeze}\n" +
                                    $"Allow while frozen: {DebugAllowWhileFrozen}. "))
            .Write();
    }

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
