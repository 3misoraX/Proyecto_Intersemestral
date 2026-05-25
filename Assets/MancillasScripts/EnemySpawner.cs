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

                // Le agregamos el puente de comunicación dinámicamente
                EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
                notifier.spawner = this;
                
                activeEnemies++;
            }
        }

        // Si la lista estaba vacía, desbloqueamos la sala inmediatamente
        if (activeEnemies == 0 && currentRoom != null)
        {
            currentRoom.UnlockRoom();
        }
    }

    // El notificador llamará a este método
    public void EnemyDied()
    {
        activeEnemies--;
        
        if (activeEnemies <= 0 && currentRoom != null)
        {
            currentRoom.UnlockRoom();
        }
    }
}