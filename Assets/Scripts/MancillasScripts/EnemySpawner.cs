using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int amountToSpawn;
}

public class EnemySpawner : MonoBehaviour
{
    public List<EnemyGroup> enemiesToSpawn;
    public Transform[] spawnPoints; 

    private int activeEnemies = 0;
    private RoomController currentRoom;

    public void SpawnEnemies(RoomController room)
    {
        currentRoom = room;
        activeEnemies = 0;

        foreach (var group in enemiesToSpawn)
        {
            for (int i = 0; i < group.amountToSpawn; i++)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemy = Instantiate(group.enemyPrefab, sp.position, Quaternion.identity);

                EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
                notifier.spawner = this;
                
                activeEnemies++;
            }
        }

        Debug.Log($"<color=yellow>Habitación activada. Enemigos totales generados: {activeEnemies}</color>");

        if (activeEnemies == 0 && currentRoom != null)
        {
            Debug.Log("<color=green>No había enemigos en la lista. Abriendo habitación instantáneamente.</color>");
            currentRoom.UnlockRoom();
        }
    }

    public void EnemyDied()
    {
        activeEnemies--;
        Debug.Log($"<color=orange>Un enemigo murió. Enemigos restantes en la sala: {activeEnemies}</color>");
        
        if (activeEnemies <= 0 && currentRoom != null)
        {
            Debug.Log("<color=green>¡Todos los enemigos eliminados! </color>");
            currentRoom.UnlockRoom();
        }
    }
}