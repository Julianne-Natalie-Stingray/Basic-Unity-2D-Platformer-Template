using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(AudioSource))]
public class AudioEmitter : MonoBehaviour
{
    public event Action<AudioEmitter> onAudioFinished;
    
    private AudioSource source;
    
    private AudioClipData data;
    
    private bool IsPlaying => source.isPlaying;
    
    private void Awake()
    {
        InitializeInternal();
    }

    private void InitializeInternal()
    {
        if (!source)
            source = GetComponent<AudioSource>();
    }

    public void Configure(AudioClipData data)
    {
        this.data = data;
        source.clip = data.Clip;
        source.loop = data.Loop;
        source.outputAudioMixerGroup = data.MixerGroup;
    }

    public void Play()
    {
        if (IsPlaying) 
            return;
        
        source.Play();
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Audio {data.AudioId} played. "))
            .Write();
        
        if (!data.Loop)
        {
            _ = new Timer(Mathf.Infinity)
                .OnComplete(Stop)
                .CompleteWhen(() => !IsPlaying)
                .Start();
        }
    }

    public void Stop()
    {
        source.Stop();
        onAudioFinished?.Invoke(this);
    }
}
