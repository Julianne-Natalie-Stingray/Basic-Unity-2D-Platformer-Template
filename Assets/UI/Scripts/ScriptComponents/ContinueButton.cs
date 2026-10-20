using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Continue: 读取存档并进入目标场景; 没有存档时按钮不可用.
/// 归属: UI.
/// 不负责: 存档内容; 只负责入口语义与场景切换.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class ContinueButton : MonoBehaviour
{
    [SerializeField, Tooltip("继续游戏进入的场景; 关卡顺序实现前固定为当前关卡")]
    private SceneDefinition target;

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

        button.onClick.AddListener(ContinueGame);
    }

    private void OnEnable()
    {
        RefreshInteractable();
    }

    private void Start()
    {
        RefreshInteractable();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(ContinueGame);
    }

    private void RefreshInteractable()
    {
        if (button == null)
            button = GetComponent<Button>();

        CoreFacade core = CoreFacade.Instance;
        button.interactable = target != null &&
                              core != null &&
                              core.Save != null &&
                              core.Save.HasSave;
    }

    private void ContinueGame()
    {
        CoreFacade core = CoreFacade.Instance;
        if (core == null || core.Save == null || core.SceneSwitch == null)
        {
            GameLog.Warning(this)
                .Subsystem("UI")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("CoreFacade.Save or SceneSwitch is not available. "))
                .Action(LogAction.Ignore)
                .Write();

            return;
        }

        if (!core.Save.Load())
        {
            GameLog.Warning(this)
                .Subsystem("UI")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("Continue was requested but the save could not be loaded. "))
                .Action(LogAction.Ignore)
                .Write();

            RefreshInteractable();
            return;
        }

        core.SceneSwitch.RequestSwitch(target);
    }
}
