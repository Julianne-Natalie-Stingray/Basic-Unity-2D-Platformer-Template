using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冻结请求协调器: 任一 owner 存在即冻结, 全部释放后解冻, 自动忽略已销毁的 owner.
/// 归属: Game (GameState).
/// 不负责: 冻结机制本身, 那属于 GameStateManager.
/// </summary>
public static class FreezeCoordinator
{
    private static readonly HashSet<Object> owners = new();

    /// <summary>重复请求幂等.</summary>
    public static void Request(Object owner)
    {
        if (owner == null)
            return;

        Purge();
        owners.Add(owner);
        GameStateManager.Freeze();
    }

    /// <summary>仅在没有其他 owner 时解冻.</summary>
    public static void Release(Object owner)
    {
        if (owner == null)
            return;

        owners.Remove(owner);
        Purge();

        if (owners.Count == 0)
            GameStateManager.Resume();
    }

    private static void Purge()
    {
        owners.RemoveWhere(owner => owner == null);
    }
}
