using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AttackCannon : NetworkBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject warningArea;
    [SerializeField] private float fireForce = 10f;
    [SerializeField] private float fireRate = 3f;
    private EnemyCannonAI enemyCannonAI;
    private Coroutine spawnCoroutine;
    private float currentTime;
    private float attackCooldown = 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        enemyCannonAI = GetComponent<EnemyCannonAI>();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (enemyCannonAI.canShoot && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnBullet());
        }

        if (!enemyCannonAI.canShoot && spawnCoroutine != null && Time.time >= currentTime + attackCooldown)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    [ClientRpc]
    private void ShootClientRpc(Vector3 targetPos)
    {
        float distanceMultiplier = Vector3.Distance(transform.position, targetPos);
        Vector3 upwardOffset = new Vector3(0f, 2f, 0f);

        GameObject bulletObj = Instantiate(bullet, transform.position, transform.rotation);
        Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
        bulletRb.AddForce((transform.forward + upwardOffset) * (distanceMultiplier * fireForce));

        currentTime = Time.time;
    }

    private IEnumerator SpawnBullet()
    {
        yield return new WaitForSeconds(fireRate);
        while (enemyCannonAI.canShoot)
        {
            enemyCannonAI.TriggerAttackAnim();
            yield return new WaitForSeconds(0.6f);
            ShootClientRpc(enemyCannonAI.getTarget());

            yield return new WaitForSeconds(fireRate);
        }
    }
}