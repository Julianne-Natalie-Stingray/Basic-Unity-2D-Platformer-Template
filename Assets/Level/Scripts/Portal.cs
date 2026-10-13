using UnityEngine;

/// <summary>
/// 简单传送门: 刚体进入触发区即移动到目标点.
/// 归属: Level.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class Portal : MonoBehaviour
{
    [SerializeField, Tooltip("传送目标点")]
    private Transform exit;

    [SerializeField, Tooltip("传送时是否清零速度")]
    private bool clearVelocity = true;

    private void Reset()
    {
        Collider2D area = GetComponent<Collider2D>();
        if (area != null)
            area.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exit == null || other == null)
            return;

        Rigidbody2D body = other.attachedRigidbody;
        if (body == null)
            return;

        body.position = exit.position;
        if (clearVelocity)
            body.velocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }
}
