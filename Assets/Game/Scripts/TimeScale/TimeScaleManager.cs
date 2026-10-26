using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 时间缩放的唯一写入者: 暂停与速度各自按 owner 计数, 最终值由请求集合派生.
/// 归属: Game (TimeScale).
/// 不负责: 谁应该暂停; owner 由调用方提供, 场景切换时统一重置.
/// </summary>
public static class TimeScaleManager
{
    private const float MinSpeed = 0.1f;
    private const float MaxSpeed = 4f;

    private static readonly HashSet<Object> PauseOwners = new();
    private static readonly Dictionary<Object, float> SpeedOwners = new();

    /// <summary>任一 owner 存在即为真; 读取时清除已销毁的 owner.</summary>
    public static bool IsPaused
    {
        get
        {
            if (Purge())
                Recompute();

            return PauseOwners.Count > 0;
        }
    }

    /// <summary>所有速度请求的乘积, 钳制在 0.1..4; 没有请求时为 1.</summary>
    public static float Speed
    {
        get
        {
            if (Purge())
                Recompute();

            return CurrentSpeed();
        }
    }

    /// <summary>暂停优先于速度; 这是 Time.timeScale 的唯一取值来源.</summary>
    public static float EffectiveScale => IsPaused ? 0f : CurrentSpeed();

    /// <summary>暂停或速度发生实际变化后触发一次; 读取时清理失效 owner 也可能触发.</summary>
    public static event Action Changed;

    /// <summary>重复请求幂等; owner 为 null 时忽略.</summary>
    public static void PushPause(Object owner)
    {
        if (owner == null)
            return;

        Purge();

        if (!PauseOwners.Add(owner))
            return;

        Recompute();
    }

    /// <summary>仅移除自己持有的请求; 未持有者调用为空操作.</summary>
    public static void PopPause(Object owner)
    {
        if (owner == null)
            return;

        if (!PauseOwners.Remove(owner))
            return;

        Recompute();
    }

    /// <summary>多个 owner 的速度相乘; 单个 owner 重复设置以最后一次为准.</summary>
    public static void SetSpeed(Object owner, float multiplier)
    {
        if (owner == null)
            return;

        float clamped = Mathf.Clamp(multiplier, MinSpeed, MaxSpeed);

        if (SpeedOwners.TryGetValue(owner, out float existing) && Mathf.Approximately(existing, clamped))
            return;

        SpeedOwners[owner] = clamped;
        Recompute();
    }

    /// <summary>移除单个速度请求; 未持有者调用为空操作.</summary>
    public static void ClearSpeed(Object owner)
    {
        if (owner == null)
            return;

        if (SpeedOwners.Remove(owner))
            Recompute();
    }

    /// <summary>清空所有请求并恢复 1 倍; 场景切换与退出前调用.</summary>
    public static void ResetAll()
    {
        PauseOwners.Clear();
        SpeedOwners.Clear();
        Time.timeScale = 1f;

        Changed?.Invoke();
    }

    private static void Recompute()
    {
        Time.timeScale = ComputeScale();
        Changed?.Invoke();
    }

    private static float ComputeScale()
        => PauseOwners.Count > 0 ? 0f : CurrentSpeed();

    private static float CurrentSpeed()
    {
        if (SpeedOwners.Count == 0)
            return 1f;

        float product = 1f;

        foreach (float multiplier in SpeedOwners.Values)
            product *= multiplier;

        return Mathf.Clamp(product, MinSpeed, MaxSpeed);
    }

    /// <summary>移除已被销毁的 owner, 返回是否发生了清理.</summary>
    private static bool Purge()
    {
        bool removed = PauseOwners.RemoveWhere(owner => owner == null) > 0;

        if (SpeedOwners.Count == 0)
            return removed;

        List<Object> stale = null;

        foreach (Object owner in SpeedOwners.Keys)
        {
            if (owner == null)
                (stale ??= new List<Object>()).Add(owner);
        }

        if (stale == null)
            return removed;

        foreach (Object owner in stale)
            SpeedOwners.Remove(owner);

        return true;
    }
}
