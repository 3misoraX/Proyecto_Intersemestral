using UnityEngine;

public class BossKey : MonoBehaviour
{
    [HideInInspector] public RoomController bossRoom;
    public Transform spriteGraphic; // El GameObject hijo que tiene el SpriteRenderer y el Animator
    public SpriteRenderer spriteRenderer;
    private Transform mainCamera;


    void Start()
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
        // Al tocar al jugador, desbloquea la sala del jefe y se destruye
        if (other.CompareTag("Player"))
        {
            if (bossRoom != null)
            {
                bossRoom.UnlockBossRoom();
                Debug.Log("¡Llave recogida! Las puertas de la sala del jefe se han abierto.");
            }
            Destroy(gameObject);
        }
    }
}