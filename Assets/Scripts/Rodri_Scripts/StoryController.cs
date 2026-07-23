using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class StoryController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image storyImage;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private Button skipButton;

    [Header("Historia")]
    [SerializeField] private Sprite[] images;

    [TextArea(2, 5)]
    [SerializeField] private string[] texts;

    [Header("Configuración")]
    [SerializeField] private string nextScene = "DemoGame";
    [SerializeField] private float fadeTime = 0.45f;
    [SerializeField] private float finalFadeTime = 0.8f;

    private int currentIndex;
    private bool canAdvance;
    private bool transitioning;

    private void Start()
    {
        if (images.Length != texts.Length)
        {
            Debug.LogWarning("La cantidad de imágenes y textos no coincide.");
        }

        skipButton.onClick.AddListener(SkipStory);

        // El panel negro solo se usa al final
        fadePanel.alpha = 0;

        ShowCurrentSlide(true);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SkipStory();
            return;
        }

        if (!canAdvance || transitioning)
            return;

        bool space = Keyboard.current != null &&
                     Keyboard.current.spaceKey.wasPressedThisFrame;

        bool click = Mouse.current != null &&
                     Mouse.current.leftButton.wasPressedThisFrame;

        if (space || click)
        {
            NextSlide();
        }
    }

    private void ShowCurrentSlide(bool instant = false)
    {
        transitioning = true;
        canAdvance = false;

        storyImage.sprite = images[currentIndex];
        storyText.text = texts[currentIndex];

        continueText.DOKill();

        continueText.text =
            currentIndex == images.Length - 1 ?
            "Presiona ESPACIO para comenzar la aventura" :
            "Presiona ESPACIO para continuar";

        if (instant)
        {
            storyImage.color = new Color(1, 1, 1, 1);
            storyText.alpha = 1;
            continueText.alpha = 1;

            continueText.alpha = 1;
            continueText
                .DOFade(0.55f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo);

            transitioning = false;
            canAdvance = true;
            return;
        }

        storyImage.color = new Color(1, 1, 1, 0);
        storyText.alpha = 0;
        continueText.alpha = 0;

        storyImage
            .DOFade(1, fadeTime);

        storyText
            .DOFade(1, fadeTime);

        storyImage
            .DOFade(1, fadeTime)
            .OnComplete(() =>
            {
                transitioning = false;
                canAdvance = true;

                continueText
                    .DOFade(0.35f, 0.8f)
                    .SetLoops(-1, LoopType.Yoyo);
            });
    }

    private void NextSlide()
    {
        if (transitioning)
            return;

        transitioning = true;
        canAdvance = false;

        continueText.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(storyImage.DOFade(0, fadeTime));
        seq.Join(storyText.DOFade(0, fadeTime));
        seq.Join(continueText.DOFade(0, fadeTime));

        seq.AppendCallback(() =>
        {
            currentIndex++;

            if (currentIndex >= images.Length)
            {
                FinishStory();
                return;
            }

            storyImage.sprite = images[currentIndex];
            storyText.text = texts[currentIndex];

            continueText.text =
                currentIndex == images.Length - 1 ?
                "Presiona ESPACIO para comenzar la aventura" :
                "Presiona ESPACIO para continuar";
        });

        seq.Append(storyImage.DOFade(1, fadeTime));
        seq.Join(storyText.DOFade(1, fadeTime));

        seq.OnComplete(() =>
        {
            transitioning = false;
            canAdvance = true;

            continueText
                .DOFade(0.35f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo);
        });
    }

    public void SkipStory()
    {
        if (transitioning)
            return;

        FinishStory();
    }

    private void FinishStory()
    {
        transitioning = true;
        canAdvance = false;

        DOTween.Kill(storyImage);
        DOTween.Kill(storyText);
        DOTween.Kill(continueText);
        DOTween.Kill(fadePanel);

        fadePanel
            .DOFade(1, finalFadeTime)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(nextScene);
            });
    }
}