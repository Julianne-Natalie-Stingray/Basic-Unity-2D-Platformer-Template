using System;
using UnityEngine;

/// <summary>
/// 玩家进入触发器即达成.
/// 归属: Level.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class LevelGoal : MonoBehaviour, ILevelGoal
{
    /// <summary>玩家是否已进入终点.</summary>
    public bool Fulfilled { get; private set; }

    /// <summary>达成状态变化时触发.</summary>
    public event Action Changed;

    [SerializeField, Tooltip("达成后是否停用自身(收集物用法)")]
    private bool disableWhenFulfilled;

    private void Reset()
    {
        Collider2D area = GetComponent<Collider2D>();
        if (area != null)
            area.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Fulfilled || other == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        Fulfilled = true;
        Changed?.Invoke();

        if (disableWhenFulfilled)
            gameObject.SetActive(false);
    }
}
