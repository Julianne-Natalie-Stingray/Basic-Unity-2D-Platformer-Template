using UnityEngine;

/// <summary>
/// 环境的唯一入口: 暴露本场景的关卡通关门面.
/// 归属: Level.
/// 不负责: 玩法规则与死亡表现.
/// </summary>
[RequireComponent(typeof(LevelManager))]
[DisallowMultipleComponent]
public sealed class EnvironmentFacade : MonoBehaviour
{
    private LevelManager level;

    /// <summary>本场景的关卡通关门面.</summary>
    public LevelManager Level
    {
        get
        {
            if (level == null)
                level = GetComponent<LevelManager>();

            return level;
        }
    }
}
