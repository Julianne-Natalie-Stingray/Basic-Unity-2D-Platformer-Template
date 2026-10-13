using UnityEngine;

/// <summary>
/// 运行期间跨场景共享的通关进度, 每次启动清空, 与音量偏好无关.
/// 归属: Game (Progress).
/// </summary>
public static class GameProgress
{
    private static LevelProgressStore store;

    /// <summary>首次读取时惰性创建.</summary>
    public static LevelProgressStore Store => store ??= new LevelProgressStore();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        store = null;
    }
}
