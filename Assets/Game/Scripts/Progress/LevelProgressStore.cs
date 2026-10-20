/// <summary>
/// 本次运行内的顺序通关进度, 不读取或写入存档.
/// 归属: Game (Progress).
/// </summary>
public sealed class LevelProgressStore
{
    /// <summary>模版当前只有一关(SampleLevel); 增加关卡时同步扩展此值.</summary>
    public const int LevelCount = 1;

    public int CompletedLevels { get; private set; }

    /// <summary>最后进入的场景稳定 id; 空表示尚未进入任何关卡.</summary>
    public string LastLevelId { get; private set; } = string.Empty;

    /// <summary>由关卡场景在初始化时写入; 空值忽略, 避免覆盖已有记录.</summary>
    public void SetLastLevel(string levelId)
    {
        if (!string.IsNullOrWhiteSpace(levelId))
            LastLevelId = levelId.Trim();
    }

    /// <summary>从存档恢复; 非法进度被夹到合法范围, 空 id 保留为空.</summary>
    public void Restore(int completedLevels, string lastLevelId)
    {
        CompletedLevels = completedLevels < 0
            ? 0
            : completedLevels > LevelCount ? LevelCount : completedLevels;

        LastLevelId = string.IsNullOrWhiteSpace(lastLevelId) ? string.Empty : lastLevelId.Trim();
    }

    /// <summary>第 1 关始终解锁; 第 N 关需第 N-1 关已通关.</summary>
    public bool IsUnlocked(int levelNumber)
    {
        return levelNumber >= 1 && levelNumber <= LevelCount && levelNumber <= CompletedLevels + 1;
    }

    /// <summary>只接受已解锁关卡; 重玩不会倒退或越级解锁.</summary>
    public bool CompleteLevel(int levelNumber)
    {
        if (!IsUnlocked(levelNumber))
            return false;

        if (levelNumber > CompletedLevels)
            CompletedLevels = levelNumber;

        return true;
    }
}
