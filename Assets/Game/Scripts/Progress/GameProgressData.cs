using System;
using UnityEngine;

/// <summary>
/// progress section 的载荷: 通关进度与最后所在场景的稳定 id.
/// 归属: Game (Progress).
/// 不负责: 场景解析; 关卡顺序仍待后续实现.
/// </summary>
[Serializable]
public sealed class GameProgressData
{
    public int CompletedLevels
    {
        get => completedLevels;
        set => completedLevels = value;
    }

    public string LastLevelId
    {
        get => lastLevelId;
        set => lastLevelId = value;
    }

    [SerializeField] private int completedLevels;
    [SerializeField] private string lastLevelId;
}
