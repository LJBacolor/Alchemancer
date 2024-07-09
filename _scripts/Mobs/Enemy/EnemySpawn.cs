using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;


public class EnemySpawn : NetworkBehaviour
{
    [SerializeField] private int sectorNumber;
    [SerializeField] private List<Transform> spawnpoints;
    [SerializeField] private List<GameObject> insectEnemies;
    [SerializeField] private List<GameObject> plantEnemies;
    [SerializeField] private List<GameObject> robotEnemies;

    [SerializeField] private GameObject buffBox;

    private Transform[] firstWaveSpawnpoints = new Transform[8];
    private Transform[] secondWaveSpawnpoints = new Transform[8];

    private int enemyCount;
    private bool inSecondWave = false;
    private bool isOver = false;

    public override void OnNetworkSpawn() 
    {
        if (!IsServer) return;

        DetermineSpawnPoints();
        SpawnFirstWave();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!inSecondWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            SpawnSecondWave();
        }

        if(inSecondWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            OpenDoorClientRpc();
            SpawnChestClientRpc();
            ToggleEnterSectorClientRpc(true);
        }
        else
        {
            ToggleEnterSectorClientRpc(false);
        }
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        GameObject sectorDoor = GameObject.Find("Door");
        sectorDoor.GetComponent<Door>().canEnter = true;

        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerPointer>().SetTarget(sectorDoor.transform);
    }

    [ClientRpc]
    private void ToggleEnterSectorClientRpc(bool open)
    {
        GameObject sectorDoor = GameObject.Find("Enter Sector");
        sectorDoor.GetComponent<BoxCollider>().enabled = open;
    }

    [ClientRpc]
    private void SpawnChestClientRpc()
    {
        if (!isOver)
        {
            RespawnDeadPlayer();

            Transform chestPos = GameObject.Find("Door").transform;
            Vector3 chestSpawn = new Vector3(chestPos.position.x - 4f, 0.6f, chestPos.position.z - 4f);

            Instantiate(buffBox,chestSpawn, Quaternion.identity);
        }
        isOver = true;
    }

    private void RespawnDeadPlayer()
    {
        if (NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().isDead.Value)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().RespawnPlayer();
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

    private void SpawnFirstWave()
    {
        enemyCount = UnityEngine.Random.Range(3, 6);
        for (int i = 0; i < enemyCount; i++)
        {
            switch(sectorNumber)
            {
                case 1:
                    spawnS1EnemiesServerRpc(i);
                    break;
                case 2:
                    spawnS2EnemiesServerRpc(i);
                    break;
                case 3:
                    spawnS3EnemiesServerRpc(i);
                    break;
                default:
                    break;
            }
        }
    }

    private void SpawnSecondWave()
    {
        enemyCount = UnityEngine.Random.Range(6, 9);
        inSecondWave = true;
        for (int i = 0; i < enemyCount; i++)
        {
            switch(sectorNumber)
            {
                case 1:
                    spawnS1EnemiesServerRpc(i);
                    break;
                case 2:
                    spawnS2EnemiesServerRpc(i);
                    break;
                case 3:
                    spawnS3EnemiesServerRpc(i);
                    break;
                default:
                    break;
            }
        }
    }

    [ServerRpc]
    private void spawnS1EnemiesServerRpc(int index)
    {
        GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
        GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[index].position, Quaternion.identity);
        NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
        enemyNetworkObject.Spawn();
    }

    [ServerRpc]
    private void spawnS2EnemiesServerRpc(int index)
    {
        int pickRate = UnityEngine.Random.Range(1, 11);
        if(pickRate <= 2)
        {
            GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[index].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
        else if(pickRate >= 3)
        {
            GameObject randomEnemyPrefab = plantEnemies[UnityEngine.Random.Range(0, plantEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[index].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
    }

    [ServerRpc]
    private void spawnS3EnemiesServerRpc(int index)
    {
        int pickRate = UnityEngine.Random.Range(1, 101);
        if(pickRate >= 30)
        {
            GameObject randomEnemyPrefab = robotEnemies[UnityEngine.Random.Range(0, robotEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[index].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
        else if(pickRate >= 15)
        {
            GameObject randomEnemyPrefab = insectEnemies[UnityEngine.Random.Range(0, insectEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[index].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
        else if(pickRate >= 1)
        {
            GameObject randomEnemyPrefab = plantEnemies[UnityEngine.Random.Range(0, plantEnemies.Count)];
            GameObject enemyObject = Instantiate(randomEnemyPrefab, firstWaveSpawnpoints[index].position, Quaternion.identity);
            NetworkObject enemyNetworkObject = enemyObject.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();
        }
    }
}