using UnityEngine;

/// <summary>
/// 玩家进入即调用其 IKillable.Kill().
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

        IKillable killable = other.GetComponentInParent<IKillable>();
        if (killable != null)
            killable.Kill();
    }
}
