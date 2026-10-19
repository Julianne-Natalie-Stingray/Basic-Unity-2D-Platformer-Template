using UnityEngine;

/// <summary>
/// 通关后打开通关面板.
/// 归属: UI.
/// 不负责: 下一关的切换(由按钮上的 SceneSwitchButton 负责).
/// </summary>
[DisallowMultipleComponent]
public sealed class NextLevelScreen : MonoBehaviour
{
    [SerializeField] private EnvironmentFacade environment;
    [SerializeField] private UiPanel panel;

    private LevelManager level;

    private void OnEnable()
    {
        if (environment == null)
            environment = FindObjectOfType<EnvironmentFacade>();

        level = environment != null ? environment.Level : null;
        if (level == null)
            return;

        level.LevelCleared += Show;
        if (level.IsCleared)
            Show();
    }

    private void OnDisable()
    {
        if (level != null)
            level.LevelCleared -= Show;

        level = null;
    }

    private void Show()
    {
        if (panel != null)
            panel.Open();
    }
}
