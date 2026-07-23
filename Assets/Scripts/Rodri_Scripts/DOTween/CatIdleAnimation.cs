using UnityEngine;
using DG.Tweening;

public class CatIdleAnimation : MonoBehaviour
{
    [Header("Float")]
    public float floatDistance = 12f;
    public float floatDuration = 2.5f;

    [Header("Breathing")]
    public float scaleMultiplier = 1.03f;
    public float scaleDuration = 1.6f;

    [Header("Rotation")]
    public float rotationAngle = 2f;
    public float rotationDuration = 2.2f;

    private Vector3 startPos;
    private Vector3 startScale;

    private void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;

        transform.DOLocalMoveY(startPos.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);

        transform.DOScale(startScale * scaleMultiplier, scaleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);

        transform.DOLocalRotate(new Vector3(0, 0, rotationAngle), rotationDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }
    private void OnDestroy()
    {
        transform.DOKill();
    }
}