using UnityEngine;
using DG.Tweening;

public class MenuPanelSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class MenuPanel
    {
        public string panelName;
        public GameObject panelObject;
        public CanvasGroup canvasGroup;
    }

    public MenuPanel[] panels;
    public float transitionTime = 0.35f;

    MenuPanel currentPanel;

    void Start()
    {
        foreach (var p in panels)
        {
            p.panelObject.SetActive(false);
            p.canvasGroup.alpha = 0;
        }

        // Panel inicial
        currentPanel = panels[0];
        currentPanel.panelObject.SetActive(true);
        currentPanel.canvasGroup.alpha = 1;
    }

    public void SwitchTo(string panelName)
    {
        MenuPanel nextPanel = null;

        foreach (var p in panels)
        {
            if (p.panelName == panelName)
            {
                nextPanel = p;
                break;
            }
        }

        if (nextPanel == null || nextPanel == currentPanel)
            return;

        Sequence seq = DOTween.Sequence();

        // Fade out panel actual
        seq.Append(currentPanel.canvasGroup
            .DOFade(0, transitionTime));

        seq.OnComplete(() =>
        {
            currentPanel.panelObject.SetActive(false);

            nextPanel.panelObject.SetActive(true);
            nextPanel.canvasGroup.alpha = 0;

            // Fade in nuevo panel
            nextPanel.canvasGroup
                .DOFade(1, transitionTime);

            // Animar títulos si existen
            var titles = nextPanel.panelObject
                .GetComponentsInChildren<SectionTitleDOTween>(true);

            foreach (var t in titles)
                t.PlayEnter();

            currentPanel = nextPanel;
        });
    }
}