using UnityEngine;

/// <summary>
/// 键盘热键路由: 空格开关暂停面板, Esc 关闭最上层面板(没有时打开暂停).
/// 归属: UI.
/// 不负责: 暂停机制(归 TimeScaleManager)与面板自身显隐(归 UiPanel).
/// </summary>
[DisallowMultipleComponent]
public sealed class UiHotkeys : MonoBehaviour
{
    [SerializeField]
    [Tooltip("空格与 Esc 在没有任何面板打开时操作的暂停面板.")]
    private UiPanel pausePanel;

    private InputManager input;
    private bool isBound;

    private void Start()
        => TryBind();

    // Core 可能由引导脚本晚于本组件实例化, 因此绑定失败时逐帧重试而不是直接禁用.
    private void Update()
    {
        if (!isBound)
            TryBind();
    }

    private void OnDestroy()
    {
        if (!isBound)
            return;

        input.PausePressed -= HandlePausePressed;
        input.CancelPressed -= HandleCancelPressed;
    }

    private void TryBind()
    {
        CoreFacade core = CoreFacade.Instance;
        input = core != null ? core.Input : FindObjectOfType<InputManager>();

        if (input == null)
            return;

        input.PausePressed += HandlePausePressed;
        input.CancelPressed += HandleCancelPressed;
        isBound = true;
    }

    private void HandlePausePressed()
    {
        if (pausePanel == null)
            return;

        UiPanel top = UiPanel.TopOpen;

        if (top == null)
        {
            pausePanel.Open();
            return;
        }

        if (top == pausePanel)
            pausePanel.Close();
    }

    private void HandleCancelPressed()
    {
        UiPanel top = UiPanel.TopOpen;

        if (top != null)
        {
            top.Close();
            return;
        }

        pausePanel?.Open();
    }
}
