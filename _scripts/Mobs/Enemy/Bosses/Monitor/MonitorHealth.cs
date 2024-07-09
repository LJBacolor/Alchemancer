using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MonitorHealth : NetworkBehaviour
{
    [SerializeField] private EnemyScriptableObject enemySO;
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private Image healthBar;
    [SerializeField] private Canvas hpPopup;
    [SerializeField] private String enemyName;

    [Header("Item Drops")]
    [SerializeField] private GameObject coin;
    private NetworkVariable<float> defense = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [NonSerialized] public NetworkVariable<float> maxHealth = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [NonSerialized] public NetworkVariable<float> health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float minHealth = 0f;
    private Camera cam;
    private MonitorAI monitorAI;
    public bool isDead = false;

    private void Awake()
    {
        cam = Camera.main;
        monitorAI = GetComponent<MonitorAI>();
    }

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
        HealthLimiter();
        UpdateHealthBar();
        IsDead();
    }

    public void TakeDamage(float damage, string status)
    {
        if (IsServer)
        {
            TakeDamageServerRpc(damage,status);
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage, string status)
    {
        if (!isDead && health.Value > minHealth && status == monitorAI.damageType)
        {
            float finalDamage = damage * (1 - (defense.Value/300));
            health.Value -= finalDamage;
            
            HPpopClientRpc(finalDamage);

            monitorAI.testPhase(); 
        }
    }

    [ClientRpc]
    public void HPpopClientRpc(float damage)
    {
        Vector3 offset = new Vector3(1f, 2f, 0.1f);
        Canvas hpPop = Instantiate(hpPopup, transform.position + offset, Quaternion.identity);
        TMP_Text hpPopText = hpPop.GetComponentInChildren<TMP_Text>();
        hpPopText.text = damage.ToString();

        hpPop.transform.LookAt(hpPop.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        Vector3 sideOffset = new Vector3(.5f, 0f, 0f);
        Rigidbody hpPopRb = hpPop.gameObject.GetComponent<Rigidbody>();
        hpPopRb.AddForce((transform.up + sideOffset) * 150f);

        Destroy(hpPop, 0.5f);
    }

    public void Heal(float amount)
    {
        health.Value += amount;
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
            isDead = true;

            if (IsHost)
            {
                if(NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("Outro",LoadSceneMode.Single);
                }
                else
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
                }
            }
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
