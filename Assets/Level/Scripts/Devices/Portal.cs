using System.Collections;
using UnityEngine;

/// <summary>
/// 简单传送门: 刚体进入触发区即移动到出口门, 并让出口门进入冷却.
/// 归属: Level.
/// 不负责: 单向/冷却数值的玩法配置(由 Inspector 决定).
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class Portal : MonoBehaviour
{
    [SerializeField, Tooltip("出口门; 为空或非 Active 时不能传送")]
    private Portal exit;

    [SerializeField, Tooltip("传送时是否清零速度")]
    private bool clearVelocity = true;

    [SerializeField, Tooltip("作为出口被使用后是否永久停用(单向)")]
    private bool oneWayPortal;

    [SerializeField, Min(0f), Tooltip("作为出口被使用后的冷却时长")]
    private float cooldownTime = 1f;

    [SerializeField, Tooltip("占位颜色载体; 留空则取子物体上的 SpriteRenderer")]
    private SpriteRenderer indicator;

    [SerializeField] private Color activeColor = Color.blue;
    [SerializeField] private Color cooldownColor = Color.yellow;
    [SerializeField] private Color inactiveColor = Color.red;

    /// <summary>当前状态; Cooldown / Inactive 都不能传送.</summary>
    public PortalState State { get; private set; } = PortalState.Active;

    private Coroutine cooldown;

    private void Awake()
    {
        if (indicator == null)
            indicator = GetComponentInChildren<SpriteRenderer>(true);

        ApplyColor();
    }

    private void OnDisable()
    {
        StopCooldown();
        SetState(PortalState.Inactive);
    }

    /// <summary>外部开关: true 进 Active, false 进 Inactive; 会取消进行中的冷却.</summary>
    public void ActivatePortal(bool active)
    {
        StopCooldown();
        SetState(active ? PortalState.Active : PortalState.Inactive);
    }

    /// <summary>作为出口被使用后调用: 进入冷却, 冷却结束回到 Active.</summary>
    public void PortalCooldown()
    {
        if (!isActiveAndEnabled)
            return;

        StopCooldown();

        if (cooldownTime <= 0f)
        {
            SetState(PortalState.Active);
            return;
        }

        SetState(PortalState.Cooldown);
        cooldown = StartCoroutine(RunCooldown());
    }

    private IEnumerator RunCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);

        cooldown = null;
        SetState(PortalState.Active);
    }

    private void StopCooldown()
    {
        if (cooldown == null)
            return;

        StopCoroutine(cooldown);
        cooldown = null;
    }

    private void SetState(PortalState next)
    {
        State = next;
        ApplyColor();
    }

    private void ApplyColor()
    {
        if (indicator == null)
            return;

        indicator.color = State switch
        {
            PortalState.Active => activeColor,
            PortalState.Cooldown => cooldownColor,
            _ => inactiveColor
        };
    }

    private void Reset()
    {
        Collider2D area = GetComponent<Collider2D>();
        if (area != null)
            area.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有 Active 能作为入口发送; 出口即使 Inactive 仍可接收.
        if (State != PortalState.Active || exit == null || exit == this)
            return;

        Rigidbody2D body = other.attachedRigidbody;
        if (body == null)
            return;

        body.position = exit.transform.position;
        if (clearVelocity)
            body.velocity = Vector2.zero;
        Physics2D.SyncTransforms();

        if (exit.oneWayPortal)
            exit.ActivatePortal(false);
        else
            exit.PortalCooldown();
    }
}
