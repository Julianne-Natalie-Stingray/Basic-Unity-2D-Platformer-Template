using UnityEngine;

/// <summary>
/// Core 子系统的单一入口: 缓存各项服务并标记自身为持久.
/// 归属: Core.
/// 不负责: 任何服务自身的行为, 只做缓存与暴露.
/// </summary>
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(TimerRunner))]
[RequireComponent(typeof(SceneSwitchManager))]
[RequireComponent(typeof(SaveService))]
[DisallowMultipleComponent]
public class CoreFacade : MonoBehaviour
{
    /// <summary>唯一能熬过场景切换的访问点; 第一个 CoreFacade 完成 Awake 前为 null.</summary>
    public static CoreFacade Instance { get; private set; }

    public InputManager Input => input;
    public AudioManager Audio => audios;
    public SceneSwitchManager SceneSwitch => sceneSwitch;
    public SaveService Save => save;

    private InputManager input;
    private AudioManager audios;
    private SceneSwitchManager sceneSwitch;
    private SaveService save;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            GameLog.Warning(this)
                .Subsystem("Core")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify("A second Core was found; its GameObject is being destroyed. "))
                .Action(LogAction.Ignore)
                .Write();

            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeInternal();
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeInternal()
    {
        if (!input)
            input = GetComponent<InputManager>();

        if (!audios)
            audios = GetComponent<AudioManager>();

        if (!sceneSwitch)
            sceneSwitch = GetComponent<SceneSwitchManager>();

        if (!save)
            save = GetComponent<SaveService>();
    }
}
