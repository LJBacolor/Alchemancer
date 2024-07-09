using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WormBossEnemiesSpawn : NetworkBehaviour
{
    [SerializeField] private WormAI wormAI;
    [SerializeField] private List<Transform> spawnpoints;
    [SerializeField] private List<GameObject> insectEnemies;

    private Transform[] firstWaveSpawnpoints = new Transform[8];
    private Transform[] secondWaveSpawnpoints = new Transform[8];

    private int enemyCount;
    private bool inSecondWave = false;

    private void Start() 
    {
        if (!IsServer) return;

        DetermineSpawnPoints();
    }

    private void Update()
    {
        if (!IsServer) return;
        CheckWave();
    }

    private void CheckWave()
    {
        if(wormAI.firstPhase && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            wormAI.canJump = true;
        }

        if(wormAI.secondPhase && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            wormAI.canJump = true;
        }

        if(GameObject.FindGameObjectsWithTag("Worm").Length == 0)
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

    [ServerRpc]
    public void SpawnFirstWaveServerRpc()
    {
        enemyCount = UnityEngine.Random.Range(3, 6);
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[i].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
    }

    [ServerRpc]
    public void SpawnSecondWaveServerRpc()
    {
        enemyCount = UnityEngine.Random.Range(6, 9);
        inSecondWave = true;
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, secondWaveSpawnpoints[i].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
    }
}