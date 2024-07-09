using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private EnemyScriptableObject enemySO;
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private Image healthBar;
    [SerializeField] private Canvas hpPopup;
    [SerializeField] private String enemyName;
    [SerializeField] private GameObject buffBox;

    [Header("Item Drops")]
    [SerializeField] private GameObject coin;
    private NetworkVariable<float> defense = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [NonSerialized] public NetworkVariable<float> maxHealth = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [NonSerialized] public NetworkVariable<float> health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float minHealth = 0f;
    public GameObject cam;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        AssignHealth();
        AssignDefense();
        health.Value = maxHealth.Value;

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (cam)
        {
            UpdateHealthBar();
        }
        else
        {
            cam = GameObject.Find("Main Camera");
        }

        if (!IsServer) return;
        HealthLimiter();
        IsDead();
    }

    public void TakeDamage(float damage)
    {
        if (IsServer)
            TakeDamageServerRpc(damage);
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        if (health.Value > minHealth)
        {
            float finalDamage = damage * (1 - (defense.Value/300));
            health.Value -= finalDamage;

            if (damage > 0)
            {
                if(enemyName == "Daisy")
                {
                    if(!GetComponent<DaisyAI>().onShield) HPpopClientRpc(damage);
                }
                else HPpopClientRpc(damage);
            }

            if(enemyName == "Worm")
            {
                WormAI wormAI = GetComponent<WormAI>();
                wormAI.testPhase(); 
            }

            if(enemyName == "Daisy")
            {
                DaisyAI daisyAI = GetComponent<DaisyAI>();
                daisyAI.testPhase();
            }
        }
    }

    [ClientRpc]
    public void HPpopClientRpc(float damage)
    {
        Vector3 offset = new Vector3(1f, 2f, 0.1f);
        Canvas hpPop = Instantiate(hpPopup, transform.position + offset, Quaternion.identity);
        TMP_Text hpPopText = hpPop.GetComponentInChildren<TMP_Text>();
        int dmg = Mathf.RoundToInt(damage);
        hpPopText.text = dmg.ToString();

        hpPop.transform.LookAt(hpPop.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        Vector3 sideOffset = new Vector3(.5f, 0f, 0f);
        Rigidbody hpPopRb = hpPop.gameObject.GetComponent<Rigidbody>();
        hpPopRb.AddForce((transform.up + sideOffset) * 150f);

        Destroy(hpPop, 0.5f);
    }

    public void Heal(float amount)
    {
        if (IsServer)
        {
            health.Value += amount;
        }
    }

    public void UpdateHealthBar()
    {
        healthCanvas.transform.LookAt(healthCanvas.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        healthBar.fillAmount = health.Value / maxHealth.Value;
    }

    private void HealthLimiter()
    {
        health.Value = Mathf.Clamp(health.Value, minHealth, maxHealth.Value);
    }

    private void IsDead()
    {
        if(health.Value <= 0)
        {
            Instantiate(coin, transform.position, Quaternion.identity);
            CheckEnemy();
        }
    }

    private void CheckEnemy()
    {
        if (name == "Worm")
        {
            DestroyWormClientRpc();
            OpenDoorClientRpc();
            ToggleEnterSectorClientRpc(true);
            SpawnChestClientRpc();
        }
        else if(name == "Daisy")
        {
            OpenDoorClientRpc();
            ToggleEnterSectorClientRpc(true);
            SpawnChestClientRpc();
        }

        //MadChemist buff //Killing an Enemy has a 25% chance to heal the player based on 2% of their Max HP
        if(PlayerBuffs.MadChemist)
        {
            int num = UnityEngine.Random.Range(1,21);
            if(num <= 5)    NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().Heal(PlayerStats.maxHealth * 0.02f);
        }

        NetworkObject.Despawn();
    }

    [ClientRpc]
    private void DestroyWormClientRpc()
    {
        GameObject[] wormsObj = GameObject.FindGameObjectsWithTag("Worm");
        foreach (GameObject worm in wormsObj)
        {
            Destroy(worm);
        }
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        GameObject sectorDoor = GameObject.Find("Door");
        sectorDoor.GetComponent<Door>().canEnter = true;
    }

    [ClientRpc]
    private void ToggleEnterSectorClientRpc(bool open)
    {
        GameObject sectorDoor = GameObject.Find("Enter Sector");
        sectorDoor.GetComponent<BoxCollider>().enabled = open;
    }

    [ClientRpc]
    private void SpawnChestClientRpc()
    {
        RespawnDeadPlayer();

        Transform chestPos = GameObject.Find("Door").transform;
        Vector3 chestSpawn = new Vector3(chestPos.position.x - 4f, 0.6f, chestPos.position.z - 2f);

        Instantiate(buffBox, chestSpawn, Quaternion.identity);

        Vector3 chestSpawn2 = new Vector3(chestPos.position.x - 2f, 0.6f, chestPos.position.z - 4f);

        Instantiate(buffBox, chestSpawn2, Quaternion.identity);
    }

    private void RespawnDeadPlayer()
    {
        if (NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().isDead.Value)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().RespawnPlayer();
        }
    }

    private void AssignHealth()
    {
        int elementIndex = FindEnemyIndexByName(enemyName);
        maxHealth.Value = GetEnemyHealthByIndex(elementIndex);
        //GetEnemyDamageByIndex(elementIndex);
    }

    private void AssignDefense()
    {
        int elementIndex = FindEnemyIndexByName(enemyName);
        defense.Value = GetEnemyDefenseByIndex(elementIndex);
        //GetEnemyDamageByIndex(elementIndex);
    }

    public int FindEnemyIndexByName(string enemyName)
    {
        for (int i = 0; i < enemySO.enemies.Length; i++)
        {
            if (enemySO.enemies[i].Name == enemyName)
            {
                return i;
            }
        }

        // Return -1 if the enemy with the given name is not found
        return -1;
    }

    public float GetEnemyHealthByIndex(int index)
    {
        if (index != -1)
        {
            return enemySO.enemies[index].maxHealth;
        }
        else
        {
            Debug.LogWarning("Enemy with name " + gameObject.name + " not found.");
            return 0f;
        }
    }

    public float GetEnemyDefenseByIndex(int index)
    {
        if (index != -1)
        {
            return enemySO.enemies[index].defense;
        }
        else
        {
            Debug.LogWarning("Enemy with name " + gameObject.name + " not found.");
            return 0f;
        }
    }
}