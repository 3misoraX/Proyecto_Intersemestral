using UnityEngine;
using DG.Tweening;

public class Planet3Animation : MonoBehaviour
{
    public float moveDistance = 12f;
    public float moveDuration = 4f;

    public float rotationTime = 18f;

    public float scaleMultiplier = 1.05f;

    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;

        transform.DOLocalMoveX(startPos.x + moveDistance, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);

        transform.DORotate(
            new Vector3(0,0,-360),
            rotationTime,
            RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetLink(gameObject);

        transform.DOScale(startScale * scaleMultiplier, 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }
    private void OnDestroy()
    {
        transform.DOKill();
    }
}