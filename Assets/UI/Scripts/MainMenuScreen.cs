using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主菜单.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
public sealed class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private SettingsScreen settingsScreen;
    [SerializeField] private TMP_Text startStatus;
    [SerializeField] private SceneDefinition sampleLevel;

    private void Update()
    {
        bool ready = CanStart;
        if (newGameButton != null) newGameButton.interactable = ready;
        if (settingsButton != null) settingsButton.interactable = ready;
        if (exitButton != null) exitButton.interactable = ready;
    }

    private bool CanStart => CoreFacade.Instance != null && CoreFacade.Instance.SceneSwitch != null &&
        !CoreFacade.Instance.SceneSwitch.IsSwitching && GameStateManager.Current == GameState.Playing &&
        settingsScreen != null && !settingsScreen.gameObject.activeSelf;

    public void NewGame()
    {
        if (!isActiveAndEnabled || !CanStart || sampleLevel == null)
        {
            SetStatus("Cannot start.");
            return;
        }

        if (CoreFacade.Instance.SceneSwitch.RequestSwitch(sampleLevel) == null)
            SetStatus("Failed to load the level.");
    }

    public void OpenSettings()
    {
        if (settingsScreen != null)
            settingsScreen.Open();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetStatus(string message)
    {
        if (startStatus != null)
            startStatus.text = message;
    }
}
