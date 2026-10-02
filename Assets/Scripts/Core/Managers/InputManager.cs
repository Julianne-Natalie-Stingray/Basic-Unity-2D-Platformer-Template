using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A thin input wrapper. Uses Unity's new InputSystem.
/// Doesn't care how the game reacts to input. Implement those logics elsewhere. 
/// Input包装. 内部使用Unity新的InputSystem. 
/// 不负责游戏对输入的响应, 请在别处实现相关逻辑. 
/// </summary>
[DisallowMultipleComponent]
public sealed class InputManager : MonoBehaviour
{
    public event Action PrimaryPressed;
    public event Action PrimaryReleased;

    public Vector2 PointerPosition => controls == null
            ? Vector2.zero
            : controls.Gameplay.PointerPosition.ReadValue<Vector2>();
    
    public Vector2 MovementInput => controls == null
            ? Vector2.zero
            : controls.Gameplay.MovementInput.ReadValue<Vector2>();

    public bool IsPrimaryPressed =>
        controls?.Gameplay.PrimaryPress.IsPressed() ?? false;

    private PlayerControls controls;


    private void Awake()
    {
        controls = new PlayerControls();
    }


    private void OnEnable()
    {
        controls.Gameplay.PrimaryPress.performed += HandlePrimaryPressed;
        controls.Gameplay.PrimaryPress.canceled += HandlePrimaryReleased;

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.PrimaryPress.performed -= HandlePrimaryPressed;
        controls.Gameplay.PrimaryPress.canceled -= HandlePrimaryReleased;

        controls.Gameplay.Disable();
    }

    private void OnDestroy()
        => controls?.Dispose();

    private void HandlePrimaryPressed(InputAction.CallbackContext context)
        => PrimaryPressed?.Invoke();

    private void HandlePrimaryReleased(InputAction.CallbackContext context)
        => PrimaryReleased?.Invoke();
}