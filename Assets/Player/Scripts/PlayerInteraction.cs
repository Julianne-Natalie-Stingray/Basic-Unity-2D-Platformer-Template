using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 接收 Interact 输入并处理半径内最近的 IInteractable.
/// 归属: Player.
/// 不负责: 交互物的具体效果(由 IInteractable 实现).
/// </summary>
[DisallowMultipleComponent]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField, Min(0f), Tooltip("交互搜索半径, 单位: Unity 单位")]
    private float interactionRadius = 2f;

    [SerializeField, Tooltip("可交互目标的碰撞层")]
    private LayerMask interactionLayers = ~0;

    private InputManager input;
    private Motor motor;
    private PlayerDeath death;
    private CharacterAnimationView view;
    private readonly List<Collider2D> hits = new();

    private void Awake()
    {
        motor = GetComponent<Motor>();
        death = GetComponent<PlayerDeath>();
        view = GetComponent<CharacterAnimationView>();
    }

    private void Start()
    {
        CoreFacade core = CoreFacade.Instance;
        if (core == null || core.Input == null)
        {
            GameLog.Error(this)
                .Subsystem("Player")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("PlayerInteraction requires a scene Core prefab with an InputManager. "))
                .Action(LogAction.DisableComponent)
                .Write();

            enabled = false;
            return;
        }

        input = core.Input;
        BindInput();
    }

    private void OnEnable()
    {
        BindInput();
    }

    private void OnDisable()
    {
        UnbindInput();
    }

    /// <summary>处理最近的一个可交互目标; 无目标或不可操作时返回 false.</summary>
    public bool TryPerformOperation()
    {
        if (!CanOperate())
            return false;

        IInteractable target = FindNearestTarget();
        if (target == null)
            return false;

        bool performed = PerformOperation(target);
        if (performed && view != null && view.TryPlayInteract() && motor != null)
            motor.LockHorizontal(true);

        return performed;
    }

    private void BindInput()
    {
        UnbindInput();
        if (input != null)
            input.InteractPressed += HandleInteract;
    }

    private void UnbindInput()
    {
        if (input != null)
            input.InteractPressed -= HandleInteract;
    }

    private void HandleInteract()
    {
        TryPerformOperation();
    }

    private bool CanOperate()
    {
        if (motor == null)
            motor = GetComponent<Motor>();

        return isActiveAndEnabled && Time.timeScale > 0f &&
            (death == null || !death.IsDead) &&
            (motor == null || !motor.IsControlLocked);
    }

    private bool PerformOperation(IInteractable target)
    {
        InteractionDetails details = new(gameObject, (target as Component)?.gameObject);

        bool performed = false;
        System.Action<IInteractable> onInteracted = node => performed = true;
        target.OnInteracted += onInteracted;
        try
        {
            target.Interact(details);
        }
        finally
        {
            target.OnInteracted -= onInteracted;
        }

        return performed;
    }

    private IInteractable FindNearestTarget()
    {
        ContactFilter2D filter = new();
        filter.SetLayerMask(interactionLayers);
        filter.useTriggers = true;

        Vector2 origin = transform.position;
        gameObject.scene.GetPhysicsScene2D().OverlapCircle(origin, Mathf.Max(0f, interactionRadius), filter, hits);

        IInteractable nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.gameObject.scene != gameObject.scene || hit.transform.IsChildOf(transform))
                continue;

            foreach (MonoBehaviour candidate in hit.GetComponentsInParent<MonoBehaviour>())
            {
                if (candidate == null || !candidate.isActiveAndEnabled || candidate.transform.IsChildOf(transform))
                    continue;
                if (candidate is not IInteractable interactable || !interactable.CanInteract)
                    continue;

                float distance = (hit.ClosestPoint(origin) - origin).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = interactable;
                    nearestDistance = distance;
                }
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0f, interactionRadius));
    }
}
