using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 死亡面板.
/// 归属: UI.
/// 不负责: 死亡后的行为, 仅暴露 Confirmed 事件供订阅者接线.
/// </summary>
[DisallowMultipleComponent]
public sealed class DeathScreen : MonoBehaviour
{
    [SerializeField] private Motor motor;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text message;

    /// <summary>玩家点击死亡面板确认按钮时触发; 具体后续行为由订阅者决定.</summary>
    public event Action Confirmed;

    private Motor subscribedMotor;
    private float nextSearchTime;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            FreezeWhileVisible.Attach(panel);
        }

        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);
    }

    private void OnEnable()
    {
        FindMotor();
    }

    private void OnDisable()
    {
        UnsubscribeMotor();
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (subscribedMotor == null && Time.unscaledTime >= nextSearchTime)
            FindMotor();
    }

    /// <summary>保留已配置的 Motor, 或在自身场景中查找(含未激活).</summary>
    [ContextMenu("Find Motor")]
    public void FindMotor()
    {
        nextSearchTime = Time.unscaledTime + 0.5f;

        if (motor == null && gameObject.scene.IsValid())
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                motor = root.GetComponentInChildren<Motor>(true);
                if (motor != null)
                    break;
            }
        }

        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (subscribedMotor != motor)
        {
            UnsubscribeMotor();
            subscribedMotor = motor;
            if (subscribedMotor != null)
                subscribedMotor.Died += Show;
        }

        if (subscribedMotor != null && subscribedMotor.IsDead)
            Show();
    }

    public void Confirm()
    {
        Confirmed?.Invoke();
    }

    private void Show()
    {
        if (panel == null)
            return;

        if (message != null)
            message.text = "You died.";
        panel.SetActive(true);
    }

    private void UnsubscribeMotor()
    {
        if (subscribedMotor != null)
            subscribedMotor.Died -= Show;
        subscribedMotor = null;
    }
}
