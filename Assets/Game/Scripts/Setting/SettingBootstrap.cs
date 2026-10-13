using UnityEngine;

/// <summary>
/// Setting 子系统的入口与当前设置的持有者: 首个场景加载前建立存储并加载一次, 供整个会话读取.
/// 归属: Setting.
/// </summary>
public static class SettingBootstrap
{
    /// <summary>首个场景加载前即有效, 任何代码随时可读, 无需检查就绪.</summary>
    public static GameSettings Settings => store.Data;

    private static FileSettingStore store;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        store = new FileSettingStore();
        store.Load();

        Application.quitting += HandleQuitting;
    }

    /// <summary>保存是显式的: 改值不落盘; 原样返回存储的结果.</summary>
    public static bool Save()
        => store.Save();

    /// <summary>仅内存重置, 不含落盘; 持久化需随后调用 Save.</summary>
    public static void ResetToDefault()
        => store.ResetToDefault();

    private static void HandleQuitting()
        => store.Save();

    // TODO: 提供从 Inspector 触发保存的手段 —— 本自举是静态类, 而 NaughtyAttributes 的 Button 只出现在
    // Component 的 Inspector 上, 没有可挂载的面; 现阶段经 Application.quitting 或显式 Save 调用验证.
}
