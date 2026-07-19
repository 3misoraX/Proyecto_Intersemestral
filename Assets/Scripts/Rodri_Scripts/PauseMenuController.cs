using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform pausePanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float hiddenX = -500f;
    [SerializeField] private float visibleX = 40f;

    [Header("Controls Panel")]
    [SerializeField] private RectTransform controlsPanel;
    [SerializeField] private CanvasGroup controlsCanvasGroup;

    private bool isPaused;
    private bool isInControls;

    void Start()
    {
        pausePanel.anchoredPosition = new Vector2(
            hiddenX,
            pausePanel.anchoredPosition.y
        );

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        controlsPanel.anchoredPosition = new Vector2(
            hiddenX,
            controlsPanel.anchoredPosition.y
        );

        controlsCanvasGroup.alpha = 0f;
        controlsCanvasGroup.interactable = false;
        controlsCanvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isInControls)
                return;

            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // =========================
    // PAUSE / RESUME
    // =========================
    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        GamePauseManager.Pause();

        pausePanel.anchoredPosition = new Vector2(
            hiddenX,
            pausePanel.anchoredPosition.y
        );

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        DOTween.Kill(pausePanel);

        pausePanel
            .DOAnchorPosX(visibleX, slideDuration)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);

        canvasGroup
            .DOFade(1f, slideDuration)
            .SetUpdate(true);
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        DOTween.Kill(pausePanel);

        pausePanel
            .DOAnchorPosX(hiddenX, slideDuration)
            .SetEase(Ease.InExpo)
            .SetUpdate(true);

        canvasGroup
            .DOFade(0, slideDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                GamePauseManager.Resume();
            });
    }

    // =========================
    // CONTROLS PANEL
    // =========================

    public void OpenControlsPanel()
    {
        if (!isPaused) return;

        isInControls = true;

        // Ocultar Pause Panel
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        DOTween.Kill(pausePanel);

        pausePanel
            .DOAnchorPosX(hiddenX, slideDuration)
            .SetEase(Ease.InExpo)
            .SetUpdate(true);

        canvasGroup
            .DOFade(0f, slideDuration)
            .SetUpdate(true);

        controlsCanvasGroup.interactable = true;
        controlsCanvasGroup.blocksRaycasts = true;

        DOTween.Kill(controlsPanel);

        controlsPanel
            .DOAnchorPosX(visibleX, slideDuration)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);

        controlsCanvasGroup
            .DOFade(1f, slideDuration)
            .SetUpdate(true);
    }

    public void CloseControlsPanel()
    {
        isInControls = false;

        // Ocultar Controls Panel
        controlsCanvasGroup.interactable = false;
        controlsCanvasGroup.blocksRaycasts = false;

        DOTween.Kill(controlsPanel);

        controlsPanel
            .DOAnchorPosX(hiddenX, slideDuration)
            .SetEase(Ease.InExpo)
            .SetUpdate(true);

        controlsCanvasGroup
            .DOFade(0f, slideDuration)
            .SetUpdate(true);

        // Volver a mostrar Pause Panel
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        DOTween.Kill(pausePanel);

        pausePanel
            .DOAnchorPosX(visibleX, slideDuration)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);

        canvasGroup
            .DOFade(1f, slideDuration)
            .SetUpdate(true);
    }

    // =========================
    // BUTTON ACTIONS
    // =========================

    public void RestartLevel()
    {
        GamePauseManager.Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        GamePauseManager.Resume();
        SceneManager.LoadScene("MainMenu");
    }
}