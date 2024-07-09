using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MonitorAttack : NetworkBehaviour
{
    [SerializeField] private float attackCooldown = 3f;
    [Header("Falling Objects")]
    [SerializeField] private GameObject[] fallingObjects;
    [NonSerialized] public float fallingObjectsTimer = 3f;
    [Header("Rotating Laser")]
    [SerializeField] private GameObject rotatingLaser;
    [SerializeField] private float laserTimer = 10f;
    [NonSerialized] public float rotateSpeed = 25f;
    [Header("Area Damage")]
    [SerializeField] private GameObject[] areaAttackPrefabs;
    [NonSerialized] public int safeZoneCount = 3;

    private GameObject targetPlayer;
    private GameObject[] players;
    private GameObject laserObj;
    private MonitorAI monitorAI;
    private float currentTime = 0;
    private bool isAttacking = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        FindTarget();

        monitorAI = GetComponent<MonitorAI>();

        base.OnNetworkSpawn();
    }


    private void Update()
    {
        if (!IsServer) return;

        if (monitorAI.canAttack)
        {
            if (!isAttacking)
            {
                StartAttack();
            }
        }
    }

    private void StartAttack()
    {
        if (!IsServer) return;

        // Attack
        int attackInt = UnityEngine.Random.Range(1,4);
        isAttacking = true;
        switch(attackInt)
        {
            case 1:
                StartCoroutine(FallingObjects());
                break;
            case 2:
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.rotatingLaser, transform);
                StartCoroutine(RotatingLasers());
                break;
            case 3:
                StartCoroutine(AreaDamage());
                break;
            default:
                break;
        }
    }

    private IEnumerator FallingObjects()
    {   
        Debug.Log("Falling Objects Attack");
        
        int numberToFall = UnityEngine.Random.Range(1,11);
        for(int i = 0; i < numberToFall; i++)
        {
            int index = UnityEngine.Random.Range(0,3);
            Vector3 newPos = new Vector3(targetPlayer.transform.position.x, targetPlayer.transform.position.y + 40f, targetPlayer.transform.position.z);
            SpawnFallingObjectClientRpc(index, newPos);
            yield return new WaitForSeconds(fallingObjectsTimer);
        }
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        FindTarget();
    }

    [ClientRpc]
    private void SpawnFallingObjectClientRpc(int index, Vector3 pos)
    {
        Instantiate(fallingObjects[index], pos, Quaternion.identity);
    }

    private IEnumerator RotatingLasers()
    {
        Debug.Log("Rotating Lasers Attack");

        //Vector3 newPos = new Vector3(rotatingLaser.transform.position.x, rotatingLaser.transform.position.y + 40, rotatingLaser.transform.position.z);
        SpawnRotatingLasersClientRpc();
        yield return new WaitForSeconds(laserTimer);
        DestroyRotatingLasersClientRpc();
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    [ClientRpc]
    private void SpawnRotatingLasersClientRpc()
    {
        laserObj = Instantiate(rotatingLaser, rotatingLaser.transform.position, Quaternion.identity);
    }

    [ClientRpc]
    private void DestroyRotatingLasersClientRpc()
    {
        Destroy(laserObj);
    }

    private IEnumerator AreaDamage()
    {
        Debug.Log("Area Damage Attack");

        int numberToAttack = areaAttackPrefabs.Length - safeZoneCount;

        List<int> indices = new List<int>();
        for(int i = 0; i < numberToAttack; i++)
        {
            int index = UnityEngine.Random.Range(0,7);

            while (indices.Contains(index))
            {
                index = UnityEngine.Random.Range(0, 7);
            }
            
            indices.Add(index);
        }

        for(int i = 0; i < indices.Count; i++)
        {
            SpawnLaserAttackClientRpc(indices[i]);
        }
        indices.Clear();
        
        yield return new WaitForSeconds(attackCooldown + 5);
        isAttacking = false;
    }

    private IEnumerator ActivateLaserAfterDelay(GameObject laserAttack, GameObject areaAttack)
    {
        yield return new WaitForSeconds(3f);
        SFXManager.Instance.PlaySFXClip(SFXManager.Instance.bigLaser, transform);
        areaAttack.GetComponent<MeshRenderer>().enabled = false;
        laserAttack.SetActive(true);
    }

    [ClientRpc]
    private void SpawnLaserAttackClientRpc(int index)
    {
        GameObject areaAttackObj = Instantiate(areaAttackPrefabs[index], areaAttackPrefabs[index].transform.position, areaAttackPrefabs[index].transform.rotation);
        GameObject laserAttack = areaAttackObj.transform.GetChild(0).gameObject;

        StartCoroutine(ActivateLaserAfterDelay(laserAttack, areaAttackObj));
        Destroy(areaAttackObj, 3.5f);
    }

    private void FindTarget()
    {
        targetPlayer = GameObject.FindGameObjectWithTag("Player");
        players = GameObject.FindGameObjectsWithTag("Player");

        if (!(NetworkManager.Singleton.ConnectedClientsIds.Count == 1))
        {
            int target = UnityEngine.Random.Range(0, players.Length);

            targetPlayer = players[target];
        }
    }
}
