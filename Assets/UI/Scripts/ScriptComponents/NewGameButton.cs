using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// New Game: 删除存档, 清空内存进度, 再进入目标场景.
/// 归属: UI.
/// 不负责: 场景切换本身; 由 SceneSwitchManager 执行.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class NewGameButton : MonoBehaviour
{
    [SerializeField, Tooltip("新游戏进入的场景; 未指定时按钮不可用")]
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

        button.onClick.AddListener(StartNewGame);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(StartNewGame);
    }

    private void StartNewGame()
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

        core.Save.NewGame();
        core.SceneSwitch.RequestSwitch(target);
    }
}
