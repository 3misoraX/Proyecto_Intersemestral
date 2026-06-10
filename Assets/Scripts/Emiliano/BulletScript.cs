using System.Linq;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public BulletType bulletType;

    void Start()
    {
        Destroy(gameObject, bulletType.duration); 
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Areas"))
        {
            return;
        }
        else
        {
            ApplyEffects(collision.gameObject);
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
