using UnityEngine;
using DG.Tweening;

public class MenuBackgroundFloat : MonoBehaviour
{
    public Vector2 moveOffset = new Vector2(30f, 15f);
    public float duration = 12f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;

        transform.DOLocalMove(
            startPos + new Vector3(moveOffset.x, moveOffset.y, 0),
            duration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }
}