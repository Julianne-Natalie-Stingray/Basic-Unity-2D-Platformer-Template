using System;
using UnityEngine;

/// <summary>
/// 玩家死亡状态: 关物理、停运动、播死亡动画并广播死亡.
/// 归属: Player.
/// 不负责: 死亡判定(由环境调用 IKillable.Kill)与死亡后的玩法(由订阅者负责).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Motor))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerDeath : MonoBehaviour, IKillable
{
    /// <summary>死亡时触发一次.</summary>
    public event Action PlayerDied;

    /// <summary>是否已死亡.</summary>
    public bool IsDead { get; private set; }

    [SerializeField, Tooltip("留空则取同物体上的 CharacterAnimationView")]
    private CharacterAnimationView view;

    private Motor motor;
    private Rigidbody2D body;

    private void Awake()
    {
        motor = GetComponent<Motor>();
        body = GetComponent<Rigidbody2D>();

        if (view == null)
            view = GetComponent<CharacterAnimationView>();
    }

    /// <summary>环境判定死亡; 幂等.</summary>
    public void Kill()
    {
        if (IsDead)
            return;

        IsDead = true;

        motor.Deactivate();

        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;

        if (view != null)
            view.PlayDeath();

        PlayerDied?.Invoke();
    }
}
