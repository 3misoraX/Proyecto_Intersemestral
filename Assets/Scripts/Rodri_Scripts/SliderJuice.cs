using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SliderJuice : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform sliderRoot;
    [SerializeField] private RectTransform handle;
    [SerializeField] private RectTransform fill;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.15f;

    [Header("Click")]
    [SerializeField] private float handleClickScale = 1.2f;
    [SerializeField] private float clickDuration = 0.12f;

    private Vector3 baseSliderScale;
    private Vector3 baseHandleScale;
    private Vector3 baseFillScale;

    private void Awake()
    {
        if (!sliderRoot) sliderRoot = GetComponent<RectTransform>();

        baseSliderScale = sliderRoot.localScale;
        baseHandleScale = handle.localScale;
        baseFillScale   = fill.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        sliderRoot.DOKill();
        sliderRoot
            .DOScale(baseSliderScale * hoverScale, hoverDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        sliderRoot.DOKill();
        sliderRoot
            .DOScale(baseSliderScale, hoverDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        handle.DOKill();
        handle
            .DOScale(baseHandleScale * handleClickScale, clickDuration)
            .SetEase(Ease.OutBack);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.DOKill();
        handle
            .DOScale(baseHandleScale, clickDuration)
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Feedback visual al mover el slider (seguro, no acumulable)
    /// </summary>
    public void PunchFill()
    {
        if (!fill) return;

        fill.DOKill();
        fill.localScale = baseFillScale;

        fill.DOPunchScale(
            Vector3.up * 0.15f,
            0.15f,
            1,
            0.5f
        );
    }
}