using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CenterSurrounder : NetworkBehaviour
{
    [Header("Surround Attack")]
    [SerializeField] private GameObject surrounderObjPrefab;
    [SerializeField] private int SurrounderObjCount;
    [SerializeField] private float AppearWaitDuration = 0.3f;

    [Header("Instant Attack")]
    [SerializeField] private GameObject instantObjPrefab;
    [SerializeField] private int InstantObjCount;
    [SerializeField] private int InstantAttackCount = 18;
    [SerializeField] private float InstantWaitDuration = .5f;

    [Header("Area Attack")]
    [SerializeField] private GameObject areaObjPrefab;

    private Transform targetPlayer;
    private GameObject[] players;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        players = GameObject.FindGameObjectsWithTag("Player");

        if (!(NetworkManager.Singleton.ConnectedClientsIds.Count == 1))
        {
            int target = Random.Range(0, players.Length);

            targetPlayer = players[target].transform;
        }

        int attackInt;
        attackInt = Random.Range(1, 4);

        switch (attackInt)
        {
            case 1:
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.daisySurround, transform);
                StartCoroutine(Surround());
                break;
            case 2:
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.daisyInstant, transform);
                StartCoroutine(Instant());
                break;
            case 3:
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.daisySurround, transform);
                StartCoroutine(Area());
                break;
            default:
                break;
        }

        StartCoroutine(DespawnObj());

        base.OnNetworkSpawn();
    }

    IEnumerator DespawnObj()
    {
        yield return new WaitForSeconds(10f);
        gameObject.GetComponent<NetworkObject>().Despawn();
    }

    //Surround Attack
    IEnumerator Surround()
    {
        Vector3 target = targetPlayer.position + new Vector3(0,0.5f,0);

        float AngleStep = 360.0f / SurrounderObjCount;

        for (int i = 0; i < SurrounderObjCount; i++)
        {
            SpawnSurroundClientRpc(target,AngleStep,i);

            yield return new WaitForSeconds(AppearWaitDuration);
        }
        StartRotateClientRpc();

        yield return new WaitForSeconds(2f);

        StopRotateClientRpc();
    }

    [ClientRpc]
    private void SpawnSurroundClientRpc(Vector3 target,float AngleStep, int i)
    {
        GameObject newSurrounderObject = Instantiate(surrounderObjPrefab, transform);

        newSurrounderObject.transform.RotateAround(transform.position, Vector3.up, AngleStep * i);

        newSurrounderObject.GetComponent<DaisyBullet>().Shoot(target);
    }


    //Instant Attack
    IEnumerator Instant()
    {
        StartRotateClientRpc();
        float AngleStep = 360 / InstantObjCount;
        for (int x = 0; x < InstantAttackCount; x++)
        {
            for (int i = 0; i < InstantObjCount; i++)
            {
                SpawnInstantClientRpc(AngleStep, i);
            }
            yield return new WaitForSeconds(InstantWaitDuration);
        }
    }

    [ClientRpc]
    private void SpawnInstantClientRpc(float AngleStep, int index)
    {
        GameObject newSurrounderObject = Instantiate(instantObjPrefab, transform);

        newSurrounderObject.transform.RotateAround(transform.position, Vector3.up, AngleStep * index);

        Rigidbody rb = newSurrounderObject.GetComponent<Rigidbody>();
        rb.AddForce(newSurrounderObject.transform.forward * 15, ForceMode.Impulse);
    }

    [ClientRpc]
    private void StartRotateClientRpc()
    {
        GetComponent<RotateSurounder>().StartRotation(transform);
    }

    [ClientRpc]
    private void StopRotateClientRpc()
    {
        GetComponent<RotateSurounder>().enabled = false;
    }
    //Area Attack
    IEnumerator Area()
    {
        Vector3 target = targetPlayer.position + Vector3.down;

        for (int i = 0; i < 2; i++)
        {
            SpawnAreaAttackClientRpc(i, target);

            yield return new WaitForSeconds(1 + i);
        }
    }

    [ClientRpc]
    private void SpawnAreaAttackClientRpc(int i, Vector3 target)
    {
        GameObject newSurrounderObject = Instantiate(areaObjPrefab, transform);

        newSurrounderObject.transform.RotateAround(transform.position, Vector3.up, 160 * i);

        newSurrounderObject.GetComponent<DaisyAreaBullet>().Shoot(target);
    }
}