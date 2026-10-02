using UnityEngine;

/// <summary>
/// Fluent, single-use configuration token for one playback request.
/// Subsystem: Core (Audio).
/// Where it lives: nowhere. It is a value created on demand by AudioManager.CreateBuilder and discarded
/// after Play.
/// Responsibility: carry the live-effect parameters of one request -- volume, pitch, an optional random
/// pitch jitter, and the Transform the sound should follow -- and hand that request to AudioManager.
/// Does NOT own: the clip, the mixer group, or whether the clip loops. Those are static characteristics of
/// AudioClipData, which is their single source of truth; a request that needs the same clip to loop in one
/// place and not in another is expressed as two AudioClipData assets, not as a builder switch.
/// It also does not own the instance limits or preemption, which are AudioManager's policy.
/// Lifetime: one instance per request. It is a struct, so the chain returns a copy and no request can leak
/// its parameters into the next one; that also removes the reference implementation's cached-builder trap,
/// where a stale position or pitch silently applied to every later playback.
/// After Play the token is spent: any further With call is ignored and Play refuses to start a second sound.
/// Paradigms: Fluent API over a value type.
/// 单次播放请求的链式配置凭证.
/// Subsystem 归属: Core (Audio).
/// 存在位置: 无. 它是按需创建的值, 由 AudioManager.CreateBuilder 产生, Play 之后即丢弃.
/// 职能: 承载一次请求的实时效果参数 —— 音量, 音高, 可选的随机音高抖动, 以及该声音应跟随的 Transform ——
/// 并把该请求交给 AudioManager.
/// 不负责: clip, mixerGroup, 以及该 clip 是否循环. 这些是 AudioClipData 的静态特征, 以它为唯一真相来源;
/// 若同一 clip 需要在某处循环而另一处不循环, 正确做法是建两个 AudioClipData 资产, 而不是给 Builder 加开关.
/// 也不负责实例上限与抢占, 那属于 AudioManager 的策略.
/// 生命周期: 每次请求一个实例. 它是 struct, 因此链式调用返回的是副本, 任何请求的参数都无法泄漏到下一个;
/// 这也消除了参考实现里缓存 Builder 的陷阱 —— 过期的位置或音高会静默应用到之后每一次播放.
/// Play 之后该凭证即用尽: 再调用任何 With 都被忽略, Play 也拒绝启动第二个声音.
/// 使用范式: 值类型上的 Fluent API.
/// </summary>
public struct AudioBuilder
{
    private AudioManager manager;
    private float volume;
    private float pitch;
    private Transform followTarget;
    private bool isUsed;

    internal AudioBuilder(AudioManager manager)
    {
        this.manager = manager;

        volume = 1f;
        pitch = 1f;
        followTarget = null;
        isUsed = false;
    }

    /// <summary>
    /// Set the live volume of this request. Clamped when it is applied, not here, so that intermediate
    /// values in a chain are never truncated.
    /// 设置本次请求的实时音量. 钳制发生在最终应用时, 不在此处, 因此链式调用中的中间值不会被截断.
    /// </summary>
    public AudioBuilder WithVolume(float volume)
    {
        if (isUsed)
            return this;

        this.volume = volume;
        return this;
    }

    /// <summary>
    /// Set the live pitch of this request. Clamped when it is applied, not here.
    /// 设置本次请求的实时音高. 钳制发生在最终应用时, 不在此处.
    /// </summary>
    public AudioBuilder WithPitch(float pitch)
    {
        if (isUsed)
            return this;

        this.pitch = pitch;
        return this;
    }

    /// <summary>
    /// Add a small random offset to the pitch of this request, for clips that would otherwise sound
    /// mechanical when repeated.
    /// 为本次请求的音高加上小幅随机偏移, 用于避免重复播放时听感机械.
    /// </summary>
    public AudioBuilder WithRandomPitch(float min = -0.05f, float max = 0.05f)
    {
        if (isUsed)
            return this;

        pitch += Random.Range(min, max);
        return this;
    }

    /// <summary>
    /// Make this sound follow a Transform. The emitter syncs to it from LateUpdate and is never parented to
    /// it, so the sound survives the target being destroyed: a one-shot plays out where the target was last
    /// seen, while a loop stops, because a loop has no natural end and would hold a pooled emitter forever.
    /// 让该声音跟随某个 Transform. emitter 在 LateUpdate 中同步, 且永不做它的子物体,
    /// 因此声音能在目标销毁后存续: 一次性音在目标最后出现的位置播完, 循环音则停止 ——
    /// 因为循环没有自然结尾, 会永久占住一个池化 emitter.
    /// </summary>
    public AudioBuilder WithFollowTarget(Transform target)
    {
        if (isUsed)
            return this;

        followTarget = target;
        return this;
    }

    /// <summary>
    /// Single entry point for submitting this request.
    /// Implementation approach: forwards every carried parameter to AudioManager, which applies the instance
    /// limits, may preempt an older sound, and returns a handle scoped to the playback it started.
    /// Returns null when the request was refused.
    /// 提交本次请求的单一入口.
    /// 实现思路: 把承载的每个参数转交 AudioManager; 由它应用实例上限, 可能抢占更旧的声音,
    /// 并返回一个作用于它所启动那次播放的句柄. 请求被拒绝时返回 null.
    /// </summary>
    public ISoundHandle Play(AudioId audioId)
    {
        if (isUsed || !manager)
            return null;

        isUsed = true;

        return manager.Play(audioId, volume, pitch, followTarget);
    }
}
