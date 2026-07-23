using UnityEngine;

public class IceZone : MonoBehaviour
{
    public string targetTag = "Player"; // Cambiar a "Enemy" en el prefab del jugador
    public float duration = 5f; // Cuánto tiempo dura la mancha en el piso
     
    // Usaremos SendMessage para decirle a ese objeto que cambie su velocidad.

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Llama a una función "ApplySlow" en el objeto que pisa el hielo
            other.SendMessage("ApplySlow", 0.5f, SendMessageOptions.DontRequireReceiver); 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Restaura la velocidad cuando sale de la zona
            other.SendMessage("RemoveSlow", SendMessageOptions.DontRequireReceiver);
        }
    }
}