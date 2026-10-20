using System;
using UnityEngine;

/// <summary>
/// 玩家死亡后打开死亡面板.
/// 归属: UI.
/// 不负责: 死亡后的行为, 仅暴露 Confirmed 事件供订阅者接线.
/// </summary>
[DisallowMultipleComponent]
public sealed class DeathScreen : MonoBehaviour
{
    [SerializeField] private PlayerDeath player;
    [SerializeField] private UiPanel panel;

    /// <summary>玩家点击死亡面板确认按钮时触发; 具体后续行为由订阅者决定.</summary>
    public event Action Confirmed;

    private void OnEnable()
    {
        if (player == null)
            player = FindObjectOfType<PlayerDeath>();

        if (player != null)
            player.PlayerDied += Show;
    }

    private void OnDisable()
    {
        if (player != null)
            player.PlayerDied -= Show;
    }

    /// <summary>确认按钮的入口; 具体后续行为由 Confirmed 订阅者决定.</summary>
    public void Confirm()
    {
        Confirmed?.Invoke();
    }

    private void Show()
    {
        if (panel != null)
            panel.Open();
    }
}
