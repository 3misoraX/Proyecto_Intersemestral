using UnityEngine;
using DG.Tweening;

public class TitleDOTween : MonoBehaviour
{
    public float introTime = 0.8f;
    public float floatDistance = 15f;
    public float floatTime = 3f;

    Vector3 originalScale;
    Vector3 startPos;
    CanvasGroup canvasGroup;

    void Awake()
    {
        originalScale = transform.localScale;
        startPos = transform.localPosition;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        canvasGroup.alpha = 0;
        transform.localScale = originalScale * 0.85f;

        Sequence intro = DOTween.Sequence();
        intro.Append(canvasGroup.DOFade(1, introTime));
        intro.Join(transform.DOScale(originalScale, introTime).SetEase(Ease.OutBack));
        intro.OnComplete(StartFloating);
    }

    void StartFloating()
    {
        transform.DOLocalMoveY(
            startPos.y + floatDistance,
            floatTime
        ).SetEase(Ease.InOutSine)
         .SetLoops(-1, LoopType.Yoyo);
    }
}