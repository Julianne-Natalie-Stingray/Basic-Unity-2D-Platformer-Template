using UnityEngine;

/// <summary>
/// 场景或关卡的稳定定义.
/// 归属: Core (SceneSwitch).
/// 不负责: 是否已注册进 Build Settings, 也不负责加载过程(归 SceneSwitchManager).
/// </summary>
[CreateAssetMenu(menuName = "Basic2DPlatformer/Scene Definition", fileName = "SceneDefinition")]
public class SceneDefinition : ScriptableObject
{
    /// <summary>稳定标识; 默认等于场景名, 仅供日志与外部查找, 不作为序列化引用键.</summary>
    public string Id => id;

    /// <summary>场景文件名(不含扩展名), 即运行时交给 SceneManager 加载的名字.</summary>
    public string SceneName => sceneName;

    /// <summary>显示名; 为空时回退到场景名.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? sceneName : displayName;

    [SerializeField] private string id;
    [SerializeField] private string sceneName;
    [SerializeField] private string displayName;
}
