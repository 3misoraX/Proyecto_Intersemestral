using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonDOTween : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [Header("Hover Scale")]
    public float hoverScale = 1.1f;
    public float hoverTime = 0.2f;

    [Header("Click")]
    public float clickScale = 0.9f;
    public float clickTime = 0.1f;

    [Header("Cursor Follow")]
    public float followStrength = 15f;
    public float followSmooth = 0.15f;

    RectTransform rectTransform;
    Canvas canvas;

    Vector3 originalScale;
    Vector2 originalAnchoredPos;

    Tween scaleTween;
    Tween moveTween;

    bool isHovering;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        originalScale = rectTransform.localScale;
        originalAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        scaleTween?.Kill();
        scaleTween = rectTransform
            .DOScale(originalScale * hoverScale, hoverTime)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // 🔥 CLAVE
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        scaleTween?.Kill();
        moveTween?.Kill();

        scaleTween = rectTransform
            .DOScale(originalScale, hoverTime)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);

        moveTween = rectTransform
            .DOAnchorPos(originalAnchoredPos, hoverTime)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isHovering) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localMousePos
        );

        Vector2 direction = localMousePos - originalAnchoredPos;
        Vector2 targetPos = originalAnchoredPos + direction * (followStrength / 100f);

        moveTween?.Kill();
        moveTween = rectTransform
            .DOAnchorPos(targetPos, followSmooth)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UISoundManager.Instance?.PlayClick();

        scaleTween?.Kill();
        scaleTween = rectTransform
            .DOScale(originalScale * clickScale, clickTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        scaleTween?.Kill();

        Vector3 targetScale = isHovering
            ? originalScale * hoverScale
            : originalScale;

        scaleTween = rectTransform
            .DOScale(targetScale, clickTime)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }
}