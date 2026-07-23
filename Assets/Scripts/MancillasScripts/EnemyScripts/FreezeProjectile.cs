using UnityEngine;

public class FreezeProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
    public float freezeDuration = 2f;
    public string targetTag = "Player"; // Cambia a "Enemy" en el prefab del jugador

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            other.SendMessage("LoseHealth", damage, SendMessageOptions.DontRequireReceiver); 
            other.SendMessage("ApplyFreeze", freezeDuration, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}