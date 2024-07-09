using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MonitorPhase : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnpoints;
    [SerializeField] private List<GameObject> insectEnemies;
    [SerializeField] private List<GameObject> plantEnemies;
    [SerializeField] private List<GameObject> robotEnemies;
    private MonitorAI monitorAI;
    private Transform[] firstWaveSpawnpoints = new Transform[8];
    private Transform[] secondWaveSpawnpoints = new Transform[8];

    private int enemyCount;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        DetermineSpawnPoints();
        monitorAI = GetComponent<MonitorAI>();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;
        CheckWave();
    }

    private void CheckWave()
    {
        if(monitorAI.firstPhase && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            monitorAI.onShield = false;
        }

        if(monitorAI.secondPhase && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            monitorAI.onShield = false;
        }

        if(GameObject.FindGameObjectsWithTag("Monitor").Length == 0)
        {
            GameObject sectorDoor = GameObject.Find("Door");
            sectorDoor.GetComponent<Door>().canEnter = true;
        }
    }

    private void ShuffleSpawnpoints()
    {
        // Fisher-Yates shuffle algorithm
        for (int i = 0; i < spawnpoints.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, spawnpoints.Count);
            Transform temp = spawnpoints[randomIndex];
            spawnpoints[randomIndex] = spawnpoints[i];
            spawnpoints[i] = temp;
        }
    }

    private void DetermineSpawnPoints()
    {
        ShuffleSpawnpoints();
        for (int i = 0; i < firstWaveSpawnpoints.Length; i++)
        {
            firstWaveSpawnpoints[i] = spawnpoints[i];
        }

        ShuffleSpawnpoints();
        for (int i = 0; i < secondWaveSpawnpoints.Length; i++)
        {
            secondWaveSpawnpoints[i] = spawnpoints[i];
        }
    }

    public void SpawnFirstWave()
    {
        enemyCount = UnityEngine.Random.Range(3, 6);
        for (int i = 0; i < enemyCount; i++)
        {
            int pickRate = UnityEngine.Random.Range(1, 101);
            if(pickRate <= 15)
            {
                GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
                GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
                NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
                enemyNetworkObject.Spawn();
            }
            else if(pickRate > 15 && pickRate <= 30)
            {
                GameObject randomEnemyPrefab = plantEnemies[UnityEngine.Random.Range(0, plantEnemies.Count)];
                GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
                NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
                enemyNetworkObject.Spawn();
            }
            else if(pickRate > 30)
            {
                GameObject randomEnemyPrefab = robotEnemies[UnityEngine.Random.Range(0, plantEnemies.Count)];
                GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
                NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
                enemyNetworkObject.Spawn();
            }
        }

        GameObject laser = GameObject.Find("Rotating Laser(Clone)");
        if(laser != null) Destroy(laser);
    }

    public void SpawnSecondWave()
    {
        enemyCount = UnityEngine.Random.Range(6, 9);

        for (int i = 0; i < enemyCount; i++)
        {
            int pickRate = UnityEngine.Random.Range(1, 101);
            if(pickRate <= 15)
            {
                GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
                GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
                NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
                enemyNetworkObject.Spawn();
            }
            else if(pickRate > 15 && pickRate <= 30)
            {
                GameObject randomEnemyPrefab = plantEnemies[UnityEngine.Random.Range(0, plantEnemies.Count)];
                GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
                NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
                enemyNetworkObject.Spawn();
            }
            else if(pickRate > 30)
            {
                GameObject randomEnemyPrefab = robotEnemies[UnityEngine.Random.Range(0, plantEnemies.Count)];
                GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
                NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
                enemyNetworkObject.Spawn();
            }
        }

        GameObject laser = GameObject.Find("Rotating Laser(Clone)");
        if(laser != null) Destroy(laser);
    }
}
