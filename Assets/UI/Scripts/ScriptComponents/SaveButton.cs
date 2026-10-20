using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Save: 调用 Core 的存档服务, 并把成功/失败反馈到可选的状态文本.
/// 归属: UI.
/// 不负责: 存档内容与文件 I/O; 由 SaveService 执行.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class SaveButton : MonoBehaviour
{
    [SerializeField, Tooltip("可选; 显示 Saved / Save failed")]
    private TMP_Text statusText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SaveGame);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(SaveGame);
    }

    private void SaveGame()
    {
        CoreFacade core = CoreFacade.Instance;
        if (core == null || core.Save == null)
        {
            SetStatus("Save unavailable.");

            GameLog.Warning(this)
                .Subsystem("UI")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("CoreFacade.Save is not available. "))
                .Action(LogAction.Ignore)
                .Write();

            return;
        }

        SaveResult result = core.Save.Save();
        SetStatus(result.Succeeded ? "Saved." : "Save failed.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
