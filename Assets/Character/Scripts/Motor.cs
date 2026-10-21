using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// 角色运动机制: 拥有刚体, 只收意图, 由物理事实推出 MovementState.
/// 归属: Character.
/// 不负责: 输入来源、动画、死亡语义、物理控制权如何被组织; 可整体停用以移交物理.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class Motor : MonoBehaviour
{
    /// <summary>MovementState 变化时触发.</summary>
    public event Action<MovementState> StateChanged;

    /// <summary>当前运动状态.</summary>
    public MovementState State { get; private set; }

    /// <summary>当前是否有向上的支撑接触.</summary>
    public bool IsGrounded { get; private set; }

    /// <summary>当前速度.</summary>
    public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;

    /// <summary>朝向: 1 右, -1 左.</summary>
    public int Facing => facing;

    /// <summary>控制是否被外部锁定(交互等); 锁定时停止水平意图.</summary>
    public bool IsControlLocked => isControlLocked;

    [SerializeField, Tooltip("运动参数"), Expandable]
    private CharacterMovementConfigs configs;

    [SerializeField, Tooltip("可作为地面的碰撞层")]
    private LayerMask groundLayers = ~0;

    private readonly List<ContactPoint2D> contacts = new();
    private const float MinimumGroundNormalY = 0.9f;
    private const float GroundedUpwardSpeedThreshold = 0.1f;
    private const float MoveEpsilon = 0.01f;

    private Rigidbody2D body;
    private float moveInput;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private float jumpBufferedAt = float.NegativeInfinity;
    private float lastGroundedAt = float.NegativeInfinity;
    private bool isControlLocked;
    private int facing = 1;

    /// <summary>水平意图, -1..1.</summary>
    public void SetMoveX(float direction)
        => moveInput = Mathf.Clamp(direction, -1f, 1f);

    /// <summary>请求跳跃; 是否生效由跳跃缓冲与 coyote 判定.</summary>
    public void Jump()
        => jumpBufferedAt = Time.time;

    /// <summary>跳跃键是否按住; 用于可变跳跃高度.</summary>
    public void SetJumpHeld(bool held)
        => jumpHeld = held;

    /// <summary>锁定/解锁水平控制; 锁定时只保留竖直物理.</summary>
    public void LockHorizontal(bool locked)
    {
        isControlLocked = locked;
        if (locked)
        {
            moveInput = 0f;
            jumpBufferedAt = float.NegativeInfinity;
        }
    }

    /// <summary>启用运动; 与 Deactivate 成对.</summary>
    public void Activate()
    {
        enabled = true;
    }

    /// <summary>停用运动并清空意图; 运动状态保留, 但不再参与 Unity 生命周期.</summary>
    public void Deactivate()
    {
        moveInput = 0f;
        jumpHeld = false;
        jumpBufferedAt = float.NegativeInfinity;
        enabled = false;
    }

    /// <summary>返回真实的向上支撑面, 与接地判定同规则.</summary>
    private bool TryGetSupportingCollider(out Collider2D surface)
    {
        surface = null;
        if (body == null || body.velocity.y > GroundedUpwardSpeedThreshold)
            return false;

        ReadGroundContacts();
        foreach (ContactPoint2D contact in contacts)
        {
            if (contact.normal.y >= MinimumGroundNormalY)
            {
                surface = contact.collider.attachedRigidbody == body ? contact.otherCollider : contact.collider;
                return true;
            }
        }

        return false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (configs == null)
        {
            GameLog.Error(this)
                .Subsystem("Character")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(configs)))
                .Action(LogAction.DisableComponent)
                .Write();

            enabled = false;
            return;
        }

        ApplyGravityScale(1f);
    }

    private void FixedUpdate()
    {
        IsGrounded = TryGetSupportingCollider(out _);
        if (IsGrounded)
            lastGroundedAt = Time.time;

        if (isControlLocked)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            ApplyGravityScale(body.velocity.y > 0f ? 1f : configs.FallGravityMultiplier);
            UpdateState();
            return;
        }

        ApplyGravityScale(body.velocity.y > 0f ? 1f : configs.FallGravityMultiplier);

        float vertical = body.velocity.y;
        bool canJump = IsGrounded || Time.time - lastGroundedAt <= configs.CoyoteTime;
        if (Time.time - jumpBufferedAt <= configs.JumpBuffer && canJump)
        {
            vertical = configs.JumpVelocity;
            jumpBufferedAt = float.NegativeInfinity;
            jumpCutApplied = false;
        }
        else if (!jumpHeld && !jumpCutApplied && body.velocity.y > 0f)
        {
            vertical = body.velocity.y * configs.VariableJumpCutoff;
            jumpCutApplied = true;
        }

        body.velocity = new Vector2(moveInput * configs.MoveSpeed, vertical);

        if (Mathf.Abs(moveInput) > MoveEpsilon)
            facing = moveInput > 0f ? 1 : -1;

        UpdateState();
    }

    private void UpdateState()
    {
        MovementState next;
        if (!IsGrounded)
            next = body.velocity.y > 0f ? MovementState.Jump : MovementState.Fall;
        else
            next = Mathf.Abs(moveInput) > MoveEpsilon ? MovementState.Run : MovementState.Idle;

        SetState(next);
    }

    private void SetState(MovementState next)
    {
        if (State == next)
            return;

        State = next;
        StateChanged?.Invoke(next);
    }

    private void ApplyGravityScale(float multiplier)
    {
        float worldGravity = Mathf.Abs(Physics2D.gravity.y);
        if (worldGravity <= 0.0001f)
            return;

        body.gravityScale = configs.RiseGravity / worldGravity * multiplier;
    }

    private void ReadGroundContacts()
    {
        ContactFilter2D filter = new();
        filter.SetLayerMask(groundLayers);
        filter.useTriggers = false;
        body.GetContacts(filter, contacts);
    }
}
