/// <summary>
/// 传送门状态: 只有 Active 能传送.
/// 归属: Level.
/// </summary>
public enum PortalState
{
    /// <summary>可被进入, 也可作为出口.</summary>
    Active,

    /// <summary>刚作为出口被使用, 暂时不可传送.</summary>
    Cooldown,

    /// <summary>被单向锁或外部开关停用.</summary>
    Inactive
}
