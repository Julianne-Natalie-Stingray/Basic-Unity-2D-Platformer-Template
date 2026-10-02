using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The single entry point of core subsystem.
/// Lives on GameCore GO. 
/// Exposes core functionalities. 
/// Enforces core dependencies.
/// Takes care of DontDestroyOnLoad. 
/// Core子系统的单一入口. 贴在GameCore上. 
/// 暴露Core核心功能. 保证Core依赖. 负责Core的DontDestroyOnLoad().  
/// </summary>
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(TimerRunner))]
[DisallowMultipleComponent]
public class CoreFacade : MonoBehaviour
{
#region APIs
    public InputManager Input => input;
    public AudioManager Audio => audios;
#endregion
    
    private InputManager input;
    private AudioManager audios;

    private void Awake()
    {
        InitializeInternal();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeInternal()
    {
        if (!input)
            input = GetComponent<InputManager>();
        
        if (!audios)
            audios = GetComponent<AudioManager>();
    }
}
