using UnityEngine;

/// <summary>
/// 单次播放请求的链式配置凭证: 承载音量, 音高与跟随目标, Play 之后即用尽.
/// 归属: Core (Audio).
/// 不负责: clip 的静态特征(归 AudioClipData)与实例上限(归 AudioManager).
/// </summary>
public struct AudioBuilder
{
    private AudioManager manager;
    private float volume;
    private float pitch;
    private Transform followTarget;
    private Vector3? position;
    private bool? surviveFreeze;
    private bool? allowWhileFrozen;
    private bool isUsed;

    internal AudioBuilder(AudioManager manager)
    {
        this.manager = manager;

        volume = 1f;
        pitch = 1f;
        followTarget = null;
        position = null;
        surviveFreeze = null;
        allowWhileFrozen = null;
        isUsed = false;
    }

    /// <summary>钳制发生在应用时而非此处, 因此链式调用中的中间值不会被截断.</summary>
    public AudioBuilder WithVolume(float volume)
    {
        if (isUsed)
            return this;

        this.volume = volume;
        return this;
    }

    /// <summary>钳制发生在应用时, 不在此处.</summary>
    public AudioBuilder WithPitch(float pitch)
    {
        if (isUsed)
            return this;

        this.pitch = pitch;
        return this;
    }

    /// <summary>为音高加小幅随机偏移, 避免重复播放时听感机械.</summary>
    public AudioBuilder WithRandomPitch(float min = -0.05f, float max = 0.05f)
    {
        if (isUsed)
            return this;

        pitch += Random.Range(min, max);
        return this;
    }

    /// <summary>位置与跟随互斥: 跟随每帧驱动位置, 因此两者都传时跟随胜出; 都不传则 emitter 停在 AudioRoot.</summary>
    public AudioBuilder WithPosition(Vector3 position)
    {
        if (isUsed)
            return this;

        this.position = position;
        return this;
    }

    /// <summary>emitter 在 LateUpdate 同步且不做目标的子物体, 因此目标销毁后声音存续: 一次性音在原地播完, 循环音停止.</summary>
    public AudioBuilder WithFollowTarget(Transform target)
    {
        if (isUsed)
            return this;

        followTarget = target;
        return this;
    }

    /// <summary>仅对本次请求覆盖冻结时是否保留播放; 不设置则用 clip 的静态初值.</summary>
    public AudioBuilder WithSurviveFreeze(bool survive)
    {
        if (isUsed)
            return this;

        surviveFreeze = survive;
        return this;
    }

    /// <summary>仅对本次请求覆盖冻结期间是否被受理; 与 WithSurviveFreeze 是不同的问题(后者管已播放的音是否保留).</summary>
    public AudioBuilder WithAllowWhileFrozen(bool allow)
    {
        if (isUsed)
            return this;

        allowWhileFrozen = allow;
        return this;
    }

    /// <summary>把承载的参数交给 AudioManager, 由其应用实例上限并可能抢占; 请求被拒绝时返回 null.</summary>
    public ISoundHandle Play(AudioId audioId)
    {
        if (isUsed || !manager)
            return null;

        isUsed = true;

        return manager.Play(audioId, volume, pitch, position, followTarget, surviveFreeze, allowWhileFrozen);
    }
}
