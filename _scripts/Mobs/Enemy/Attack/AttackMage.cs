using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AttackMage : NetworkBehaviour
{
    [SerializeField] private GameObject spikeW;
    [SerializeField] private float fireRate = 1f;
    private EnemyMageAI enemyMageAI;
    private bool canSpawn = true;
    private Coroutine spawnCoroutine;
    private float currentTime;
    private float attackCooldown = 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        enemyMageAI = GetComponent<EnemyMageAI>();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (enemyMageAI.canShoot && canSpawn && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnSpike());
        }

        if (!enemyMageAI.canShoot && canSpawn && spawnCoroutine != null && Time.time >= currentTime + attackCooldown)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    [ClientRpc]
    private void ShootClientRpc(Vector3 targetPos)
    {
        SFXManager.Instance.PlaySFXClip(SFXManager.Instance.plantMageAttack, transform);

        targetPos.y -= 0.5f;
        Instantiate(spikeW, targetPos, transform.rotation);

        currentTime = Time.time;
    }

    private IEnumerator SpawnSpike()
    {
        yield return new WaitForSeconds(fireRate);
        while (enemyMageAI.canShoot)
        {
            ShootClientRpc(enemyMageAI.getTarget());

            yield return new WaitForSeconds(fireRate);
        }
    }
}