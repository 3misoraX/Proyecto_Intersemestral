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
    public GameObject keyPrefab; // Prefab de la llave asignable en inspector
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
    
        Vertex bossRoom = null;
        List<Vertex> candidateRooms = new List<Vertex>();
        foreach (var r in roomList)
        {
            if (r != root && vertexPositions.ContainsKey(r))
            {
                candidateRooms.Add(r);
            }
        }
        if (candidateRooms.Count > 0)
        {
            bossRoom = candidateRooms[UnityEngine.Random.Range(0, candidateRooms.Count)];
        }

        // Elegir una habitación aleatoria para la llave (que no sea raíz ni el jefe)
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

        RoomController bossRoomController = null;

        foreach (var room in roomList)
        {
            if (!instantiatedRooms.ContainsKey(room)) continue;

            RoomController controller = instantiatedRooms[room].GetComponent<RoomController>();
            if (controller != null)
            {
                bool hasNorth = false, hasSouth = false, hasEast = false, hasWest = false;
                List<Vertex> connectedNodes = new List<Vertex>(room.Edges);
                if (room.ParentVertex != null) connectedNodes.Add(room.ParentVertex);

                Vector3 myPos = vertexPositions[room];

                foreach (var node in connectedNodes)
                {
                    if (!vertexPositions.ContainsKey(node)) continue;
                    
                    Vector3 nodePos = vertexPositions[node];
                    Vector3 dir = (nodePos - myPos).normalized;

                    if (dir.z > 0.5f) hasNorth = true;
                    if (dir.z < -0.5f) hasSouth = true;
                    if (dir.x > 0.5f) hasEast = true;
                    if (dir.x < -0.5f) hasWest = true;
                }

                // Marcamos si es la habitación del jefe antes de configurar salas
                controller.isBossRoom = (room == bossRoom);
                if (room == bossRoom) bossRoomController = controller;

                controller.SetupRoom(hasNorth, hasSouth, hasEast, hasWest);
            }
        }

        // Instanciar la llave física en el centro de la sala elegida
        if (keyRoom != null && instantiatedRooms.ContainsKey(keyRoom) && bossRoomController != null)
        {
            Vector3 keyPos = vertexPositions[keyRoom];
            GameObject keyObj = Instantiate(keyPrefab, keyPos, Quaternion.identity);
            
            BossKey bossKeyScript = keyObj.GetComponent<BossKey>();
            if (bossKeyScript != null)
            {
                bossKeyScript.bossRoom = bossRoomController;
            }
        }
    }
}