using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停/设置面板.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
public sealed class SettingsScreen : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider ostVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private SceneDefinition mainMenu;

    private void OnEnable()
    {
        FreezeCoordinator.Request(this);
    }

    private void OnDisable()
    {
        FreezeCoordinator.Release(this);
    }

    public void Open()
    {
        if (gameObject.activeSelf || GameStateManager.Current == GameState.Loading || !HasReferences())
            return;

        LoadValues();
        if (statusText != null)
            statusText.text = string.Empty;
        if (exitButton != null)
            exitButton.interactable = GameStateManager.Current == GameState.Playing;

        gameObject.SetActive(true);
    }

    public void ContinueGame()
    {
        gameObject.SetActive(false);
    }

    public void SaveSettings()
    {
        if (!gameObject.activeInHierarchy || !HasReferences())
            return;

        AudioSettings audio = SettingBootstrap.Settings.Audio;
        audio.MasterVolume = masterVolume.value;
        audio.OstVolume = ostVolume.value;
        audio.SfxVolume = sfxVolume.value;

        if (SettingBootstrap.Save())
        {
            ApplyAudio();
            if (statusText != null)
                statusText.text = "Saved.";
            return;
        }

        if (statusText != null)
            statusText.text = "Save failed.";
    }

    public void ReturnToMainMenu()
    {
        if (mainMenu == null || CoreFacade.Instance == null || CoreFacade.Instance.SceneSwitch == null)
            return;

        gameObject.SetActive(false);
        if (CoreFacade.Instance.SceneSwitch.RequestSwitch(mainMenu) == null)
            gameObject.SetActive(true);
    }

    private void LoadValues()
    {
        AudioSettings audio = SettingBootstrap.Settings.Audio;
        if (masterVolume != null) masterVolume.SetValueWithoutNotify(audio.MasterVolume);
        if (ostVolume != null) ostVolume.SetValueWithoutNotify(audio.OstVolume);
        if (sfxVolume != null) sfxVolume.SetValueWithoutNotify(audio.SfxVolume);
    }

    private void ApplyAudio()
    {
        CoreFacade core = CoreFacade.Instance;
        if (core != null && core.Audio != null)
            core.Audio.ApplyAudioSettings(masterVolume.value, ostVolume.value, sfxVolume.value);
    }

    private bool HasReferences()
    {
        return masterVolume != null && ostVolume != null && sfxVolume != null;
    }
}
