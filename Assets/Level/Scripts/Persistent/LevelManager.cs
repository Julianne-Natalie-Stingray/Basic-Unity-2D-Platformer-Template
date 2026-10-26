using System;
using UnityEngine;

/// <summary>
/// 关卡通关门面: 汇聚根条件并广播通关.
/// 归属: Level.
/// 不负责: 条件之间的关系(由组合器表达)与通关后的表现(由订阅者负责).
/// </summary>
[DisallowMultipleComponent]
public sealed class LevelManager : MonoBehaviour
{
    [SerializeField, Tooltip("根条件; 需实现 ILevelGoal")]
    private MonoBehaviour rootCondition;

    /// <summary>是否已通关.</summary>
    public bool IsCleared { get; private set; }

    /// <summary>是否已失败; 与 IsCleared 互斥, 先到者胜.</summary>
    public bool IsFailed { get; private set; }

    /// <summary>是否已经出结果(通关或失败).</summary>
    public bool IsResolved => IsCleared || IsFailed;

    /// <summary>首次满足根条件时触发一次.</summary>
    public event Action LevelCleared;

    /// <summary>首次判定失败时触发一次; 由关卡自定义失败条件调用 NotifyFailed.</summary>
    public event Action LevelFailed;

    private ILevelGoal levelGoal;

    private void Start()
    {
        if (rootCondition == null)
        {
            GameLog.Error(this)
                .Subsystem("Level")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(rootCondition)))
                .Action(LogAction.DisableComponent)
                .Write();

            enabled = false;
            return;
        }

        if (rootCondition is ILevelGoal goal)
        {
            levelGoal = goal;
            levelGoal.Changed += HandleGoalChanged;
            if (levelGoal.Fulfilled)
                Clear();
        }
        else
            GameLog.Error(this)
                .Subsystem("Level")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify($"{nameof(ILevelGoal)} does not implement {typeof(ILevelGoal)}"))
                .Action(LogAction.DisableComponent)
                .Write();
        
    }

    private void OnDestroy()
    {
        if (levelGoal != null)
            levelGoal.Changed -= HandleGoalChanged;
    }

    private void HandleGoalChanged()
    {
        if (levelGoal != null && levelGoal.Fulfilled)
            Clear();
    }

    private void Clear()
    {
        if (IsResolved)
            return;

        IsCleared = true;
        LevelCleared?.Invoke();
    }

    /// <summary>供关卡自定义失败条件调用(计时、护送、越界等); 已出结果时为空操作.</summary>
    public void NotifyFailed()
    {
        if (IsResolved)
            return;

        IsFailed = true;
        LevelFailed?.Invoke();
    }
}
