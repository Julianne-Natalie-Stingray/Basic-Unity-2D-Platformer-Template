using UnityEngine;

/// <summary>
/// 面板激活时持有冻结请求, 隐藏或销毁时释放.
/// 归属: Game (GameState).
/// 不负责: 冻结机制本身, 由 FreezeCoordinator 协调.
/// </summary>
[DisallowMultipleComponent]
public sealed class FreezeWhileVisible : MonoBehaviour
{
    /// <summary>为面板补齐本组件(幂等).</summary>
    public static void Attach(GameObject panel)
    {
        if (panel != null && panel.GetComponent<FreezeWhileVisible>() == null)
            panel.AddComponent<FreezeWhileVisible>();
    }

    private void OnEnable()
    {
        FreezeCoordinator.Request(this);
    }

    private void OnDisable()
    {
        FreezeCoordinator.Release(this);
    }
}
