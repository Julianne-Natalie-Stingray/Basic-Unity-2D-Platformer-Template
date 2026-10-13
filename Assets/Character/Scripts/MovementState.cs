/// <summary>
/// 角色的物理运动状态.
/// 归属: Character.
/// </summary>
public enum MovementState
{
    /// <summary>落地且无水平意图.</summary>
    Idle,

    /// <summary>落地且有水平意图.</summary>
    Run,

    /// <summary>离地且上升.</summary>
    Jump,

    /// <summary>离地且下落.</summary>
    Fall,

    /// <summary>已死亡, 不再运动.</summary>
    Dead
}
