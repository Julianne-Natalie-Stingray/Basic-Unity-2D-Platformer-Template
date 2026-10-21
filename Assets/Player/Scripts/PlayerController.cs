using UnityEngine;

/// <summary>
/// 玩家控制器: 把 Core 输入转成 Motor 意图.
/// 归属: Player.
/// </summary>
[RequireComponent(typeof(CharacterAnimationView))]
[RequireComponent(typeof(PlayerDeath))]
[RequireComponent(typeof(PlayerAnimationAudio))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(Motor))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    private Motor motor;

    private InputManager input;

    private void Awake()
    {
        InitializeInternal();
    }

    private void Start()
    {
        CoreFacade core = CoreFacade.Instance;
        if (core == null || core.Input == null)
        {
            GameLog.Error(this)
                .Subsystem("Player")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("PlayerController requires a scene Core prefab with an InputManager. "))
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
        if (motor != null)
        {
            motor.SetMoveX(0f);
            motor.SetJumpHeld(false);
        }
    }

    private void Update()
    {
        if (motor == null)
            return;

        float horizontal = input != null && input.isActiveAndEnabled
            ? Mathf.Clamp(input.MovementInput.x, -1f, 1f)
            : 0f;
        motor.SetMoveX(horizontal);
    }

    private void BindInput()
    {
        if (input == null)
            return;

        input.UpPressed -= HandleJumpPressed;
        input.UpPressed += HandleJumpPressed;
        input.UpReleased -= HandleJumpReleased;
        input.UpReleased += HandleJumpReleased;
    }

    private void UnbindInput()
    {
        if (input == null)
            return;

        input.UpPressed -= HandleJumpPressed;
        input.UpReleased -= HandleJumpReleased;
    }

    private void HandleJumpPressed()
    {
        if (motor == null)
            return;

        motor.SetJumpHeld(true);
        motor.Jump();
    }

    private void HandleJumpReleased()
    {
        if (motor != null)
            motor.SetJumpHeld(false);
    }
    
    private void InitializeInternal()
    {
        if (motor == null)
            motor = GetComponent<Motor>();
    }
}
