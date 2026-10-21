using UnityEngine;

/// <summary>
/// 动画视图: 观察 Motor 的 MovementState 与朝向并驱动 Animator; 不参与移动逻辑.
/// 归属: Character.
/// </summary>
[RequireComponent(typeof(Motor))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class CharacterAnimationView : MonoBehaviour
{
    private Motor motor;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [SerializeField, Tooltip("未翻转的原始精灵是否朝右")]
    private bool spriteFacesRight = true;

    [SerializeField, Tooltip("可选的朝向标记, 必须是本物体的子物体")]
    private Transform facingMarker;

    private static readonly int MovingParameter = Animator.StringToHash("tryMoving");
    private static readonly int InteractParameter = Animator.StringToHash("Interact");
    private static readonly int DieParameter = Animator.StringToHash("Die");

    private Vector3 markerPosition;

    private void Awake()
    {
        InitializeInternal();
    }


    private void OnEnable()
    {
        if (motor != null)
            motor.StateChanged += ApplyState;
    }

    private void OnDisable()
    {
        if (motor != null)
            motor.StateChanged -= ApplyState;
        if (CanUseAnimator())
            animator.SetBool(MovingParameter, false);
    }

    private void LateUpdate()
    {
        ApplyFacing();
    }

    /// <summary>请求播放交互动画; 无可用 Animator 时返回 false.</summary>
    public bool TryPlayInteract()
    {
        if (!CanUseAnimator())
            return false;

        animator.ResetTrigger(InteractParameter);
        animator.SetTrigger(InteractParameter);
        return true;
    }

    /// <summary>交互动画结束的动画事件; 通知 Motor 解锁.</summary>
    public void OnInteractionAnimationEnded()
    {
        if (motor != null)
            motor.LockHorizontal(false);
    }

    /// <summary>请求播放死亡动画; 由 PlayerDeath 调用.</summary>
    public void PlayDeath()
    {
        if (!CanUseAnimator())
            return;

        animator.SetTrigger(DieParameter);
    }

    private void ApplyState(MovementState state)
    {
        if (!CanUseAnimator())
            return;

        animator.SetBool(MovingParameter, state == MovementState.Run);
    }

    private void ApplyFacing()
    {
        if (motor == null || spriteRenderer == null)
            return;

        spriteRenderer.flipX = (motor.Facing > 0) != spriteFacesRight;
        if (facingMarker != null)
        {
            facingMarker.localPosition = new Vector3(
                spriteRenderer.flipX ? -markerPosition.x : markerPosition.x,
                markerPosition.y,
                markerPosition.z);
        }
    }

    private bool CanUseAnimator()
    {
        return animator != null && animator.isActiveAndEnabled &&
            animator.runtimeAnimatorController != null && animator.isInitialized;
    }
    
    private void InitializeInternal()
    {
        if (motor == null)
            motor = GetComponent<Motor>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (facingMarker != null && facingMarker.IsChildOf(transform) && facingMarker != transform)
            markerPosition = facingMarker.localPosition;
        else
            facingMarker = null;
    }
}
