using System;

/// <summary>
/// 场景可交互契约.
/// 归属: Contracts.
/// </summary>
public interface IInteractable
{
    /// <summary>当前是否接受请求; 为假时 Interact 不应改变交互状态.</summary>
    bool CanInteract { get; }

    /// <summary>处理请求后触发; 事件参数只有节点本身.</summary>
    event Action<IInteractable> OnInteracted;

    /// <summary>交互单一入口; 无法服务请求时不改变状态.</summary>
    void Interact(InteractionDetails details);
}
