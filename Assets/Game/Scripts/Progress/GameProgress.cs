using System;
using UnityEngine;

/// <summary>
/// 运行期间跨场景共享的通关进度, 每次启动清空, 与音量偏好无关.
/// 归属: Game (Progress).
/// </summary>
public static class GameProgress
{
    /// <summary>进度首次向前推进时触发; 订阅方负责延迟落盘, 不在此处写文件.</summary>
    public static event Action LevelCompleted;

    private static LevelProgressStore store;

    /// <summary>首次读取时惰性创建.</summary>
    public static LevelProgressStore Store => store ??= new LevelProgressStore();

    /// <summary>New Game 使用: 丢弃本次运行的全部内存进度.</summary>
    public static void Reset()
        => store = null;

    /// <summary>由 LevelProgressTracker 在进度真正前进后调用.</summary>
    public static void NotifyLevelCompleted()
        => LevelCompleted?.Invoke();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        store = null;
        LevelCompleted = null;
    }
}
