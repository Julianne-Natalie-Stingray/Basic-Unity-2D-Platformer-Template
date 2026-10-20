using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Core 侧的存档宿主: 持有 SaveSystem, 扫描当前场景贡献者, 并在场景加载后延迟恢复.
/// 归属: Core (Save).
/// 不负责: 具体 section 的序列化; 何时手动存档由 UI 决定, 自动存档触发点由本类接线.
/// </summary>
[DisallowMultipleComponent]
public sealed class SaveService : MonoBehaviour
{
    private SaveSystem system;
    private GameSaveStore store;
    private bool saveRequested;
    private bool suppressQuitSave;

    public bool HasSave => system != null && system.HasSave;

    public string FilePath => store == null ? string.Empty : store.FilePath;

    private void Awake()
    {
        store = new GameSaveStore();

        system = new SaveSystem(
            store,
            new[] { GameProgressContributor.Instance });

        GameProgress.LevelCompleted += HandleLevelCompleted;
        Application.quitting += HandleQuitting;

        if (!system.TryLoad())
        {
            if (store.Exists)
            {
                GameLog.Warning(this)
                    .Subsystem("Save")
                    .Name(LogName.Class)
                    .Issue(LogIssue.Specify($"Could not load {store.FilePath}; progress starts empty. "))
                    .Action(LogAction.UseFallbackValue("empty progress"))
                    .Write();
            }

            return;
        }

        // progress 是全局 section, 不依赖场景对象, 可以立即恢复.
        system.TryRestore(GameProgressContributor.Instance);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        GameProgress.LevelCompleted -= HandleLevelCompleted;
        Application.quitting -= HandleQuitting;
    }

    /// <summary>延迟到下一帧再存档, 避免在 LevelCleared 的同步回调里写盘.</summary>
    private void RequestSave()
    {
        if (system == null || saveRequested)
            return;

        saveRequested = true;
        StartCoroutine(SaveAfterFrame());
    }

    /// <summary>扫描当前场景 + 全局贡献者, 两阶段拍快照后原子写入.</summary>
    public SaveResult Save()
    {
        if (system == null)
            return SaveResult.Failure("SaveService is not initialized.");

        SaveResult result = system.Save(FindSceneContributors);

        if (!result.Succeeded)
        {
            GameLog.Error(this)
                .Subsystem("Save")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify(result.Error))
                .Action(LogAction.Ignore)
                .Write();
        }

        return result;
    }

    /// <summary>重新读取文件; 成功时先恢复全局 section, 场景 section 留待场景加载后.</summary>
    public bool Load()
    {
        if (system == null)
            return false;

        if (!system.TryLoad())
        {
            if (store.Exists)
            {
                GameLog.Warning(this)
                    .Subsystem("Save")
                    .Name(LogName.Class)
                    .Issue(LogIssue.Specify($"Could not load {store.FilePath}; progress was not changed. "))
                    .Action(LogAction.Ignore)
                    .Write();
            }

            return false;
        }

        system.TryRestore(GameProgressContributor.Instance);
        return true;
    }

    /// <summary>删除存档并清空内存进度; 调用方随后切换场景.</summary>
    public void NewGame()
    {
        StopAllCoroutines();
        saveRequested = false;

        if (system != null)
            system.NewGame();

        GameProgress.Reset();
        suppressQuitSave = true;

        string path = store == null ? "save file" : store.FilePath;

        GameLog.Info(this)
            .Subsystem("Save")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"New Game: deleted {path}. "))
            .Action(LogAction.Ignore)
            .Write();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        suppressQuitSave = false;

        if (system == null || system.PendingCount == 0)
            return;

        StartCoroutine(RestoreSceneAfterFrame());
    }

    private void HandleLevelCompleted()
    {
        RequestSave();
    }

    /// <summary>退出时只在已有存档且未刚执行 New Game 的情况下覆盖; 失败保留旧文件.</summary>
    private void HandleQuitting()
    {
        if (suppressQuitSave || !HasSave)
            return;

        Save();
    }

    private IEnumerator SaveAfterFrame()
    {
        yield return null;

        saveRequested = false;

        if (system != null)
            Save();
    }

    /// <summary>等一帧, 确保场景对象的 Start 已完成, 再恢复 section.</summary>
    private IEnumerator RestoreSceneAfterFrame()
    {
        yield return null;

        if (system == null)
            yield break;

        system.RestoreAll(FindSceneContributors());

        if (system.PendingCount > 0)
        {
            GameLog.Warning(this)
                .Subsystem("Save")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify($"{system.PendingCount} section(s) remain pending after scene load. "))
                .Action(LogAction.Ignore)
                .Write();
        }
    }

    /// <summary>只扫描当前活动场景; DontDestroyOnLoad 与其它 additive 场景不参与.</summary>
    private static IEnumerable<ISaveContributor> FindSceneContributors()
    {
        Scene active = SceneManager.GetActiveScene();
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is ISaveContributor contributor &&
                behaviour.gameObject.scene == active)
            {
                yield return contributor;
            }
        }
    }

}
