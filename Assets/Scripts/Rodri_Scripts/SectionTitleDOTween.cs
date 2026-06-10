using UnityEngine;
using DG.Tweening;

public class SectionTitleDOTween : MonoBehaviour
{
    public float enterTime = 0.5f;
    public float offsetX = -300f;

    Vector3 originalPos;
    CanvasGroup canvasGroup;

    void Awake()
    {
        originalPos = transform.localPosition;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void PlayEnter()
    {
        transform.DOKill();

        transform.localPosition = originalPos + Vector3.right * offsetX;
        canvasGroup.alpha = 0;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1, enterTime));
        seq.Join(transform.DOLocalMove(originalPos, enterTime)
            .SetEase(Ease.OutExpo));
    }
}