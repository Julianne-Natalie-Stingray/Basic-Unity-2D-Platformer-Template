using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 交互音效反馈: 悬停、按下、抬起、提交与滑块变化.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Selectable))]
public sealed class SelectableAudio : MonoBehaviour,
    IPointerEnterHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISubmitHandler
{
    [SerializeField] private AudioId hoverSfx = AudioId.None;
    [SerializeField] private AudioId downSfx = AudioId.None;
    [SerializeField] private AudioId upSfx = AudioId.None;
    [SerializeField] private AudioId submitSfx = AudioId.None;
    [SerializeField] private AudioId sliderChangedSfx = AudioId.None;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanPlay && hoverSfx != AudioId.None)
            Play(hoverSfx);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanPlay && downSfx != AudioId.None)
            Play(downSfx);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (CanPlay && upSfx != AudioId.None)
            Play(upSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlay && submitSfx != AudioId.None)
            Play(submitSfx);
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
