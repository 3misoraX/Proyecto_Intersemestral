using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int maxRooms = 5;
    public int maxChilds = 2;
    public float roomSize = 10;
    public int hallLength = 10;
    public GameObject startRoomPrefab;
    public GameObject bossRoomPrefab;
    public List<GameObject> roomPrefab;
    public List<GameObject> hallPrefab;
    public GameObject keyPrefab; 
    private List<Vertex> roomList = new List<Vertex>();
    private Dictionary<Vertex, Vector3> vertexPositions = new Dictionary<Vertex, Vector3>();

    private void Start()
    {
        createVertexMap();
        if (maxChilds > 3) { maxChilds = 3; }
    }

    private void createVertexMap()
    {
        roomList.Add(new Vertex(0,"node " + 0));
        for (int i = 0; i < maxRooms; i++)
        {
            int RandomRoomsAcount = UnityEngine.Random.Range(1, maxChilds+1);
            List<Vertex> tempList = new List<Vertex>();
            for (int j = roomList.Count; j < roomList.Count + RandomRoomsAcount; j++)
            {
                if (j < maxRooms)
                {
                    tempList.Add(new Vertex(roomList.Count, "node " + j , roomList[i]));
                }
            }
            if (tempList.Count > 0)
            {
                roomList.AddRange(tempList);
                roomList[i].Edges = tempList;
            }
            if (roomList.Count >= maxRooms) { break; }
        }   
        createMap();
    }

    public void createMap()
    {
        vertexPositions.Clear();
        Queue<Vertex> queue = new Queue<Vertex>();
        HashSet<Vector3> usedPositions = new HashSet<Vector3>();
        float stepDistance = Mathf.Max(1f, roomSize + hallLength);
    
        Vertex root = roomList[0];
        vertexPositions[root] = Vector3.zero;
        usedPositions.Add(Vector3.zero);
        queue.Enqueue(root);
    
        while (queue.Count > 0)
        {
            Vertex current = queue.Dequeue();
            Vector3 parentPos = vertexPositions[current];
    
            Vector3[] directions = new Vector3[]
            {
                new Vector3(stepDistance, 0, 0),
                new Vector3(-stepDistance, 0, 0),
                new Vector3(0, 0, stepDistance),
                new Vector3(0, 0, -stepDistance)
            };
    
            int dirIndex = 0;
            foreach (var child in current.Edges)
            {
                bool foundFreePosition = false;
                Vector3 candidatePos = parentPos;

                for (int ring = 1; ring <= maxRooms && !foundFreePosition; ring++)
                {
                    for (int d = 0; d < directions.Length; d++)
                    {
                        Vector3 probe = parentPos + (directions[(dirIndex + d) % directions.Length] * ring);
                        if (!usedPositions.Contains(probe))
                        {
                            candidatePos = probe;
                            foundFreePosition = true;
                            dirIndex = (dirIndex + d + 1) % directions.Length;
                            break;
                        }
                    }
                }

                if (!foundFreePosition) continue;
    
                vertexPositions[child] = candidatePos;
                usedPositions.Add(candidatePos);
                queue.Enqueue(child);
            }
        }
    
        // 1. Filtrar candidatos para el Jefe: Solo habitaciones con EXACTAMENTE una conexión (hojas del árbol)
        Vertex bossRoom = null;
        List<Vertex> candidateRooms = new List<Vertex>();
        
        foreach (var r in roomList)
        {
            if (r != root && vertexPositions.ContainsKey(r))
            {
                // Contamos cuántas conexiones reales tiene esta habitación
                int connectionCount = 0;
                if (r.ParentVertex != null && vertexPositions.ContainsKey(r.ParentVertex)) connectionCount++;
                if (r.Edges != null)
                {
                    foreach (var edge in r.Edges)
                    {
                        if (vertexPositions.ContainsKey(edge)) connectionCount++;
                    }
                }

                // Si solo tiene una conexión, es un callejón sin salida perfecto para el jefe
                if (connectionCount == 1)
                {
                    candidateRooms.Add(r);
                }
            }
        }

        // Plan B: Si por alguna razón la estructura no generó hojas, permitimos cualquier sala que no sea el root
        if (candidateRooms.Count == 0)
        {
            foreach (var r in roomList)
            {
                if (r != root && vertexPositions.ContainsKey(r)) candidateRooms.Add(r);
            }
        }

        if (candidateRooms.Count > 0)
        {
            bossRoom = candidateRooms[UnityEngine.Random.Range(0, candidateRooms.Count)];
        }

        // 2. Elegir una habitación para la llave (que sea distinta al root y al jefe, idealmente con buen flujo)
        Vertex keyRoom = null;
        List<Vertex> keyCandidateRooms = new List<Vertex>();
        foreach (var r in roomList)
        {
            if (r != root && r != bossRoom && vertexPositions.ContainsKey(r))
            {
                keyCandidateRooms.Add(r);
            }
        }
        if (keyCandidateRooms.Count > 0)
        {
            keyRoom = keyCandidateRooms[UnityEngine.Random.Range(0, keyCandidateRooms.Count)];
        }

        Dictionary<Vertex, GameObject> instantiatedRooms = new Dictionary<Vertex, GameObject>();

        foreach (var room in roomList)
        {
            if (!vertexPositions.ContainsKey(room)) continue;

            GameObject prefabToSpawn = null;

            if (room == root)
            {
                prefabToSpawn = startRoomPrefab != null ? startRoomPrefab : (roomPrefab != null && roomPrefab.Count > 0 ? roomPrefab[UnityEngine.Random.Range(0, roomPrefab.Count)] : null);
            }
            else
            {
                if (room == bossRoom && bossRoomPrefab != null)
                {
                    prefabToSpawn = bossRoomPrefab;
                }
                else if (roomPrefab != null && roomPrefab.Count > 0)
                {
                    prefabToSpawn = roomPrefab[UnityEngine.Random.Range(0, roomPrefab.Count)];
                }
            }

            if (prefabToSpawn == null) continue;

            GameObject spawnedRoom = Instantiate(prefabToSpawn, vertexPositions[room], Quaternion.identity);
            instantiatedRooms[room] = spawnedRoom;
        }
    
        // 1. Instanciamos los pasillos visuales
        foreach (var room in roomList)
        {
            if (room.ParentVertex != null && vertexPositions.ContainsKey(room.ParentVertex) && vertexPositions.ContainsKey(room))
            {
                Vector3 start = vertexPositions[room.ParentVertex];
                Vector3 end = vertexPositions[room];
                Vector3 direction = end - start;

                Vector3 stepDirection = Mathf.Abs(direction.x) >= Mathf.Abs(direction.z)
                    ? new Vector3(Mathf.Sign(direction.x), 0, 0)
                    : new Vector3(0, 0, Mathf.Sign(direction.z));

                int hallSegments = Mathf.Max(1, Mathf.RoundToInt(direction.magnitude / stepDistance));

                for (int i = 0; i < hallSegments; i++)
                {
                    Vector3 segmentCenter = start + stepDirection * (stepDistance * (i + 0.5f));
                    Quaternion rotation = Quaternion.LookRotation(stepDirection);
                    Instantiate(hallPrefab[0], segmentCenter, rotation);
                }
            }
        }

        // 2. NUEVO: Mapeo de la ruta física de los pasillos
        Dictionary<Vector3, List<Vector3>> physicalDoors = new Dictionary<Vector3, List<Vector3>>();

        foreach (var room in roomList)
        {
            if (room.ParentVertex != null && vertexPositions.ContainsKey(room.ParentVertex) && vertexPositions.ContainsKey(room))
            {
                Vector3 start = vertexPositions[room.ParentVertex];
                Vector3 end = vertexPositions[room];
                Vector3 diff = end - start;

                Vector3 stepDir = Mathf.Abs(diff.x) >= Mathf.Abs(diff.z)
                    ? new Vector3(Mathf.Sign(diff.x), 0, 0)
                    : new Vector3(0, 0, Mathf.Sign(diff.z));

                int steps = Mathf.Max(1, Mathf.RoundToInt(diff.magnitude / stepDistance));

                Vector3 current = start;
                for (int i = 0; i < steps; i++)
                {
                    Vector3 next = current + stepDir * stepDistance;

                    // Añade una puerta de salida de 'current' hacia 'next'
                    if (!physicalDoors.ContainsKey(current)) physicalDoors[current] = new List<Vector3>();
                    physicalDoors[current].Add(stepDir);

                    // Añade una puerta de entrada a 'next' desde 'current'
                    if (!physicalDoors.ContainsKey(next)) physicalDoors[next] = new List<Vector3>();
                    physicalDoors[next].Add(-stepDir);

                    current = next;
                }
            }
        }

        // 3. Configuración de Habitaciones
        RoomController bossRoomController = null;

        foreach (var room in roomList)
        {
            if (!instantiatedRooms.ContainsKey(room)) continue;

            RoomController controller = instantiatedRooms[room].GetComponent<RoomController>();
            if (controller != null)
            {
                bool hasNorth = false, hasSouth = false, hasEast = false, hasWest = false;
                Vector3 myPos = vertexPositions[room];

                // Verificamos el mapa físico en lugar de las conexiones lógicas
                if (physicalDoors.ContainsKey(myPos))
                {
                    foreach (Vector3 dir in physicalDoors[myPos])
                    {
                        if (dir.z > 0.5f) hasNorth = true;
                        if (dir.z < -0.5f) hasSouth = true;
                        if (dir.x > 0.5f) hasEast = true;
                        if (dir.x < -0.5f) hasWest = true;
                    }
                }

                controller.isBossRoom = (room == bossRoom);
                if (room == bossRoom) bossRoomController = controller;

                controller.SetupRoom(hasNorth, hasSouth, hasEast, hasWest);
            }
        }

        // 4. Instanciar la llave física en la sala elegida
        if (keyRoom != null && instantiatedRooms.ContainsKey(keyRoom) && bossRoomController != null)
        {
            // Tomamos la posición base de la sala y le sumamos 1.5 en el eje Y para que flote
            Vector3 keyPos = vertexPositions[keyRoom] + new Vector3(0f, 1.5f, 0f); 
            
            GameObject keyObj = Instantiate(keyPrefab, keyPos, Quaternion.identity);
            
            BossKey bossKeyScript = keyObj.GetComponent<BossKey>();
            if (bossKeyScript != null)
            {
                bossKeyScript.bossRoom = bossRoomController;
            }
        }
    }
}