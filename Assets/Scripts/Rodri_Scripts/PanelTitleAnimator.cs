using UnityEngine;
using DG.Tweening;

public class PanelTitleAnimator : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private float offscreenX = -800f; // fuera a la izquierda
    [SerializeField] private float animDuration = 0.5f;

    private RectTransform rect;
    private Vector2 finalPos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        finalPos = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        PlayEnter();
    }

    public void PlayEnter()
    {
        rect.DOKill();

        rect.anchoredPosition = new Vector2(offscreenX, finalPos.y);

        rect.DOAnchorPos(finalPos, animDuration)
            .SetEase(Ease.OutBack);
    }

    public void PlayExit()
    {
        rect.DOKill();

        rect.DOAnchorPosX(offscreenX, animDuration * 0.8f)
            .SetEase(Ease.InExpo);
    }
}