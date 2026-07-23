using UnityEngine;

public class Fih : MonoBehaviour
{
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private AudioClip healSound;
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
        if (spriteGraphic != null && mainCamera != null)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SfxPlayer.PlayAtPoint(healSound, transform.position);
            other.SendMessage("Heal", 1, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}