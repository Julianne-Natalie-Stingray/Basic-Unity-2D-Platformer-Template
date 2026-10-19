using UnityEngine;

/// <summary>
/// 交互载荷.
/// 归属: Contracts.
/// 不负责: 校验 Actor 与 Target, 由接收者负责.
/// </summary>
public class InteractionDetails
{
    public GameObject Actor { get; }
    public GameObject Target { get; }

    public InteractionDetails(GameObject actor, GameObject target)
    {
        Actor = actor;
        Target = target;
    }
}
