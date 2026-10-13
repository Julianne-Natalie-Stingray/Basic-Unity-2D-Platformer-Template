using System;

/// <summary>
/// ISoundHandle 的唯一实现: 把一次播放钉在一个池化 AudioEmitter 上.
/// 归属: Core (Audio).
/// 不负责: 池化, 实例上限, 注册表, 以及任何抢占决策.
/// </summary>
public sealed class SoundHandle : ISoundHandle
{
    public AudioId AudioId => audioId;
    public bool IsPlaying => isValid && emitter.IsPlaying;
    public bool IsFinished => isFinished;

    public event Action<ISoundHandle> Finished;

    private readonly AudioEmitter emitter;
    private readonly AudioId audioId;

    private bool isValid;
    private bool isFinished;

    /// <summary>订阅 emitter 的完成回调 —— 所有结束路径都汇入它, 因此句柄不会漏掉任何打断.</summary>
    public SoundHandle(AudioEmitter emitter, AudioId audioId)
    {
        this.emitter = emitter;
        this.audioId = audioId;

        isValid = true;
        isFinished = false;

        this.emitter.onAudioFinished += OnPlaybackEnded;
    }

    public bool Stop()
    {
        if (!isValid)
            return false;

        emitter.Stop();
        Invalidate(false);
        return true;
    }

    public bool TrySetVolume(float volume)
    {
        if (!isValid)
            return false;

        emitter.Volume = volume;
        return true;
    }

    public bool TrySetPitch(float pitch)
    {
        if (!isValid)
            return false;

        emitter.Pitch = pitch;
        return true;
    }

    private void Invalidate(bool finishedNaturally)
    {
        if (!isValid)
            return;

        isValid = false;
        isFinished = finishedNaturally;

        emitter.onAudioFinished -= OnPlaybackEnded;

        Finished?.Invoke(this);
    }

    private void OnPlaybackEnded(AudioEmitter source)
        => Invalidate(source.IsFinished);
}
