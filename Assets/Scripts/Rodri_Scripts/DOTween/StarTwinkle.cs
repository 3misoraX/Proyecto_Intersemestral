using System.Collections;
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
    private Coroutine blinkRoutine;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            float fadeOut = Random.Range(minFadeTime, maxFadeTime);
            float fadeIn = Random.Range(minFadeTime, maxFadeTime);

            yield return img
                .DOFade(minAlpha, fadeOut)
                .SetLink(gameObject)
                .WaitForCompletion();

            yield return new WaitForSeconds(Random.Range(0.1f, 0.8f));

            yield return img
                .DOFade(maxAlpha, fadeIn)
                .SetLink(gameObject)
                .WaitForCompletion();
        }
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        img.DOKill();
    }

    private void OnDestroy()
    {
        img.DOKill();
    }
}