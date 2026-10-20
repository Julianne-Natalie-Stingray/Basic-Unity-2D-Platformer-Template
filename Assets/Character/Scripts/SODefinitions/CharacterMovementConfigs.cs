using UnityEngine;

/// <summary>
/// 角色运动参数, 用几何量表达.
/// 归属: Character.
/// </summary>
[CreateAssetMenu(fileName = "CharacterMovementConfigs", menuName = "Basic2DPlatformer/Character Movement Configs")]
public class CharacterMovementConfigs : ScriptableObject
{
    [SerializeField, Min(0f), Tooltip("水平最大速度, 单位/秒")]
    private float moveSpeed = 8f;

    [SerializeField, Min(0f), Tooltip("跳跃最高点相对起跳点的高度, 世界单位")]
    private float jumpHeight = 3f;

    [SerializeField, Min(0.05f), Tooltip("从起跳到最高点的时间, 秒")]
    private float timeToApex = 0.35f;

    [SerializeField, Min(0f), Tooltip("下落时的重力倍率, 1 表示与上升相同")]
    private float fallGravityMultiplier = 1.6f;

    [SerializeField, Min(0f), Tooltip("离地后仍可起跳的宽限时间, 秒")]
    private float coyoteTime = 0.1f;

    [SerializeField, Min(0f), Tooltip("落地前提前缓存跳跃的时间, 秒")]
    private float jumpBuffer = 0.1f;

    [SerializeField, Range(0f, 1f), Tooltip("提前松开跳跃键时保留的上升速度比例")]
    private float variableJumpCutoff = 0.5f;

    public float MoveSpeed => moveSpeed;
    public float JumpHeight => jumpHeight;
    public float TimeToApex => timeToApex;
    public float FallGravityMultiplier => fallGravityMultiplier;
    public float CoyoteTime => coyoteTime;
    public float JumpBuffer => jumpBuffer;
    public float VariableJumpCutoff => variableJumpCutoff;

    /// <summary>起跳初速度, 由 jumpHeight 与 timeToApex 推出.</summary>
    public float JumpVelocity => 2f * jumpHeight / Mathf.Max(0.05f, timeToApex);

    /// <summary>上升段重力加速度, 由 jumpHeight 与 timeToApex 推出.</summary>
    public float RiseGravity => 2f * jumpHeight / (Mathf.Max(0.05f, timeToApex) * Mathf.Max(0.05f, timeToApex));
}
