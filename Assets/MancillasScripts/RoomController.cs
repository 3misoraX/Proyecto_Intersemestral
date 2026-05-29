using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Puertas (Se cierran en combate)")]
    public GameObject doorNorth;
    public GameObject doorSouth;
    public GameObject doorEast;
    public GameObject doorWest;

    [Header("Muros (Bloquean el vacío)")]
    public GameObject wallNorth;
    public GameObject wallSouth;
    public GameObject wallEast;
    public GameObject wallWest;

    [Header("Referencias")]
    public EnemySpawner spawner;

    [HideInInspector]public bool isCleared = false;
    private bool isLocked = false;

    // Configura los muros y puertas según las conexiones que dicte el MapGenerator
    public void SetupRoom(bool hasNorth, bool hasSouth, bool hasEast, bool hasWest)
    {
        // Activa el muro si NO hay conexión. Activa la puerta si SÍ hay conexión.
        if (wallNorth) wallNorth.SetActive(!hasNorth);
        if (doorNorth) doorNorth.SetActive(hasNorth);

        if (wallSouth) wallSouth.SetActive(!hasSouth);
        if (doorSouth) doorSouth.SetActive(hasSouth);

        if (wallEast) wallEast.SetActive(!hasEast);
        if (doorEast) doorEast.SetActive(hasEast);

        if (wallWest) wallWest.SetActive(!hasWest);
        if (doorWest) doorWest.SetActive(hasWest);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta al jugador. Si la sala no ha sido limpiada ni bloqueada, iniciamos el combate.
        if (other.CompareTag("Player") && !isCleared && !isLocked)
        {
            LockRoom();
            
            if (spawner != null)
            {
                spawner.SpawnEnemies(this);
            }
            else
            {
                UnlockRoom(); // Si es una sala segura sin spawner, se abre de inmediato
            }
        }
    }

    private void LockRoom()
    {
        isLocked = true;
        // Disparar animaciones para cerrar las puertas visualmente
        Debug.Log("Jugador entró. ¡Puertas cerradas!");
    }

    public void UnlockRoom()
    {
        isLocked = false;
        isCleared = true;
        // Disparar animaciones para abrir las puertas
        Debug.Log("¡Habitación despejada! Las puertas se abren.");
    }
}