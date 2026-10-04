using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Paste on a GO. 
/// </summary>
public class DebugScript : MonoBehaviour
{
    public CoreFacade core;
    
    private void Start()
    {
        Test();
    }

    private void Test()
    {
        Debug.Log($"###MasterVolume: {SettingBootstrap.Settings.Audio.MasterVolume}\n" +
                  $"OstVolume: {SettingBootstrap.Settings.Audio.OstVolume}\n" +
                  $"SfxVolume: {SettingBootstrap.Settings.Audio.SfxVolume}");
        
        _ = new Timer(10f)
            .UseUnscaledTime()
            .At(0f, () =>
            {
                Debug.Log("###Timescale changed to 20x. ");
                Time.timeScale = 20f;
                Debug.Log("###Play before freeze. ");
                core.Audio.CreateBuilder()
                    .Play(AudioId.DefaultOst);
            })            
            .At(2f, () =>
            {
                Debug.Log("###Timescale changed to 100x. ");
                Time.timeScale = 100f; // passed
            })
            // .At(4.5f, () =>
            // {
            //     Debug.Log("###Play right before freeze. Edge case. ");
            //     core.Audio.CreateBuilder()
            //         .Play(AudioId.DefaultSfx); // case 1 passed. Stopped halfway (intended). 
            // })              
            .At(4.7f, () =>
            {
                Debug.Log("###Play right before freeze with survive freeze. Edge case. ");
                var handle = core.Audio.CreateBuilder()
                    .WithSurviveFreeze(true)
                    .Play(AudioId.DefaultSfx);
                StartCoroutine(
                    LogIfNotIsPlayingBeforeRelease(() => handle.IsPlaying)); // case 2 passed. 
            })            
            .At(5f, () =>
            {
                Debug.Log("###Freeze Program.");
                GameStateManager.Freeze();
                GameStateManager.Freeze(); // passed
            })
            .At(7f, () =>
            {
                Debug.Log("###Play when froze. ");
                core.Audio.CreateBuilder()
                    .Play(AudioId.DefaultSfx);
            })            
            .At(7f, () =>
            {
                Debug.Log("###Play when froze with allow while frozen. ");
                core.Audio.CreateBuilder()
                    .WithAllowWhileFrozen(true)
                    .Play(AudioId.DefaultSfx);
            })            
            .At(8f, () =>
            {
                Debug.Log("###Resume Program.");
                GameStateManager.Resume();
            })
            // .OnComplete(() =>
            // {
            //     Debug.Log("###Application quit. ");
            //     Application.Quit();
            // })
            .Start();
    }

    [Button("Test Write in Settings")]
    private void TestWriteInSettings() // passed
    {
        var random = Random.value;
        
        SettingBootstrap.Settings.Audio.MasterVolume = random;
        SettingBootstrap.Settings.Audio.OstVolume = random;
        SettingBootstrap.Settings.Audio.SfxVolume = random;
        
        Debug.Log($"###MasterVolume: {SettingBootstrap.Settings.Audio.MasterVolume}\n" +
                  $"OstVolume: {SettingBootstrap.Settings.Audio.OstVolume}\n" +
                  $"SfxVolume: {SettingBootstrap.Settings.Audio.SfxVolume}");
    }

    [Button("Test Resetting Settings")]
    private void TestResettingSettings() // passed
    {
        SettingBootstrap.ResetToDefault();
        Debug.Log($"###MasterVolume: {SettingBootstrap.Settings.Audio.MasterVolume}\n" +
                  $"OstVolume: {SettingBootstrap.Settings.Audio.OstVolume}\n" +
                  $"SfxVolume: {SettingBootstrap.Settings.Audio.SfxVolume}");
    }

    private IEnumerator LogIfNotIsPlayingBeforeRelease(Func<bool> condition)
    {
        yield return null;
        while (!condition())
        {
            Debug.Log("!!!"); // not seen in console. passed. 
        }
    }
}
