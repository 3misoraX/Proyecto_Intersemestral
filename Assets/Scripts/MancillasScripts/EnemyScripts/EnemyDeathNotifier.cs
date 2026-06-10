using UnityEngine;

public class EnemyDeathNotifier : MonoBehaviour
{
    [HideInInspector] public EnemySpawner spawner;
    private bool hasNotified = false;

    // Se llama cuando el enemigo ejecuta su animación de muerte
    public void NotifyDeath()
    {
        if (!hasNotified && spawner != null)
        {
            hasNotified = true;
            spawner.EnemyDied();
        }
    }

    // Por seguridad: si el enemigo es destruido por caer al vacío u otra razón externa
    private void OnDestroy()
    {
        NotifyDeath();
    }
}