using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 空白处点击音效: 挂在全屏点击层上, 按钮已消费的点击不会到达这里.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
public sealed class UiClickAudio : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    [Tooltip("空白处左键点击时播放的音效; 留空则不发声.")]
    private AudioId clickSfx = AudioId.None;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !clickSfx.IsValid)
            return;

        CoreFacade core = CoreFacade.Instance;
        if (core == null || core.Audio == null)
            return;

        core.Audio.CreateBuilder()
            .WithAllowWhileFrozen(true)
            .WithSurviveFreeze(true)
            .Play(clickSfx);
    }
}
