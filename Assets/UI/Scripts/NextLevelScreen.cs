using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通关面板.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
public sealed class NextLevelScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Button nextButton;
    [SerializeField] private SceneDefinition nextLevel;

    private LevelManager levelManager;
    private bool switching;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            FreezeWhileVisible.Attach(panel);
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(LoadNextLevel);
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(LoadNextLevel);
    }

    private void OnEnable()
    {
        BindLevelManager();
    }

    private void OnDisable()
    {
        if (levelManager != null)
            levelManager.LevelCleared -= Show;
        levelManager = null;
        if (panel != null)
            panel.SetActive(false);
    }

    /// <summary>进入下一关; 请求被拒时不关闭面板.</summary>
    public void LoadNextLevel()
    {
        if (!isActiveAndEnabled || switching || nextLevel == null)
            return;
        if (CoreFacade.Instance == null || CoreFacade.Instance.SceneSwitch == null)
            return;
        if (CoreFacade.Instance.SceneSwitch.RequestSwitch(nextLevel) == null)
            return;

        switching = true;
    }

    private void BindLevelManager()
    {
        if (levelManager != null)
            levelManager.LevelCleared -= Show;

        levelManager = FindLevelManager();
        if (levelManager != null)
        {
            levelManager.LevelCleared += Show;
            if (levelManager.IsCleared)
                Show();
        }
    }

    private LevelManager FindLevelManager()
    {
        if (!gameObject.scene.IsValid())
            return null;

        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            LevelManager found = root.GetComponentInChildren<LevelManager>(true);
            if (found != null)
                return found;
        }

        return null;
    }

    private void Show()
    {
        if (panel == null || switching)
            return;

        if (message != null)
            message.text = "Level complete!";
        panel.SetActive(true);
    }
}
