using UnityEngine;

/// <summary>
/// 通关后更新本次运行的解锁进度.
/// 归属: Level.
/// </summary>
[DisallowMultipleComponent]
public sealed class LevelProgressTracker : MonoBehaviour
{
    [SerializeField, Range(1, LevelProgressStore.LevelCount)]
    private int levelNumber = 1;

    [SerializeField, Tooltip("稳定关卡 id; 场景改名后仍应保持不变. 留空会回退到场景名并记录警告.")]
    private string levelId;

    [SerializeField, Tooltip("留空则在本场景内自动查找")]
    private LevelManager levelManager;

    private void Awake()
    {
        RecordLastLevel();

        if (levelManager == null)
            levelManager = GetComponent<LevelManager>();
        if (levelManager == null)
        {
            GameLog.Error(this)
                .Subsystem("Level")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(LevelManager)))
                .Action(LogAction.DisableComponent)
                .Write();
            enabled = false;
        }
    }

    private void OnEnable()
    {
        levelManager.LevelCleared += RecordCompletion;
        if (levelManager.IsCleared)
            RecordCompletion();
    }

    private void OnDisable()
    {
        if (levelManager != null)
            levelManager.LevelCleared -= RecordCompletion;
    }

    private void RecordCompletion()
    {
        int before = GameProgress.Store.CompletedLevels;

        if (GameProgress.Store.CompleteLevel(levelNumber) && GameProgress.Store.CompletedLevels > before)
            GameProgress.NotifyLevelCompleted();
    }

    private void RecordLastLevel()
    {
        if (string.IsNullOrWhiteSpace(levelId))
        {
            levelId = gameObject.scene.name;

            GameLog.Warning(this)
                .Subsystem("Level")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("levelId is empty; falling back to the scene name. Renaming the scene will break old saves. "))
                .Action(LogAction.UseFallbackValue(levelId))
                .Write();
        }

        GameProgress.Store.SetLastLevel(levelId);
    }
}
