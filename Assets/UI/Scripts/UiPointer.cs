using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 命中查询: 区分"被真正的 UI 挡住"与"只命中全屏点击层".
/// 归属: UI.
/// 不负责: 世界输入本身; 调用方在按下时查询一次即可.
/// </summary>
public static class UiPointer
{
    private static readonly List<RaycastResult> Results = new();

    /// <summary>指针位置取 Core 的输入包装; 无 EventSystem 或无 Core 时返回 false.</summary>
    public static bool IsPointerOverBlockingUi()
    {
        EventSystem current = EventSystem.current;
        CoreFacade core = CoreFacade.Instance;

        if (current == null || core == null || core.Input == null)
            return false;

        return IsPointerOverBlockingUi(current, core.Input.PointerPosition);
    }

    /// <summary>供自定义输入源与测试使用; 位置为屏幕坐标.</summary>
    public static bool IsPointerOverBlockingUi(Vector2 screenPosition)
    {
        EventSystem current = EventSystem.current;
        return current != null && IsPointerOverBlockingUi(current, screenPosition);
    }

    private static bool IsPointerOverBlockingUi(EventSystem eventSystem, Vector2 screenPosition)
    {
        var eventData = new PointerEventData(eventSystem) { position = screenPosition };

        Results.Clear();
        eventSystem.RaycastAll(eventData, Results);

        foreach (RaycastResult result in Results)
        {
            if (result.gameObject == null)
                continue;

            if (result.gameObject.GetComponentInParent<PassThroughUi>() != null)
                continue;

            return true;
        }

        return false;
    }
}
