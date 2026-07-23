using System.Linq;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public BulletType bulletType;
    public GameObject hitEffect;
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
        Destroy(gameObject, bulletType.duration);

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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 0.2f);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Areas")|| collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        else
        {
            ApplyEffects(collision.gameObject);
            if(hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 0.2f);
            }
            Destroy(gameObject);
        }
    }
    private void ApplyEffects(GameObject target)
    {
        // --- APLICAR DAÑO ---
        // Aquí usamos SendMessage para enviar el daño sin importar cómo se llame tu script de vida.
        // Asegurarse de que el script del jugador/enemigo tenga un método llamado "TakeDamage(int amount)"
        target.SendMessage("TakeDamage", bulletType.dmg, SendMessageOptions.DontRequireReceiver);

        // --- APLICAR ATURDIMIENTO ---
        if (bulletType.properties.Contains("Stun"))
        {
            // De igual forma, el objetivo debe tener un método llamado "ApplyStun(float duration)"
            target.SendMessage("ApplyStun", bulletType.abilityDuration, SendMessageOptions.DontRequireReceiver);
        }
    }
}
