using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttackLaser : NetworkBehaviour
{
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserTracker;
    [SerializeField] private float fireForce = 10f;
    [SerializeField] private float fireRate = 3f;
    [SerializeField] private LayerMask layer;
    private EnemyLaserAI enemyLaserAI;
    private Coroutine spawnCoroutine;
    private float currentTime;
    private float attackCooldown = 1f;
    public bool isShooting;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        enemyLaserAI = GetComponent<EnemyLaserAI>();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (enemyLaserAI.canShoot && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnBullet());
        }

        if (enemyLaserAI.canShoot)
        {
            RaycastHit hit;

            var fwd = transform.TransformDirection(Vector3.forward);

            if (Physics.Raycast(transform.position, fwd, out hit, 90f, layer))
            {
                ActiveLaserTrackerClientRpc(true, hit.distance);
            }
        }
        else
        {
            ActiveLaserTrackerClientRpc(false);
        }

        if (!enemyLaserAI.canShoot && spawnCoroutine != null && Time.time >= currentTime + attackCooldown)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    [ClientRpc]
    private void ActiveLaserTrackerClientRpc(bool isActive, float distance = default)
    {
        laserTracker.SetActive(isActive);

        if (isActive)
        {
            laserTracker.transform.localScale = new Vector3(0.1f, 0.2f * distance * 4f, 0.1f);
        }
    }

    [ClientRpc]
    private void ShootClientRpc(float distance)
    {
        SFXManager.Instance.PlaySFXClip(SFXManager.Instance.robotLaserAttack, transform);
        
        GameObject laserObj = Instantiate(laser, transform.position, transform.rotation);
        laserObj.transform.localScale += Vector3.forward * distance * 4f;

        Destroy(laserObj, 1f);
    }

    private IEnumerator SpawnBullet()
    {
        yield return new WaitForSeconds(fireRate);
        while (enemyLaserAI.canShoot)
        {
            isShooting = true;
            RaycastHit hit;

            var fwd = transform.TransformDirection(Vector3.forward);

            if(Physics.Raycast(transform.position, fwd, out hit, 90f, layer))
            {
                enemyLaserAI.TriggerAttackAnim();
                ShootClientRpc(hit.distance);

                yield return new WaitForSeconds(1f);
                isShooting = false;
            }
            
            currentTime = Time.time;

            yield return new WaitForSeconds(fireRate);
        }
    }
}