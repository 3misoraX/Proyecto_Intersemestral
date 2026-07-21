using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class StarTwinkle : MonoBehaviour
{
    [Header("Alpha")]
    [SerializeField] private float minAlpha = 0.15f;
    [SerializeField] private float maxAlpha = 1f;

    [Header("Tiempo")]
    [SerializeField] private float minFadeTime = 0.3f;
    [SerializeField] private float maxFadeTime = 1.2f;

    [SerializeField] private float minWaitTime = 0.2f;
    [SerializeField] private float maxWaitTime = 2f;

    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();

        // Que cada estrella empiece en un momento diferente
        Invoke(nameof(StartBlink), Random.Range(0f, 2f));
    }

    void StartBlink()
    {
        Blink();
    }

    void Blink()
    {
        float fadeOut = Random.Range(minFadeTime, maxFadeTime);
        float fadeIn = Random.Range(minFadeTime, maxFadeTime);
        float wait = Random.Range(minWaitTime, maxWaitTime);

        Sequence seq = DOTween.Sequence();

        // Espera un tiempo aleatorio
        seq.AppendInterval(wait);

        // Desaparece
        seq.Append(img.DOFade(minAlpha, fadeOut));

        // Espera apagada
        seq.AppendInterval(Random.Range(0.1f, 0.8f));

        // Vuelve a aparecer
        seq.Append(img.DOFade(maxAlpha, fadeIn));

        seq.OnComplete(Blink);
    }
}