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

    private bool isPaused;

    void Start()
    {
        pausePanel.anchoredPosition = new Vector2(
            hiddenX,
            pausePanel.anchoredPosition.y
        );

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
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

        // 🔥 RESET VISUAL CLAVE
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