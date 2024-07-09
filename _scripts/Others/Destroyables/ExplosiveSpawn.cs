using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Unity.Netcode;

public class ExplosiveSpawn : NetworkBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private List<Transform> spawnpoints;
    [SerializeField] private List<GameObject> explosivePrefabs;

    private Transform[] randomizedSpawnpoints = new Transform[8];

    private int explosiveCount;


    private void Start() 
    {
        if (!IsServer) return;

        DetermineSpawnPoints();
        SpawnExplosivesServerRpc();

        navMeshSurface.BuildNavMesh();
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
        for (int i = 0; i < randomizedSpawnpoints.Length; i++)
        {
            randomizedSpawnpoints[i] = spawnpoints[i];
        }
    }

    [ServerRpc]
    private void SpawnExplosivesServerRpc()
    {
        explosiveCount = UnityEngine.Random.Range(0, 5);
        for (int i = 0; i < explosiveCount; i++)
        {
            GameObject randomexplosivePrefab = explosivePrefabs[UnityEngine.Random.Range(0, explosivePrefabs.Count)];
            GameObject explosivePrefab = Instantiate(randomexplosivePrefab, randomizedSpawnpoints[i].position, randomexplosivePrefab.transform.rotation);
            NetworkObject explosiveNetworkObject = explosivePrefab.GetComponent<NetworkObject>();
            explosiveNetworkObject.Spawn();
        }
    }
}
