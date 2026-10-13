/// <summary>
/// 单次音频播放的契约: 对单次播放的作用域控制(查询状态, 改音频参数, 提前停止).
/// 归属: Core (Audio).
/// 不负责: 由谁实现 —— 仅 SoundHandle 实现一次, 玩法代码永不实现它.
/// </summary>
public interface ISoundHandle
{
    /// <summary>本次播放所针对的 clip 标识; 句柄失效后仍可读.</summary>
    AudioId AudioId { get; }

    /// <summary>本次播放仍在进行时为真; 句柄失效后为 false, 且不会误报复用同一池化 emitter 的后一次播放.</summary>
    bool IsPlaying { get; }

    /// <summary>自然播完为真, 被打断为假; 打断包括显式 Stop, 被实例上限抢占, 以及跟随目标销毁时应用的循环策略.</summary>
    bool IsFinished { get; }

    /// <summary>本次播放因任何原因结束时触发一次; 触发时句柄已经失效.</summary>
    event System.Action<ISoundHandle> Finished;

    /// <summary>提前停止本次播放; 幂等, 对已结束的句柄返回 false.</summary>
    bool Stop();

    /// <summary>设置本次播放的实时音量; 句柄失效时返回 false, 越界值被钳制.</summary>
    bool TrySetVolume(float volume);

    /// <summary>设置本次播放的实时音高; 句柄失效时返回 false, 越界值被钳制.</summary>
    bool TrySetPitch(float pitch);
}
