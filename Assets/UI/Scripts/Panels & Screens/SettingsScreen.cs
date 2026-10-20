using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 音量设置面板: 读取、保存并即时应用音量.
/// 归属: UI.
/// 不负责: 面板的显隐与冻结, 由 UiPanel 负责.
/// </summary>
[DisallowMultipleComponent]
public sealed class SettingsScreen : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider ostVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private TMP_Text statusText;

    private void OnEnable()
    {
        LoadValues();

        if (statusText != null)
            statusText.text = string.Empty;
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
