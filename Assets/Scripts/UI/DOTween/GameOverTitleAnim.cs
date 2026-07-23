using DG.Tweening;
using UnityEngine;

public class GameOverTitleAnim : MonoBehaviour
{
    [SerializeField] private RectTransform title;

    private Vector2 targetPos;

    private void Start()
    {
        targetPos = title.anchoredPosition;

        // Empieza un poco arriba
        title.anchoredPosition = targetPos + Vector2.up * 80f;

        Sequence seq = DOTween.Sequence();

        // Baja rápidamente
        seq.Append(
            title.DOAnchorPos(targetPos, 0.45f)
                 .SetEase(Ease.OutQuart)
        );

        // Pequeño impacto
        seq.Append(
            title.DOShakeAnchorPos(
                duration: 0.15f,
                strength: new Vector2(6f, 2f),
                vibrato: 18,
                randomness: 30,
                snapping: false,
                fadeOut: true
            )
        );

        seq.OnComplete(IdleAnimation);
    }

    void IdleAnimation()
    {
        title.DORotate(
            new Vector3(0, 0, 1.5f),
            2f
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }
}