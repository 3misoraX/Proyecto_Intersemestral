using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class TitleDOTween : MonoBehaviour
{
    [Header("Intro")]
    public float introTime = 0.8f;

    [Header("Floating")]
    public float floatDistance = 15f;
    public float floatTime = 3f;

    Vector3 originalScale;
    Vector3 startPos;
    CanvasGroup canvasGroup;

    Tween floatingTween;

    void Awake()
    {
        originalScale = transform.localScale;
        startPos = transform.localPosition;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        PlayIntro();
    }

    void OnDisable()
    {
        floatingTween?.Kill();
    }

    void PlayIntro()
    {
        floatingTween?.Kill();
        transform.DOKill();

        canvasGroup.alpha = 0f;
        transform.localScale = originalScale * 0.85f;
        transform.localPosition = startPos;

        Sequence intro = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(gameObject);

        intro.Append(
            canvasGroup.DOFade(1f, introTime)
        );

        intro.Join(
            transform.DOScale(originalScale, introTime)
                     .SetEase(Ease.OutBack)
        );

        intro.OnComplete(StartFloating);
    }

    void StartFloating()
    {
        floatingTween = transform
            .DOLocalMoveY(startPos.y + floatDistance, floatTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(gameObject);
    }
}