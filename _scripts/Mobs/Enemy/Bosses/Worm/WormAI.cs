using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class WormAI : NetworkBehaviour
{
    [SerializeField] private float JumpPower = 5;
    [SerializeField] private int Gap = 10;
    [SerializeField] private int BodyLength = 40;
    //[SerializeField] private float radius = 3;
    [SerializeField] private float minCooldown = 4f;
    [SerializeField] private float maxCooldown = 6f;
    [SerializeField] private GameObject HeadPrefab;
    [SerializeField] private GameObject BodyPrefab;
    [SerializeField] private GameObject BodyPrefab2;
    [SerializeField] private GameObject Warning;
    [SerializeField] private WormBossEnemiesSpawn wormBossEnemiesSpawn;

    private List<GameObject> BodyParts = new List<GameObject>();
    private List<Vector3> PosHistory = new List<Vector3>();

    private EnemyHealth enemyHealth;
    private Rigidbody rb;
    private Transform targetPlayer;
    private GameObject[] players;
    private float cooldownTimer = 0f;

    public bool canJump = true;
    [NonSerialized] public bool firstPhase = false;
    [NonSerialized] public bool secondPhase = false;

    private void Awake()
    {
        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        players = GameObject.FindGameObjectsWithTag("Player");
        rb = GetComponent<Rigidbody>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        AddHead();
        AddBody(BodyLength);

        if (!IsServer) return;

        Gap = Mathf.RoundToInt(Gap / 1.5f);

        SFXManager.Instance.PlaySFXClip(SFXManager.Instance.wormDig, transform);
        StartCoroutine(Attack());

        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    private void Update()
    {
        PosHistory.Insert(0,transform.position);

        if(PosHistory.Count > BodyLength * 20) 
        {
            PosHistory.RemoveAt(BodyLength * 20);
        }

        int index = 0;

        foreach(var BodyPart in BodyParts)
        {
            Vector3 point = PosHistory[Mathf.Clamp(index * Gap, 0, PosHistory.Count - 1)];
            Vector3 moveDirection = point - BodyPart.transform.position;

            BodyPart.transform.position += moveDirection * JumpPower * Time.deltaTime;
            BodyPart.transform.LookAt(point);
            index++;
        }

        if (!IsServer) return;

        if (!(NetworkManager.Singleton.ConnectedClientsIds.Count == 1))
        {
            float curDistance = Mathf.Infinity;

            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);

                if (distance < curDistance)
                {
                    targetPlayer = player.transform;
                    curDistance = distance;
                }
            }
        }

        if (cooldownTimer <= 0f)
        {
            if(canJump)
            {
                StartCoroutine(Attack());
            }
        }

        cooldownTimer -= Time.deltaTime;

    }

    private void AddBody(int count)
    {
        for(int i = 0; i < count; i++)
        {
            if (i % 3 == 1)
            {
                GameObject Body = Instantiate(BodyPrefab2);
                BodyParts.Add(Body);
            }
            else
            {
                GameObject Body = Instantiate(BodyPrefab);
                BodyParts.Add(Body);
            }
        }
    }

    private void AddHead()
    {
        GameObject Head = Instantiate(HeadPrefab);
        BodyParts.Add(Head);
    }

    [ClientRpc]
    private void JumpClientRpc(Vector3 target,int x,int y)
    {
        rb.velocity = Vector3.zero;
        transform.position = target;
        transform.rotation = Quaternion.Euler(x,y,0);

        rb.AddForce(transform.forward * JumpPower * 100);
    }

    IEnumerator Attack()
    {
        Vector3 warningPos = new Vector3(targetPlayer.position.x, targetPlayer.position.y - 0.5f, targetPlayer.position.z);
        cooldownTimer = UnityEngine.Random.Range(minCooldown, maxCooldown) * 1;
        WarnClientRpc(warningPos);

        yield return new WaitForSeconds(1f);

        JumpClientRpc(warningPos, UnityEngine.Random.Range(-60, -80), UnityEngine.Random.Range(0, 360));
    }

    [ClientRpc]
    private void WarnClientRpc(Vector3 warningPos)
    {
        GameObject Warn = Instantiate(Warning, warningPos, Quaternion.identity);
        Destroy(Warn, 2f);
    }

    public void testPhase()
    {
        if(!IsServer) { return; }

        if(!firstPhase)
        {
            if(enemyHealth.health.Value <= (enemyHealth.maxHealth.Value * 0.66f) && enemyHealth.health.Value > (enemyHealth.maxHealth.Value * 0.33f))
            {
                canJump = false;
                firstPhase = true;
                wormBossEnemiesSpawn.SpawnFirstWaveServerRpc();
            }
        }
        else
        {
            if(!canJump)
            {
                enemyHealth.health.Value = enemyHealth.maxHealth.Value * 0.66f;
            }
        }

        if(!secondPhase)
        {
            if(enemyHealth.health.Value <= (enemyHealth.maxHealth.Value * 0.33f))
            {
                canJump = false;
                secondPhase = true;
                wormBossEnemiesSpawn.SpawnSecondWaveServerRpc();
            }
        }
        else
        {
            if(!canJump)
            {
                enemyHealth.health.Value = enemyHealth.maxHealth.Value * 0.33f;
            }
        }
    }
}