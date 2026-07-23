using UnityEngine;
using DG.Tweening;

public class Planet2Animation : MonoBehaviour
{
    public float offset = 6f;
    public float duration = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;

        Sequence orbit = DOTween.Sequence();

        orbit.Append(transform.DOLocalMove(
            startPos + new Vector3(0, offset, 0),
            duration));

        orbit.Append(transform.DOLocalMove(
            startPos + new Vector3(offset, 0, 0),
            duration));

        orbit.Append(transform.DOLocalMove(
            startPos + new Vector3(0, -offset, 0),
            duration));

        orbit.Append(transform.DOLocalMove(
            startPos + new Vector3(-offset, 0, 0),
            duration));

        orbit.Append(transform.DOLocalMove(startPos, duration));

        orbit.SetEase(Ease.InOutSine);
        orbit.SetLoops(-1);
    }
}