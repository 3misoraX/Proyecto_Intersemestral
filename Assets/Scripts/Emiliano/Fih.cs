using UnityEngine;

public class Fih : MonoBehaviour
{
    public Transform spriteGraphic; // El GameObject hijo que tiene el SpriteRenderer y el Animator
    public SpriteRenderer spriteRenderer;
    private Transform mainCamera;

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró una Main Camera para el billboarding.");
        }
    }

    void LateUpdate()
    {
        // Billboarding: Hacer que el sprite siempre mire a la cámara
        if (spriteGraphic != null && mainCamera != null)
        {
            // Esto hace que el plano del sprite sea paralelo a la pantalla
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.SendMessage("Heal", 1, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
