using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class StatusBullet : NetworkBehaviour
{
    [SerializeField] private GameObject oxygenHit;
    [SerializeField] private GameObject nitrogenHit;
    [SerializeField] private GameObject carbonHit;
    [SerializeField] private GameObject oxyAreaCollider;

    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletForce = 10f;

    [SerializeField] private bool isStraight = true;
    [SerializeField] private bool isCannon = false;

    private NetworkVariable<Vector3> targetpos = new NetworkVariable<Vector3>(Vector3.zero,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    private float damage;
    private GameObject hitVFX;
    private string status;

    private NetworkObject networkObject;

    private NetworkVariable<float> nitroDmg = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> carbonDmg = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> oxyDmg = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> reactionDmg = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> critReact = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> devolved = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void Start()
    {
        if (isStraight)
        {
            Rigidbody bulletRb = GetComponent<Rigidbody>();
            bulletRb.AddForce(transform.forward * bulletSpeed * 100);
        }
        else if (isCannon)
        {
            SetOwnerIdServerRpc();

            LaunchObjectClientRpc(targetpos.Value);
        }

        if (IsOwner)
        {
            nitroDmg.Value = PlayerStats.nitrogenDamage;
            carbonDmg.Value = PlayerStats.carbonDamage;
            oxyDmg.Value = PlayerStats.oxygenDamage;
            reactionDmg.Value = PlayerStats.reactionDamage;

            critReact.Value = PlayerBuffs.Criteactions;
            devolved.Value = PlayerBuffs.Devolved;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetOwnerIdServerRpc()
    {
        targetpos.Value = NetworkManager.Singleton.ConnectedClients[networkObject.OwnerClientId].PlayerObject.GetComponent<PlayerAttack>().targetPosition.Value;
    }

    [ClientRpc]
    private void LaunchObjectClientRpc(Vector3 targetpos)
    {
        float distanceMultiplier = Vector3.Distance(transform.position, targetpos);

        Rigidbody bulletRb = GetComponent<Rigidbody>();
        bulletRb.AddForce((transform.forward + new Vector3(0f, 2f, 0f)) * (distanceMultiplier * bulletForce) * 10);
    }

    private void Update()
    {
        if (isCannon)
        {
            transform.LookAt(targetpos.Value + GetComponent<Rigidbody>().velocity);
        }
    }

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Obstacle")
        {
            CheckBullet();
            DestroyNetworkObject();
        }
        else if (col.tag == "Ground")
        {
            CheckBullet();
            DestroyNetworkObject();
        }
        else if (col.tag == "Acid")
        {
            CheckBullet();
            DestroyNetworkObject();
        }
        else if (col.tag == "Explosive")
        {
            CheckBullet();
            StatusExplosive statusExplosive = col.gameObject.GetComponent<StatusExplosive>();
            statusExplosive.TakeDamage(damage);
            DestroyNetworkObject();
        }
        else if (col.tag == "Dummy")
        {
            CheckBullet();
            DummyStatus dummyStatus = col.gameObject.GetComponent<DummyStatus>();
            dummyStatus.setReactionDmg(reactionDmg.Value);
            DummyHealth dummyHealth = col.gameObject.GetComponent<DummyHealth>();
            dummyHealth.TakeDamage(damage);
            DestroyNetworkObject();
        }
        else if (col.tag == "Enemy")
        {
            CheckBullet();
            EnemyStatus enemyStatus = col.gameObject.GetComponent<EnemyStatus>();
            enemyStatus.setReactionDmg(reactionDmg.Value,critReact.Value,devolved.Value);
            EnemyHealth enemyHealth = col.gameObject.GetComponent<EnemyHealth>();
            enemyHealth.TakeDamage(damage);
            DestroyNetworkObject();
        }
        else if (col.gameObject.tag == "Worm")
        {
            CheckBullet();
            EnemyHealth wormHealth = GameObject.Find("Worm").GetComponent<EnemyHealth>();
            wormHealth.TakeDamage(damage);
            DestroyNetworkObject();
        }
        else if (col.gameObject.tag == "Daisy")
        {
            CheckBullet();
            EnemyHealth daisyHealth = GameObject.Find("Daisy").GetComponent<EnemyHealth>();
            daisyHealth.TakeDamage(damage);
            DestroyNetworkObject();
        }
        else if(col.gameObject.tag == "Monitor")
        {
            CheckBullet();
            MonitorHealth monitorHealth = GameObject.Find("Monitor").GetComponent<MonitorHealth>();
            monitorHealth.TakeDamage(damage, status);
            DestroyNetworkObject();
        }
    }

    private void CheckBullet()
    {
        if (gameObject.tag == "OxyBullet")
        {
            status = "O";
            damage = 0f;
            hitVFX = oxygenHit;

            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.explosion, transform);
            GameObject hit = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(hit, 2f);
            GameObject areaDamage = GameObject.Find("Oxy Damage Area(Clone)");
            Destroy(areaDamage);

            SpawnOxyDamageClientRpc(oxyDmg.Value);
        }
        else if (gameObject.tag == "Nitrogen")
        {
            status = "N";
            damage = nitroDmg.Value;
            hitVFX = nitrogenHit;

            GameObject hit = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(hit, 2f);
        }
        else if (gameObject.tag == "Carbon")
        {
            status = "C";
            damage = carbonDmg.Value;
            hitVFX = carbonHit;

            GameObject hit = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(hit, 2f);
        }
    }

    [ClientRpc]
    private void SpawnOxyDamageClientRpc(float dmg)
    {
        GameObject oxyArea = Instantiate(oxyAreaCollider, transform.position, Quaternion.identity);
        oxyArea.GetComponent<OxyAreaCollider>().setDamage(dmg,reactionDmg.Value,critReact.Value,devolved.Value);
    }

    private void DestroyNetworkObject()
    {
        if (networkObject.IsSpawned && IsServer)
        {
            networkObject.DontDestroyWithOwner = true;
            networkObject.Despawn();
        }
    }
}
