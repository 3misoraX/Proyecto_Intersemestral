using UnityEngine;
using DG.Tweening;

public class Planet1Animation : MonoBehaviour
{
    public float floatDistance = 8f;
    public float floatDuration = 3f;

    public float rotationTime = 35f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;

        transform.DOLocalMoveY(startPos.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);

        transform.DORotate(
            new Vector3(0, 0, 360),
            rotationTime,
            RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetLink(gameObject);
    }
    private void OnDestroy()
    {
        transform.DOKill();
    }
}