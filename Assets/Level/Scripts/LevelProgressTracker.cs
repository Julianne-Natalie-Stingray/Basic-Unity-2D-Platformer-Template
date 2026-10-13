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

    [SerializeField, Tooltip("留空则在本场景内自动查找")]
    private LevelManager levelManager;

    private void OnEnable()
    {
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
        if (levelManager == null)
            return;

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
        GameProgress.Store.CompleteLevel(levelNumber);
    }
}
