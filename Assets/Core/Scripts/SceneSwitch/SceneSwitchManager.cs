using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Core 的场景切换器, 加载另一个场景的唯一入口.
/// 归属: Core (SceneSwitch).
/// 不负责: 该去哪个场景(由调用方以参数决定), 也不负责进度上报, 转场界面, 音频处置与输入屏蔽.
/// </summary>
[DisallowMultipleComponent]
public class SceneSwitchManager : MonoBehaviour
{
    /// <summary>当前是否有一次切换在途; RequestSwitch 无论如何都会强制执行该约束.</summary>
    public bool IsSwitching => isSwitching;

    private AsyncOperation operation;
    private bool isSwitching;

    private const float ProgressBeforeActivation = 0.9f;

    /// <summary>定义为空, 已有切换在途, 未填场景名或目标未注册进 Build Settings 时返回 null 并拒绝.</summary>
    public AsyncOperation RequestSwitch(SceneDefinition definition)
    {
        if (isSwitching || definition == null)
            return null;

        string sceneName = definition.SceneName;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Invalid(nameof(SceneDefinition.SceneName)))
                .Action(LogAction.Ignore)
                .Write();

            return null;
        }

        if (!IsRegisteredInBuildSettings(sceneName))
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify($"{sceneName} is not registered in Build Settings. "))
                .Action(LogAction.Ignore)
                .Write();

            return null;
        }

        operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        isSwitching = true;

        StartCoroutine(RunSwitch(definition, sceneName));

        return operation;
    }

    private IEnumerator RunSwitch(SceneDefinition definition, string sceneName)
    {
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Switching to {definition.Id} ({sceneName}). "))
            .Write();

        operation.allowSceneActivation = false;

        GameStateManager.EnterLoading();

        while (operation.progress < ProgressBeforeActivation)
            yield return null;

        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;

        GameStateManager.ExitLoading();

        isSwitching = false;
        operation = null;

        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Switched to {definition.Id} ({sceneName}). "))
            .Write();
    }

    private static bool IsRegisteredInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int index = 0; index < count; index++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(index);
            if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(path), sceneName, System.StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
