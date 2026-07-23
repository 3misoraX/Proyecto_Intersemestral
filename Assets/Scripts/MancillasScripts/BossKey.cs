using UnityEngine;

public class BossKey : MonoBehaviour
{
    [HideInInspector] public RoomController bossRoom;
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private AudioClip pickupSound;
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
        if (spriteGraphic != null && mainCamera != null)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SfxPlayer.PlayAtPoint(pickupSound, transform.position);

            if (bossRoom != null)
            {
                bossRoom.UnlockBossRoom();
                Debug.Log("¡Llave recogida! Las puertas de la sala del jefe se han abierto.");
            }
            Destroy(gameObject);
        }
    }
}