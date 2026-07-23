using UnityEngine;

public class SnowballProjectile : MonoBehaviour
{
    [Header("Configuración de Bola de Nieve")]
    public float speed = 5f;
    public float maxScaleMultiplier = 3f;
    public float growthRate = 1f;
    public int damage = 2;
    public string targetTag = "Player"; // Cambiar a "Enemy" en el prefab del jugador

    [Header("Rastro de Hielo")]
    public GameObject iceTrailPrefab;
    public float dropInterval = 0.5f;
    
    private Vector3 initialScale;
    private float timer;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 1. Mover hacia adelante
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 2. Crecer
        if (transform.localScale.x < initialScale.x * maxScaleMultiplier)
        {
            transform.localScale += Vector3.one * (growthRate * Time.deltaTime);
        }

        // 3. Dejar rastro de hielo
        timer += Time.deltaTime;
        if (timer >= dropInterval && iceTrailPrefab != null)
        {
            Instantiate(iceTrailPrefab, new Vector3(transform.position.x, 0.05f, transform.position.z), Quaternion.identity);
            timer = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Daño al objetivo (Jugador o Enemigo)
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            other.SendMessage("LoseHealth", damage, SendMessageOptions.DontRequireReceiver); 
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall")) 
        {
            // Se destruye al tocar un muro con el tag "Wall"
            Destroy(gameObject); 
        }
    }
}