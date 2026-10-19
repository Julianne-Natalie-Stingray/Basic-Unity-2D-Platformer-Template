using System.Collections.Generic;

/// <summary>
/// 按开始顺序记录正在播放的池化 emitter.
/// 归属: Core (Audio).
/// 不负责: 任何策略(是否抢占, 抢占谁, 请求是否放行), 它只被询问事实.
/// </summary>
public sealed class AudioRegistry
{
    internal int Count => emitters.Count;

    private readonly List<AudioEmitter> emitters = new();
    private readonly List<long> sequences = new();

    private long nextSequence;

    /// <summary>先压实延迟移除, 再追加 emitter 与下一个序号 —— 序号即开始顺序的依据.</summary>
    internal void Register(AudioEmitter emitter)
    {
        if (!emitter)
            return;

        Compact();

        emitters.Add(emitter);
        sequences.Add(nextSequence++);
    }

    /// <summary>只把槽位置空而不移除, 使正在迭代本注册表的抢占流程不会遇到被改动的列表.</summary>
    internal void Unregister(AudioEmitter emitter)
    {
        if (!emitter)
            return;

        int index = emitters.IndexOf(emitter);
        if (index < 0)
            return;

        emitters[index] = null;
        sequences[index] = 0;
    }

    internal int CountOf(AudioId audioId)
    {
        int count = 0;

        for (int i = 0; i < emitters.Count; i++)
        {
            if (emitters[i] && emitters[i].AudioId == audioId)
                count++;
        }

        return count;
    }

    /// <summary>返回最小序号的注册项, 可限定在某个 AudioId 上; 线性扫描只在已经触顶时执行, 不在常规播放路径上.</summary>
    internal bool TryGetOldest(AudioId audioId, out AudioEmitter emitter)
    {
        emitter = null;
        long oldest = long.MaxValue;

        for (int i = 0; i < emitters.Count; i++)
        {
            AudioEmitter candidate = emitters[i];

            if (!candidate || candidate.AudioId != audioId)
                continue;

            if (sequences[i] >= oldest)
                continue;

            oldest = sequences[i];
            emitter = candidate;
        }

        return emitter;
    }

    internal bool TryGetOldest(out AudioEmitter emitter)
    {
        emitter = null;
        long oldest = long.MaxValue;

        for (int i = 0; i < emitters.Count; i++)
        {
            AudioEmitter candidate = emitters[i];

            if (!candidate)
                continue;

            if (sequences[i] >= oldest)
                continue;

            oldest = sequences[i];
            emitter = candidate;
        }

        return emitter;
    }

    /// <summary>原地重建两个并列列表并保持存活项的相对顺序, 因此开始顺序不被破坏.</summary>
    internal void Compact()
    {
        int write = 0;

        for (int read = 0; read < emitters.Count; read++)
        {
            if (!emitters[read])
                continue;

            emitters[write] = emitters[read];
            sequences[write] = sequences[read];
            write++;
        }

        if (write >= emitters.Count)
            return;

        emitters.RemoveRange(write, emitters.Count - write);
        sequences.RemoveRange(write, sequences.Count - write);
    }
}
