using System;

/// <summary>
/// 通关条件的被动契约: 只暴露是否达成与达成变化.
/// 归属: Level.
/// 不负责: 如何达成, 由各实现自行决定(如触发器进入、计数、组合子项).
/// </summary>
public interface ILevelGoal
{
    /// <summary>当前是否已达成.</summary>
    bool Fulfilled { get; }

    /// <summary>达成状态变化时触发.</summary>
    event Action Changed;
}
