using UnityEngine;

/// <summary>
/// 玩家进入即调用其 Motor.Die().
/// 归属: Level.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class Hazard : MonoBehaviour
{
    private void Reset()
    {
        Collider2D area = GetComponent<Collider2D>();
        if (area != null)
            area.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        Motor motor = other.GetComponentInParent<Motor>();
        if (motor != null)
            motor.Die();
    }
}
