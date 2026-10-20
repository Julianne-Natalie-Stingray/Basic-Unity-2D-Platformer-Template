using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 把按钮点击接到关闭指定面板.
/// 归属: UI.
/// 不负责: 面板的开关逻辑, 由 UiPanel 执行.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class ClosePanelButton : MonoBehaviour
{
    /// <summary>要关闭的面板; 未指定时按钮不可用.</summary>
    [SerializeField] private UiPanel target;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (target == null)
        {
            GameLog.Error(this)
                .Subsystem("UI")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(target)))
                .Action(LogAction.DisableComponent)
                .Write();

            button.interactable = false;
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (target == null || button == null)
            return;

        button.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(Close);
    }

    private void Close()
    {
        target.Close();
    }
}
