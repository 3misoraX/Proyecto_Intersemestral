using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UniversalProjectile : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Cantidad de daño que hará este proyectil.")]
    public int damage = 1;
    
    [Tooltip("Lista de etiquetas (Tags) que este proyectil puede dañar.")]
    public List<string> targetTags = new List<string> { "Player" }; 
    
    [Tooltip("Etiqueta para los muros o entorno donde el proyectil debe destruirse.")]
    public string wallTag = "Wall";

    [Header("Configuración de Aturdimiento")]
    [Tooltip("¿Este proyectil paraliza al objetivo?")]
    public bool isStunProjectile = false;
    [Tooltip("Tiempo en segundos que el objetivo quedará paralizado.")]
    public float stunDuration = 2f;

    [Header("Ciclo de Vida")]
    [Tooltip("Tiempo en segundos antes de que la bala se destruya sola (para evitar fugas de memoria).")]
    public float lifetime = 5f;

    [Header("Efectos Visuales (Opcional)")]
    [Tooltip("Prefab que aparece cuando la bala choca (ej. una pequeña explosión o chispa).")]
    public GameObject impactEffectPrefab;
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

        // Destruir el proyectil automáticamente si no choca con nada después de 'lifetime' segundos
        Destroy(gameObject, lifetime);
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
        // 1. Comprobar si chocó contra un objetivo válido (Jugador o Enemigo)
        if (targetTags.Contains(other.gameObject.tag))
        {
            ApplyEffects(other.gameObject);
            DestroyProjectile();
        }
        // 2. Comprobar si chocó contra una pared
        else if (other.gameObject.CompareTag(wallTag))
        {
            DestroyProjectile();
        }
    }

    private void ApplyEffects(GameObject target)
    {
        // --- APLICAR DAÑO ---
        // Aquí usamos SendMessage para enviar el daño sin importar cómo se llame tu script de vida.
        // Asegurarse de que el script del jugador/enemigo tenga un método llamado "TakeDamage(int amount)"
        if(target.tag == "Player")
        {
            target.SendMessage("LoseHealth", damage, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        // --- APLICAR ATURDIMIENTO ---
        if (isStunProjectile)
        {
            // De igual forma, el objetivo debe tener un método llamado "ApplyStun(float duration)"
            target.SendMessage("ApplyStun", stunDuration, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void DestroyProjectile()
    {
        // Instanciar el efecto de impacto si hay uno asignado
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destruir el objeto de la bala
        Destroy(gameObject);
    }
}