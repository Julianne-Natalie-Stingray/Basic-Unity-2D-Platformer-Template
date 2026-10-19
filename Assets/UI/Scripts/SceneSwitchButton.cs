using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 把按钮点击接到场景切换; 目标场景由本组件携带, 因此无需为每个按钮写脚本.
/// 归属: UI.
/// 不负责: 切换本身, 由 SceneSwitchManager 执行.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class SceneSwitchButton : MonoBehaviour
{
    /// <summary>要切换到的场景; 未指定时按钮不可用.</summary>
    [SerializeField] private SceneDefinition target;

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

        button.onClick.AddListener(Switch);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Switch);
    }

    private void Switch()
    {
        if (CoreFacade.Instance == null || CoreFacade.Instance.SceneSwitch == null)
            return;

        CoreFacade.Instance.SceneSwitch.RequestSwitch(target);
    }
}
