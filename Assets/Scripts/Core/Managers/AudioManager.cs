using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages Audio. Exposes OST / SFX APIs.
/// Implemented using pooling. 
/// Access externally via CoreFacade.
/// 管理音频. 暴露OST / SFX相关API.
/// 使用对象池实现.
/// 外部访问请从CoreFacade处访问. 
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioEmitter emitterPrefab;
    [SerializeField] private Transform emitterRoot;
    [SerializeField, Expandable] private 
    AudioManagerConfigs configs;
    
    private GameObjectPool<AudioEmitter> emitterPool;

    private void Awake()
    {
        InitializeInternal();
    }

    private void Start()
    {
        emitterPool.Prewarm(configs.PrewarmAmount);
    }
    
    public void RequestAudio(AudioId requested)
    {
        if (!configs.TryGetClip(requested, out var data))
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.CannotFind(nameof(requested), nameof(configs)))
                .Action(LogAction.DisableGameObject)
                .Write();
            gameObject.SetActive(false);
        }
        
        var emitter = emitterPool.Get();
        emitter.Configure(data);
        emitter.onAudioFinished += emitterPool.Release;
        emitter.Play();
    }
    
    private void OnEmitterGet(AudioEmitter emitter)
    {
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue
            .Specify("AudioEmitter is pooled. "))
            .Write();
    }

    private void OnEmitterRelease(AudioEmitter emitter)
    {
        emitter.onAudioFinished -= emitterPool.Release;
        
        GameLog.Info(this)
            .Subsystem("Core")
            .Name(LogName.Class)
            .Issue(LogIssue
            .Specify("AudioEmitter is released. "))
            .Write();
    }
    
    private void InitializeInternal()
    {
        if (!emitterPrefab)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(emitterPrefab)))
                .Action(LogAction.DisableGameObject)
                .Write();
            gameObject.SetActive(false);
        }

        if (!emitterRoot)
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(emitterRoot)))
                .Action(LogAction.UseFallbackValue("self transform"))
                .Write();
            emitterRoot = transform;
        }

        if (!configs)
        {
            GameLog.Error(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.NotAssigned(nameof(configs)))
                .Action(LogAction.DisableComponent)
                .Write();
            enabled = false;
        }
                
        emitterPool ??= new(
            emitterPrefab,
            emitterRoot,
            OnEmitterGet,
            OnEmitterRelease,
            configs.CollectionCheck,
            configs.DefaultCapacity,
            configs.MaxPoolSize);
    }

#region Debug

    [SerializeField] private AudioId debugAudioRequest;
    [SerializeField] private int requestTime = 1;
    
    [Button("Test Audio Request")]
    private void TestAudioRequest()
    {
        if (!Application.isPlaying)
        {
            GameLog.Info(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("Please run in play mode."))
                .Action(LogAction.Return)
                .Write();
            return;
        }

        for (int i = 0; i < requestTime; i++)
            RequestAudio(debugAudioRequest);
    }

#endregion
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (requestTime <= 0)
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Invalid(nameof(requestTime)))
                .Action(LogAction.UseFallbackValue("1"))
                .Write();
            requestTime = 1;
        }
    }
#endif
}
