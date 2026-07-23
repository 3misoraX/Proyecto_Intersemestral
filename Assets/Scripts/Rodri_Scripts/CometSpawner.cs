using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CometSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform cometPrefab;
    [SerializeField] private RectTransform effectsParent;

    [Header("Spawn")]
    [SerializeField] private float minSpawnTime = 10f;
    [SerializeField] private float maxSpawnTime = 20f;

    [Header("Movement")]
    [SerializeField] private float minTravelTime = 2f;
    [SerializeField] private float maxTravelTime = 3.2f;

    [SerializeField] private float outsideOffset = 350f;

    private RectTransform canvasRect;


    private void Start()
    {
        canvasRect = effectsParent.root.GetComponent<RectTransform>();

        StartCoroutine(SpawnRoutine());
    }


    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(minSpawnTime, maxSpawnTime)
            );

            SpawnComet();
        }
    }


    void SpawnComet()
    {
        RectTransform comet = Instantiate(
            cometPrefab,
            effectsParent
        );

        comet.gameObject.SetActive(true);
        comet.SetAsLastSibling();


        CanvasGroup cg = comet.GetComponent<CanvasGroup>();

        if (cg == null)
            cg = comet.gameObject.AddComponent<CanvasGroup>();


        float width = canvasRect.rect.width;
        float height = canvasRect.rect.height;


        bool leftToRight = Random.value > 0.5f;


        Vector2 start;
        Vector2 end;


        if (leftToRight)
        {
            start = new Vector2(
                -width / 2 - outsideOffset,
                Random.Range(height * 0.25f, height / 2)
            );

            end = new Vector2(
                width / 2 + outsideOffset,
                Random.Range(-height / 2, -height * 0.25f)
            );
        }
        else
        {
            start = new Vector2(
                width / 2 + outsideOffset,
                Random.Range(height * 0.25f, height / 2)
            );

            end = new Vector2(
                -width / 2 - outsideOffset,
                Random.Range(-height / 2, -height * 0.25f)
            );
        }


        comet.anchoredPosition = start;


        // ============================
        // ROTACIÓN CORRECTA DEL COMETA
        // ============================

        Vector2 direction = end - start;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;


        // Tu estela está hacia la derecha
        // por eso invertimos el sprite
        comet.localRotation =
            Quaternion.Euler(0, 0, angle + 140f);



        // ============================
        // CURVA SUAVE
        // ============================

        Vector2 middle = (start + end) / 2f;


        middle += new Vector2(
            Random.Range(-100f, 100f),
            Random.Range(-50f, 50f)
        );


        Vector3[] path =
        {
            start,
            middle,
            end
        };



        float travelTime = Random.Range(
            minTravelTime,
            maxTravelTime
        );


        cg.alpha = 0f;

        comet.localScale = Vector3.one * 0.8f;



        Sequence seq = DOTween.Sequence()
            .SetLink(comet.gameObject);
            Destroy(comet.gameObject);



        // Aparece
        seq.Append(
            cg.DOFade(1f, 0.2f)
        );


        // Crece ligeramente
        seq.Join(
            comet.DOScale(
                1f,
                0.3f
            )
        );


        // Movimiento
        seq.Append(
            comet.DOPath(
                path,
                travelTime,
                PathType.CatmullRom
            )
            .SetEase(Ease.Linear)
        );


        // Desaparece al final
        seq.Join(
            cg.DOFade(
                0f,
                0.35f
            )
            .SetDelay(travelTime - 0.35f)
        );


        seq.OnComplete(() =>
        {
            Destroy(comet.gameObject);
        });
    }
}