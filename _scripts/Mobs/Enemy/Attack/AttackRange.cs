using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AttackRange : NetworkBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private float fireForce = 10f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private bool isTriple = false;
    private EnemyRangeAI enemyRangeAI;
    private Coroutine spawnCoroutine;
    private float currentTime;
    private float attackCooldown = 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        enemyRangeAI = GetComponent<EnemyRangeAI>();

        base.OnNetworkSpawn();
    }
    
    private void Update()
    {
        if (!IsServer) return;

        if (enemyRangeAI.canShoot && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnBullet());
        }

        if (!enemyRangeAI.canShoot && spawnCoroutine != null && Time.time >= currentTime + attackCooldown)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    [ClientRpc]
    private void ShootClientRpc()
    {
        GameObject bulletObj;
        Rigidbody bulletRb;

        if(GetComponent<EnemyStatus>().imInsect) SFXManager.Instance.PlaySFXClip(SFXManager.Instance.insectRangeAttack, transform);
        else SFXManager.Instance.PlaySFXClip(SFXManager.Instance.robotShootAttack, transform);

        bulletObj = Instantiate(bullet, transform.position, transform.rotation);
        bulletRb = bulletObj.GetComponent<Rigidbody>();
        bulletRb.AddForce(transform.forward * fireForce * 20f);

        if(isTriple)
        {
            bulletObj = Instantiate(bullet, transform.position, transform.rotation * Quaternion.Euler(0, 30, 0));
            bulletRb = bulletObj.GetComponent<Rigidbody>();
            bulletRb.AddForce(bulletObj.transform.forward * fireForce * 20f);

            bulletObj = Instantiate(bullet, transform.position, transform.rotation * Quaternion.Euler(0, -30, 0));
            bulletRb = bulletObj.GetComponent<Rigidbody>();
            bulletRb.AddForce(bulletObj.transform.forward * fireForce * 20f);
        }
        

        currentTime = Time.time;
    }

    private IEnumerator SpawnBullet()
    {
        yield return new WaitForSeconds(fireRate);
        while (true)
        {
            enemyRangeAI.TriggerAttackAnim();
            yield return new WaitForSeconds(0.3f);
            ShootClientRpc();

            yield return new WaitForSeconds(fireRate);
        }
    }
}