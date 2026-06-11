using UnityEngine;

public class BossKey : MonoBehaviour
{
    [HideInInspector] public RoomController bossRoom;

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