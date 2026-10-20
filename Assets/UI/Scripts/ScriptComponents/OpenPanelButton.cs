using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 把按钮点击接到打开指定面板.
/// 归属: UI.
/// 不负责: 面板的开关逻辑, 由 UiPanel 执行.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class OpenPanelButton : MonoBehaviour
{
    /// <summary>要打开的面板; 未指定时按钮不可用.</summary>
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
            return;
        }

        button.onClick.AddListener(Open);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Open);
    }

    private void Open()
    {
        target.Open();
    }
}
