using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class DestroyableRespawn : NetworkBehaviour
{
    [SerializeField] private GameObject destroyableObj;
    private GameObject gameObj;
    private bool canSpawn = true;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SpawnServerRpc();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer)
        {
            StopAllCoroutines();
            return;
        }

        if (!gameObj && canSpawn)
        {
            StartCoroutine(Respawn());
        }  
    }

    IEnumerator Respawn()
    {
        canSpawn = false;
        yield return new WaitForSeconds(3f);
        SpawnServerRpc();
        canSpawn = true;
    }

    [ServerRpc]
    private void SpawnServerRpc()
    {
         gameObj = Instantiate(destroyableObj, transform.position, transform.rotation);
         NetworkObject gameNetworkObject = gameObj.GetComponent<NetworkObject>();
         gameNetworkObject.Spawn();
    }
}
