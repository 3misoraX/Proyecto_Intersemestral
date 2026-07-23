using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Optional")]
    [SerializeField] private float panelSwitchDelay = 0.6f;

    private GameObject currentPanel;

    private void Start()
    {
        ShowMainMenuInstant();
    }

    // =========================
    // CORE
    // =========================

    private void DisableAllPanels()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void PlayExitAnimation(GameObject panel)
    {
        if (panel == null) return;

        PanelTitleAnimator title =
            panel.GetComponentInChildren<PanelTitleAnimator>();

        if (title != null)
            title.PlayExit();
    }
    private void SwitchPanel(GameObject nextPanel)
    {
        StopAllCoroutines();
        StartCoroutine(SwitchPanelRoutine(nextPanel));
    }

    private IEnumerator SwitchPanelRoutine(GameObject nextPanel)
    {
        // 1. Salida del panel actual
        if (currentPanel != null)
        {
            PlayExitAnimation(currentPanel);

            PanelFadeAnimator fadeOut =
                currentPanel.GetComponent<PanelFadeAnimator>();

            if (fadeOut != null)
                fadeOut.FadeOut();
        }

        // 2. Esperar animaciones
        yield return new WaitForSeconds(panelSwitchDelay);

        // 3. Apagar todos
        DisableAllPanels();

        // 4. Activar siguiente panel
        nextPanel.SetActive(true);
        currentPanel = nextPanel;

        // 5. Fade + entrada
        PanelFadeAnimator fadeIn =
            nextPanel.GetComponent<PanelFadeAnimator>();

        if (fadeIn != null)
            fadeIn.FadeIn();
    }

    // =========================
    // BUTTON ACTIONS
    // =========================

    public void PlayGame()
    {
        SceneManager.LoadScene("IntroStory");
    }

    public void ShowMainMenu()
    {
        SwitchPanel(mainMenuPanel);
    }

    public void ShowControls()
    {
        SwitchPanel(controlsPanel);
    }

    public void ShowCredits()
    {
        SwitchPanel(creditsPanel);
    }

    public void ShowSettings()
    {
        SwitchPanel(settingsPanel);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =========================
    // STARTUP
    // =========================

    private void ShowMainMenuInstant()
    {
        DisableAllPanels();
        mainMenuPanel.SetActive(true);
        currentPanel = mainMenuPanel;
    }
}