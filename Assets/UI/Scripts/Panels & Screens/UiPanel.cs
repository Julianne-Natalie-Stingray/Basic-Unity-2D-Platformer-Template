using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个面板的开关门面: 显隐、焦点还原与可选的冻结.
/// 归属: UI.
/// 不负责: 面板内容, 以及"同一时刻只允许一个面板"之类的全局策略.
/// </summary>
[DisallowMultipleComponent]
public sealed class UiPanel : MonoBehaviour
{
    /// <summary>打开后接收焦点的控件; 留空则不改动当前焦点.</summary>
    [SerializeField] private Selectable defaultSelection;

    /// <summary>打开期间是否冻结游戏; 菜单场景通常关闭.</summary>
    [SerializeField] private bool freezeWhileOpen;

    private GameObject previousSelection;

    /// <summary>加载中或已打开时拒绝; 成功后把焦点交给 defaultSelection.</summary>
    public void Open()
    {
        if (gameObject.activeSelf)
            return;

        if (GameStateManager.Current == GameState.Loading)
            return;

        previousSelection = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        gameObject.SetActive(true);

        if (defaultSelection != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(defaultSelection.gameObject);
    }

    /// <summary>隐藏面板; 若打开前的焦点仍在场, 则还给它.</summary>
    public void Close()
    {
        if (!gameObject.activeSelf)
            return;

        gameObject.SetActive(false);

        if (previousSelection != null && previousSelection.activeInHierarchy &&
            !previousSelection.transform.IsChildOf(transform) &&
            EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousSelection);

        previousSelection = null;
    }

    private void OnEnable()
    {
        if (freezeWhileOpen)
            TimeScaleManager.PushPause(this);
    }

    private void OnDisable()
    {
        if (freezeWhileOpen)
            TimeScaleManager.PopPause(this);
    }
}
