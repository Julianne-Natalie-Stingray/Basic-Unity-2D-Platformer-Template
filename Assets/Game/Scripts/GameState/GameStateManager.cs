/// <summary>
/// 全局状态标签的拥有者: 只维护 Playing / Loading, 不施加时间或音频机制.
/// 归属: GameState.
/// 不负责: 谁应该暂停(归 TimeScaleManager), 暂停时音频如何表现(归 AudioManager).
/// </summary>
public static class GameStateManager
{
    /// <summary>首个场景加载前即正确, 任意时刻可读.</summary>
    public static GameState Current { get; private set; }

    private static GameState stateBeforeLoading = GameState.Playing;

    /// <summary>记录当前状态以便之后还原; 重复进入为空操作.</summary>
    public static void EnterLoading()
    {
        if (Current == GameState.Loading)
            return;

        stateBeforeLoading = Current;

        Apply(GameState.Loading);
    }

    /// <summary>还原 EnterLoading 记录的状态而非假定 Playing; 无切换进行时空操作.</summary>
    public static void ExitLoading()
    {
        if (Current != GameState.Loading)
            return;

        Apply(stateBeforeLoading);
    }

    private static void Apply(GameState state)
    {
        Current = state;

        GameLog.Info()
            .Subsystem("GameState")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Game state is now {state}. "))
            .Write();
    }
}
