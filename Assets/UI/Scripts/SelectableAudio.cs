using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 交互音效反馈.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Selectable))]
public sealed class SelectableAudio : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] AudioId downSfx = AudioId.None;
    [SerializeField] AudioId upSfx = AudioId.None;
    [SerializeField] AudioId sliderChangedSfx = AudioId.None;
    
    private Selectable selectable;
    private Slider slider;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        slider = selectable as Slider;
        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanPlay && slider && downSfx != AudioId.None)
            Play(downSfx);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (CanPlay && slider && upSfx != AudioId.None)
            Play(upSfx);
    }

    private void OnSliderChanged(float value)
    {
        if (CanPlay && sliderChangedSfx != AudioId.None)
            Play(sliderChangedSfx);
    }

    private bool CanPlay => isActiveAndEnabled && selectable != null && selectable.IsInteractable();

    private static void Play(AudioId requested)
    {
        CoreFacade core = CoreFacade.Instance;
        if (core != null && core.Audio != null)
            core.Audio.CreateBuilder().
                WithAllowWhileFrozen(true).
                WithSurviveFreeze(true).
                Play(requested);
    }
}
