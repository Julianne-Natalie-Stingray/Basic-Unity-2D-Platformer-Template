using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单移动平台: 在起点与偏移点之间往返, 并把自身位移带给站在上面的刚体.
/// 归属: Level.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField, Tooltip("相对起点的位移")]
    private Vector2 offset = new Vector2(0f, 3f);

    [SerializeField, Min(0f), Tooltip("往返速度, 单位/秒")]
    private float speed = 2f;

    private readonly HashSet<Rigidbody2D> riders = new();
    private Rigidbody2D body;
    private Vector2 start;
    private Vector2 previousPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        start = body.position;
        previousPosition = start;
    }

    private void FixedUpdate()
    {
        Vector2 next = Vector2.Lerp(start, start + offset, Mathf.PingPong(Time.time * speed * 0.5f, 1f));
        body.MovePosition(next);

        Vector2 delta = next - previousPosition;
        previousPosition = next;

        if (delta.sqrMagnitude <= 0f)
            return;

        riders.RemoveWhere(rider => rider == null);
        foreach (Rigidbody2D rider in riders)
            rider.position += delta;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrackRider(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TrackRider(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null)
            riders.Remove(collision.rigidbody);
    }

    private void TrackRider(Collision2D collision)
    {
        Rigidbody2D rider = collision.rigidbody;
        if (rider == null || rider.bodyType != RigidbodyType2D.Dynamic)
            return;
        if (rider.position.y < body.position.y)
            return;

        riders.Add(rider);
    }
}
