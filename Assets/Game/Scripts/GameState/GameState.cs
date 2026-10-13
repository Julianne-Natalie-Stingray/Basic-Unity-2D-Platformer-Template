/// <summary>
/// 游戏的全局状态标签: 只命名状态, 不含行为.
/// 归属: GameState.
/// 不负责: 进入某个状态时应当发生什么.
/// </summary>
public enum GameState
{
    /// <summary>会话初始即处于此状态.</summary>
    Playing,

    /// <summary>游玩被挂起: 游戏时间停止, 音频暂停.</summary>
    Freezed,

    /// <summary>场景切换进行中, 新场景尚未显示; 不施加任何机制, 仅 SceneSwitchManager 进入与离开.</summary>
    Loading
}
