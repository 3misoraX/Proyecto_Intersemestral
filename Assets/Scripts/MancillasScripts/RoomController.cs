using System.Collections;
using System.Collections.Generic;
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

    [Header("Animación de Puertas")]
    [Tooltip("Distancia en el eje Y que bajarán las puertas para esconderse")]
    public float doorHideDistance = 4f; 
    [Tooltip("Velocidad a la que suben y bajan las puertas")]
    public float doorMoveSpeed = 3f;

    [Header("Referencias")]
    public EnemySpawner spawner;

    private bool isCleared = false;
    private bool isLocked = false;
    
    // Listas para manejar solo las puertas que existen en esta habitación
    private List<Transform> activeDoors = new List<Transform>();
    private Dictionary<Transform, Vector3> closedPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> openPositions = new Dictionary<Transform, Vector3>();

    public void SetupRoom(bool hasNorth, bool hasSouth, bool hasEast, bool hasWest)
    {
        // 1. Configurar Muros (Aparecen si NO hay camino)
        if (wallNorth) wallNorth.SetActive(!hasNorth);
        if (wallSouth) wallSouth.SetActive(!hasSouth);
        if (wallEast) wallEast.SetActive(!hasEast);
        if (wallWest) wallWest.SetActive(!hasWest);

        // 2. Configurar Puertas (Aparecen si SÍ hay camino, pero las guardamos para animarlas)
        if (doorNorth) { doorNorth.SetActive(hasNorth); if (hasNorth) activeDoors.Add(doorNorth.transform); }
        if (doorSouth) { doorSouth.SetActive(hasSouth); if (hasSouth) activeDoors.Add(doorSouth.transform); }
        if (doorEast) { doorEast.SetActive(hasEast); if (hasEast) activeDoors.Add(doorEast.transform); }
        if (doorWest) { doorWest.SetActive(hasWest); if (hasWest) activeDoors.Add(doorWest.transform); }

        // 3. Registrar posiciones y esconder las puertas bajo el suelo
        foreach (Transform door in activeDoors)
        {
            // La posición en el prefab será la posición "Cerrada" (bloqueando el paso)
            closedPositions[door] = door.localPosition; 
            
            // La posición "Abierta" será X metros más abajo
            openPositions[door] = door.localPosition - new Vector3(0, doorHideDistance, 0);
            
            // Al iniciar la sala, las puertas están abiertas (escondidas abajo)
            door.localPosition = openPositions[door];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCleared && !isLocked)
        {
            LockRoom();
            
            if (spawner != null)
            {
                spawner.SpawnEnemies(this);
            }
            else
            {
                UnlockRoom(); // Si no hay enemigos, se vuelve a abrir al instante
            }
        }
    }

    private void LockRoom()
    {
        isLocked = true;
        StopAllCoroutines(); // Detiene cualquier movimiento previo por seguridad
        StartCoroutine(AnimateDoors(true)); // True = Subir puertas
    }

    public void UnlockRoom()
    {
        isLocked = false;
        isCleared = true;
        StopAllCoroutines();
        StartCoroutine(AnimateDoors(false)); // False = Bajar puertas
    }

    // Corrutina que mueve las puertas suavemente a lo largo del tiempo
    private IEnumerator AnimateDoors(bool closing)
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * doorMoveSpeed;
            
            foreach (Transform door in activeDoors)
            {
                Vector3 startPos = closing ? openPositions[door] : closedPositions[door];
                Vector3 endPos = closing ? closedPositions[door] : openPositions[door];
                
                // Lerp crea una transición fluida entre dos posiciones
                door.localPosition = Vector3.Lerp(startPos, endPos, progress);
            }
            
            yield return null; // Espera al siguiente frame
        }
    }
}