using System;
using UnityEngine;

/// <summary>
/// 全局游戏状态的拥有者: 持有 Current, 响应 Freeze 与 Resume, 并施加时间与监听器暂停.
/// 归属: GameState.
/// 不负责: 哪个组件应当响应, 或除时间与音频监听器之外还有哪些系统受影响.
/// </summary>
public static class GameStateManager
{
    /// <summary>首个场景加载前即正确, 任意时刻可读.</summary>
    public static GameState Current { get; private set; }

    /// <summary>在 Current 改变之后触发, 因此处理函数读到的是本次通知所属的值.</summary>
    public static event Action<GameState> Changed;

    private static float timeScaleBeforeFreeze = 1f;
    private static GameState stateBeforeLoading = GameState.Playing;

    /// <summary>已处于 Freezed 时空操作, 以免覆盖已记录的时间缩放.</summary>
    public static void Freeze()
    {
        if (Current == GameState.Freezed)
        {
            GameLog.Info()
                .Subsystem("GameState")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("Freeze was requested while already Freezed; ignored. "))
                .Action(LogAction.Ignore)
                .Write();

            return;
        }

        timeScaleBeforeFreeze = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        Apply(GameState.Freezed);
    }

    /// <summary>还原进入 Freeze 时的时间缩放而非假定某值; 幂等.</summary>
    public static void Resume()
    {
        if (Current == GameState.Playing)
        {
            GameLog.Info()
                .Subsystem("GameState")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("Resume was requested while already Playing; ignored. "))
                .Action(LogAction.Ignore)
                .Write();

            return;
        }

        Time.timeScale = timeScaleBeforeFreeze;
        AudioListener.pause = false;

        Apply(GameState.Playing);
    }

    /// <summary>记录当前状态以便之后还原; 不施加任何机制, 重复进入为空操作.</summary>
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

        Changed?.Invoke(state);
    }
}
