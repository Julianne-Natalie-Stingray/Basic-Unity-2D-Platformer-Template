using UnityEngine;
using System;

/// <summary>
/// 单个池化的音频声部: 播放一个 clip 并只报告一次结束.
/// 归属: Core (Audio).
/// 不负责: 播放策略, 实例上限与跟随目标的来源(归 AudioManager / AudioBuilder).
/// </summary>
[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class AudioEmitter : MonoBehaviour
{
    internal event Action<AudioEmitter> onAudioFinished;

    internal AudioId AudioId => data ? data.AudioId : AudioId.None;
    internal bool IsPlaying => source && source.isPlaying;
    internal bool IsFinished => isFinished;
    internal bool IsLooping => data && data.Loop;
    internal bool IsFollowingTarget => followTarget;

    internal float Volume
    {
        get => volume;
        set
        {
            volume = Mathf.Clamp(value, MinVolume, MaxVolume);
            isVolumeDirty = true;
        }
    }

    internal float Pitch
    {
        get => pitch;
        set
        {
            pitch = Mathf.Clamp(value, MinPitch, MaxPitch);

            InitializeInternal();
            source.pitch = pitch;
        }
    }

    /// <summary>本次播放是否在游戏冻结时被保留; clip 仅提供静态初值, 该值归发起播放的一方所有.</summary>
    internal bool SurviveFreeze
    {
        get => surviveFreeze;
        set
        {
            surviveFreeze = value;

            InitializeInternal();
            source.ignoreListenerPause = surviveFreeze;
        }
    }

    private AudioSource source;
    private AudioClipData data;

    private Transform followTarget;
    private Vector3 lastTargetPosition;

    private float volume = 1f;
    private float pitch = 1f;
    private bool isPlaying;
    private bool isFinished;
    private bool surviveFreeze;
    private bool isVolumeDirty;

    private const float MinVolume = 0f;
    private const float MaxVolume = 1f;
    private const float MinPitch = 0.1f;
    private const float MaxPitch = 3f;

    private bool willFollowTarget;

    private void Awake()
    {
        InitializeInternal();
    }

    private void LateUpdate()
    {
        if (!IsPlaying)
            return;

        if (isVolumeDirty)
        {
            source.volume = volume;
            isVolumeDirty = false;
        }

        if (!willFollowTarget)
            return;

        SyncPosition();
    }

    /// <summary>只写入 clip 拥有的静态特征; 音量与音高属于单次播放, 在 Play 中确立.</summary>
    internal void Configure(AudioClipData data)
    {
        InitializeInternal();

        this.data = data;

        if (!data)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.IsNull(nameof(data)))
                .Action(LogAction.Ignore)
                .Write();

            return;
        }

        source.clip = data.Clip;
        source.loop = data.Loop;
        source.outputAudioMixerGroup = data.MixerGroup;

        source.spatialBlend = data.SpatialBlend;
        source.minDistance = data.MinDistance;
        source.maxDistance = data.MaxDistance;

        // clip 只提供静态初值; 在此写入使刚配置好的 emitter 在按次覆盖生效前也自洽.
        SurviveFreeze = data.DefaultSurviveFreeze;
    }

    /// <summary>会先用 Stop 结束仍在进行中的播放以便完成回调执行, 使池能对已交出的 emitter 记账; 循环 clip 不装载结束 Timer.</summary>
    internal void Play(float? volumeOverride, float? pitchOverride)
    {
        Stop();

        InitializeInternal();
        source.Stop();

        isPlaying = true;
        isFinished = false;

        // 未覆盖时回落到 clip 的 baseline; 覆盖值只对本次播放有效.
        volume = Mathf.Clamp(volumeOverride ?? (data ? data.Volume : 1f), MinVolume, MaxVolume);
        pitch = Mathf.Clamp(pitchOverride ?? (data ? data.Pitch : 1f), MinPitch, MaxPitch);

        source.volume = volume;
        source.pitch = pitch;
        isVolumeDirty = false;

        if (!source.clip)
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned("clip"))
                .Action(LogAction.Ignore)
                .Write();

            // 仍要报告结束, 否则 AudioManager 已登记并订阅的池租约永远不会归还.
            Complete(false);
            return;
        }

        source.Play();

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Audio {AudioId} played. "))
            .Write();

        if (data && data.Loop)
            return;

        _ = new Timer(Mathf.Infinity)
            .CompleteWhen(() => !source.isPlaying)
            .OnComplete(StopNaturally)
            .UseUnscaledTime()
            .Start();
    }

    /// <summary>自然播完必须与显式 Stop 区分, 否则句柄的 IsFinished 永远为假.</summary>
    private void StopNaturally()
    {
        if (!isPlaying)
            return;

        InitializeInternal();
        source.Stop();
        Complete(true);
    }

    /// <summary>幂等; 提前与自然结束都只经此报告一次, 因此句柄的 Stop 与抢占不会把同一次播放报告两次.</summary>
    internal void Stop()
    {
        if (!isPlaying)
            return;

        InitializeInternal();
        source.Stop();
        Complete(false);
    }

    /// <summary>只写世界坐标, 不触碰跟随状态; spatialBlend 大于零的 clip 必须靠它或 SetFollowTarget 才从非听者位置发声.</summary>
    internal void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>保存目标并在 LateUpdate 跟踪, 不改变父子关系, 因此 emitter 不随目标销毁.</summary>
    internal void SetFollowTarget(Transform target)
    {
        if (!target)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.IsNull(nameof(target)))
                .Action(LogAction.Ignore)
                .Write();

            followTarget = null;
            return;
        }

        followTarget = target;
        lastTargetPosition = target.position;
        transform.position = lastTargetPosition;
        willFollowTarget = true;
    }

    /// <summary>清空单次播放数据与跟随目标并恢复默认音量音高, 使之后的播放不继承上一次的任何东西.</summary>
    internal void ResetEmitter()
    {
        InitializeInternal();

        source.Stop();
        source.clip = null;
        source.loop = false;

        data = null;
        followTarget = null;
        willFollowTarget = false;

        isPlaying = false;

        // isFinished 留给下一个读者(SoundHandle)消费; 下一次 Play 会重置它, 因此不在这里清.
        volume = 1f;
        pitch = 1f;
        surviveFreeze = false;
        source.volume = volume;
        source.pitch = pitch;
        source.ignoreListenerPause = false;
        isVolumeDirty = false;
    }

    private void Complete(bool finishedNaturally)
    {
        isPlaying = false;
        isFinished = finishedNaturally;

        onAudioFinished?.Invoke(this);
    }

    private void SyncPosition()
    {
        if (!followTarget)
        {
            HandleLostTarget();
            return;
        }

        lastTargetPosition = followTarget.position;
        transform.position = lastTargetPosition;
    }

    private void HandleLostTarget()
    {
        followTarget = null;
        transform.position = lastTargetPosition;
        willFollowTarget = false;

        if (IsLooping)
            Stop();
    }

    private void InitializeInternal()
    {
        if (!source)
            source = GetComponent<AudioSource>();
    }
}
