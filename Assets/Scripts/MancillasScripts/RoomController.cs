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
    public GameObject fih;
    public bool roomError = false;

    [Header("Configuración de Jefe")]
    public bool isBossRoom = false;

    private bool isCleared = false;
    private bool isLocked = false;
    
    private List<Transform> activeDoors = new List<Transform>();
    private Dictionary<Transform, Vector3> closedPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> openPositions = new Dictionary<Transform, Vector3>();

    private CameraBounds confiner;
    public Transform confinerPos;

    void Awake()
    {
        confiner = GameObject.Find("CameraBounds").GetComponent<CameraBounds>();
    }

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

        // 3. Registrar posiciones y configurar estado inicial de las puertas
        foreach (Transform door in activeDoors)
        {
            closedPositions[door] = door.localPosition; 
            openPositions[door] = door.localPosition - new Vector3(0, doorHideDistance, 0);
            
            // Si es la sala del jefe, las puertas empiezan CERRADAS bloqueando el paso
            if (isBossRoom)
            {
                door.localPosition = closedPositions[door];
            }
            else
            {
                door.localPosition = openPositions[door];
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            confiner.inRoom = true;
            confiner.StartCoroutine(confiner.MoveBoundaries(confinerPos));
            BoxCollider confBox = gameObject.GetComponent<BoxCollider>();
            //confiner.ResizeBoundaries(confBox.size.x + 1, confBox.size.z + 1);

            if (!isCleared && !isLocked)
            {
                LockRoom(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Proteccion para que abra las puertas en caso que el jugador salga antes de completar la sala
        if(other.CompareTag("Player"))
        {
            confiner.inRoom = false;
            if (!isCleared)
            {
                isLocked = false;
                roomError = true;
                List<GameObject> enemiesToKill = new List<GameObject>();
                GameObject.FindGameObjectsWithTag("Enemy", enemiesToKill);
                foreach (GameObject item in enemiesToKill)
                {
                    Destroy(item);
                }
                StopAllCoroutines();
                StartCoroutine(AnimateDoors(false));
            }
        }
    }

    private void LockRoom(Collider other)
    {
        isLocked = true;
        roomError = false;
        StopAllCoroutines(); 
        StartCoroutine(AnimateDoors(true, other)); // True = Subir puertas
    }

    public void UnlockRoom()
    {
        if (!roomError)
        {
            isLocked = false;
            isCleared = true;
            StopAllCoroutines();
            int r = Random.Range(0, 4);
            if(r == 0)
            {
                GameObject fish = Instantiate(fih, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
            }
            StartCoroutine(AnimateDoors(false)); // False = Bajar puertas
        }
    }

    // Se ejecuta desde el script de la llave cuando el jugador la recoge
    public void UnlockBossRoom()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateDoors(false)); // Baja las puertas para permitir el paso inicial
    }

    private IEnumerator AnimateDoors(bool closing, Collider other = null)
    {
        float progress = 0f;

        if (closing)
        {
            yield return new WaitForSeconds(0.2f);
            if(other != null && other.CompareTag("Player"))
            {
                if (spawner != null)
                {
                    spawner.SpawnEnemies(this);
                }
                else
                {
                    UnlockRoom();
                }
            }
        }

        while (progress < 1f)
        {
            progress += Time.deltaTime * doorMoveSpeed;
            
            foreach (Transform door in activeDoors)
            {
                Vector3 startPos = closing ? openPositions[door] : closedPositions[door];
                Vector3 endPos = closing ? closedPositions[door] : openPositions[door];
                
                door.localPosition = Vector3.Lerp(startPos, endPos, progress);
            }
            
            yield return null; 
        }
    }
}