using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 组合条件: 全部子条件达成才算达成; 空列表视为未达成.
/// 归属: Level.
/// </summary>
[DisallowMultipleComponent]
public sealed class LevelGoalAndOperator : MonoBehaviour, ILevelGoal
{
    [SerializeField, Tooltip("子条件; 需实现 ILevelGoal")]
    private List<MonoBehaviour> goals = new();

    /// <summary>子条件是否全部达成; 空列表返回 false.</summary>
    public bool Fulfilled
    {
        get
        {
            bool any = false;
            foreach (ILevelGoal goal in EnumerateGoals())
            {
                any = true;
                if (!goal.Fulfilled)
                    return false;
            }

            return any;
        }
    }

    /// <summary>整体达成状态变化时触发.</summary>
    public event Action Changed;

    private bool lastFulfilled;

    private void OnEnable()
    {
        lastFulfilled = Fulfilled;
        foreach (ILevelGoal goal in EnumerateGoals())
            goal.Changed += Reevaluate;
    }

    private void OnDisable()
    {
        foreach (ILevelGoal goal in EnumerateGoals())
            goal.Changed -= Reevaluate;
    }

    private void Reevaluate()
    {
        bool now = Fulfilled;
        if (now == lastFulfilled)
            return;

        lastFulfilled = now;
        Changed?.Invoke();
    }

    private IEnumerable<ILevelGoal> EnumerateGoals()
    {
        foreach (MonoBehaviour behaviour in goals)
        {
            if (behaviour is ILevelGoal goal)
                yield return goal;
        }
    }
}
