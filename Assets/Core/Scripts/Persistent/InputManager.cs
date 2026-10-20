using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 薄输入包装: 只发布设备层, 不承载任何玩法语义.
/// 归属: Core (Input).
/// 不负责: 各输入的玩法含义与响应(归玩法代码).
/// </summary>
[DisallowMultipleComponent]
public sealed class InputManager : MonoBehaviour
{
    public event Action PrimaryPressed;
    public event Action PrimaryReleased;

    /// <summary>Up 按钮(当前绑定空格)的按下/松开; 玩法层把它解释为跳跃.</summary>
    public event Action UpPressed;
    public event Action UpReleased;

    /// <summary>交互按钮(当前绑定 F)的按下/松开; 含义由玩法层解释, 本 wrapper 不判断能否交互.</summary>
    public event Action InteractPressed;
    public event Action InteractReleased;

    /// <summary>屏幕空间的指针位置; 模版不附带消费者.</summary>
    public Vector2 PointerPosition => controls == null
            ? Vector2.zero
            : controls.Gameplay.PointerPosition.ReadValue<Vector2>();

    /// <summary>主二维轴输入; 模版不附带消费者, 且 Y 轴可能为空绑定(asset 属于工程而非 wrapper).</summary>
    public Vector2 MovementInput => controls == null
            ? Vector2.zero
            : controls.Gameplay.MovementInput.ReadValue<Vector2>();

    /// <summary>主按钮当前是否被按住; 模版不附带消费者.</summary>
    public bool IsPrimaryPressed =>
        controls?.Gameplay.PrimaryPress.IsPressed() ?? false;

    // TODO: 模版刻意不为 MovementInput / PointerPosition / IsPrimaryPressed 附带消费者 —— 它们是词汇而非句子, 消费者归属玩法; 曾经的使用者都是早期代码, 如今已不再被实例化.
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.PrimaryPress.performed += HandlePrimaryPressed;
        controls.Gameplay.PrimaryPress.canceled += HandlePrimaryReleased;
        controls.Gameplay.Up.performed += HandleUpPressed;
        controls.Gameplay.Up.canceled += HandleUpReleased;
        controls.Gameplay.Interact.performed += HandleInteractPressed;
        controls.Gameplay.Interact.canceled += HandleInteractReleased;

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.PrimaryPress.performed -= HandlePrimaryPressed;
        controls.Gameplay.PrimaryPress.canceled -= HandlePrimaryReleased;
        controls.Gameplay.Up.performed -= HandleUpPressed;
        controls.Gameplay.Up.canceled -= HandleUpReleased;
        controls.Gameplay.Interact.performed -= HandleInteractPressed;
        controls.Gameplay.Interact.canceled -= HandleInteractReleased;

        controls.Gameplay.Disable();
    }

    private void OnDestroy()
        => controls?.Dispose();

    private void HandlePrimaryPressed(InputAction.CallbackContext context)
        => PrimaryPressed?.Invoke();

    private void HandlePrimaryReleased(InputAction.CallbackContext context)
        => PrimaryReleased?.Invoke();

    private void HandleUpPressed(InputAction.CallbackContext context)
        => UpPressed?.Invoke();

    private void HandleUpReleased(InputAction.CallbackContext context)
        => UpReleased?.Invoke();

    private void HandleInteractPressed(InputAction.CallbackContext context)
        => InteractPressed?.Invoke();

    private void HandleInteractReleased(InputAction.CallbackContext context)
        => InteractReleased?.Invoke();
}
