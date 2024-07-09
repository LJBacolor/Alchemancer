using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Cinemachine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class PlayerHealth : NetworkBehaviour
{
    public static PlayerHealth Instance;

    [SerializeField] private EnemyScriptableObject enemySO;

    [Header("Health Properties")]
    [NonSerialized] public NetworkVariable<float> health = new NetworkVariable<float>(default,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [NonSerialized] public bool canHeal;
    [SerializeField] public GameObject healthCanvas;
    [SerializeField] public GameObject damageIndicator;
    [SerializeField] public GameObject acidDmgIndicator;

    [SerializeField] private GameObject vikHP;
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject xtraLifePanel;
    [SerializeField] private GameObject[] xtraLifeUI;
    private float minHealth = 0f;
    private bool onAcid = false;

    [Header("Player Properties")]
    [SerializeField] private TextMeshProUGUI goldAmtText;
    [SerializeField] private PlayerShield playerShield;
    private CharacterController characterController;
    
    private float playerSpeed;
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool isDeadProcessed = false;
    public bool isRespawning = false;
    public GameObject cam;

    private void Awake()
    {
        if (!IsOwner) return;

        Instance = this;

        characterController = GetComponent<CharacterController>();
        playerSpeed = GetComponent<PlayerController>().playerSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            healthCanvas.SetActive(false);
        }

        if(!IsHost)
        {
            vikHP.SetActive(false);
        }
    }

    private void Start()
    {
        if (!IsOwner) return;

        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

        Scene currentscene = SceneManager.GetActiveScene();
        if(currentscene.name == "Lobby")
        {
            PlayerStats.currentHealth = PlayerStats.maxHealth;
            health.Value = PlayerStats.currentHealth;

            PlayerStats.currentXtraLife = PlayerStats.maxXtraLife;
        }

        onAcid = false;
        StopCoroutine(AcidDamage());  
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (IsOwner)
        {
            Heal(PlayerStats.healBuff);

            if(sceneName == "Lobby")
            {
                isDead.Value = false;
                isDeadProcessed = false;
            }
        }

        onAcid = false;
        StopCoroutine(AcidDamage());  
    }

    private void Update()
    {
        if (!IsOwner) return;

        HealthLimiter();
        UpdateHealthBar();
        UpdateXtraLife();
        CanHeal();

        CoinAmount();

        if(health.Value <= 0 && !isDeadProcessed)
        {
            IsDead();
            isDeadProcessed = true;

            if (PlayerStats.currentXtraLife > 0)
            {
                isRespawning = true;
                TakeLifeServerRpc();
                
                GameObject reviveScreen = GameObject.Find("Revive Screen");
                Animation reviveoutAnim = reviveScreen.GetComponent<Animation>();
                reviveoutAnim.Play();
                Invoke("RespawnPlayer", 1f);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeLifeServerRpc()
    {
        TakeLifeClientRpc();
    }

    [ClientRpc]
    private void TakeLifeClientRpc()
    {
        PlayerStats.currentXtraLife--;
    }

    public void TakeDamage(float damage)
    {
        if (!IsOwner) return;

        if(PlayerInventory.hasShield && playerShield.canShield)
        {
            StartCoroutine(playerShield.ActivateShield());
        }
        else
        {
            if(health.Value > minHealth)
            {
                if(!onAcid) SFXManager.Instance.PlaySFXClip(SFXManager.Instance.hurt, transform);

                float finalDamage = damage * (1 - (PlayerStats.defense/300));
                
                if(PlayerBuffs.Justkeepgroovin && PlayerController.Instance.isMoving)
                {
                    float damageReduction = finalDamage * 0.5f;
                    health.Value -= finalDamage - damageReduction;
                }
                else
                {
                    health.Value -= finalDamage;
                }
                
                PlayerStats.currentHealth = health.Value;
                damageIndicator.SetActive(true);
                Invoke("DelayDmgIndicator", 0.2f);
            }
        }
        
    }

    private void DelayDmgIndicator()
    {
        damageIndicator.SetActive(false);
    }

    public void Heal(float heal)
    {
        health.Value += heal;
        PlayerStats.currentHealth = health.Value;
    }

    private void CanHeal()
    {
        if(health.Value > minHealth && health.Value < PlayerStats.maxHealth)
        {
            canHeal = true;
        }
        else
        {
            canHeal = false;
        }
    }

    private void UpdateHealthBar()
    {
        Scene currentscene = SceneManager.GetActiveScene();
        if(currentscene.name == "Lobby")
        {
            PlayerStats.currentHealth = PlayerStats.maxHealth;
            health.Value = PlayerStats.currentHealth;
        }

        healthBar.fillAmount = health.Value / PlayerStats.maxHealth;

        int healthIntValue = Mathf.RoundToInt(health.Value);
        healthText.text = healthIntValue.ToString();
    }

    private void UpdateXtraLife()
    {
        Scene currentscene = SceneManager.GetActiveScene();
        if(currentscene.name == "Lobby")
        {
            PlayerStats.currentXtraLife = PlayerStats.maxXtraLife;
        }

        if(PlayerStats.currentXtraLife > 0)
        {
            xtraLifePanel.SetActive(true);

            for (int i = 0; i < xtraLifeUI.Length; i++)
            {
                xtraLifeUI[i].SetActive(i < PlayerStats.currentXtraLife);
            }
        }
        else
        {
            xtraLifePanel.SetActive(false);
        }
    }

    private void HealthLimiter()
    {
        health.Value = Mathf.Clamp(health.Value, minHealth, PlayerStats.maxHealth);
    }

    private void CoinAmount()
    {
        goldAmtText.text = PlayerInventory.goldAmount.ToString();
        PlayerInventory.goldAmount = Mathf.Clamp(PlayerInventory.goldAmount, 0, 99999);
    }

    private void IsDead()
    {
        DataManager.Instance.SaveGame();
        DisablePlayer();
        GetComponent<PlayerController>().anim.SetTrigger("isDeath");
        isDead.Value = true;
    }

    public void RespawnPlayer()
    {
        GameObject playerSpawn = GameObject.Find("PlayerSpawn");
        Debug.Log(playerSpawn.transform.position);
        transform.position = playerSpawn.transform.position;

        EnablePlayer();
        health.Value = PlayerStats.maxHealth;
        isDead.Value = false;
        isDeadProcessed = false;
        PlayerStats.maxHealth = health.Value;
        
        isRespawning = false;
    }

    public void DisablePlayer()
    {
        //characterController.enabled = false;
        //GetComponent<PlayerController>().playerSpeed = 0f;
        GetComponent<PlayerController>().canMove = false;
        GetComponent<PlayerController>().canDash = false;
        GetComponent<PlayerEquips>().enabled = false;
        GetComponent<PlayerAttack>().enabled = false;
        GetComponent<IsoAim>().enabled = false;
    }

    public void EnablePlayer()
    {
        //characterController.enabled = true;
        //GetComponent<PlayerController>().playerSpeed = playerSpeed;
        GetComponent<PlayerController>().canMove = true;
        GetComponent<PlayerController>().canDash = true;
        GetComponent<PlayerEquips>().enabled = true;
        GetComponent<PlayerAttack>().enabled = true;
        GetComponent<IsoAim>().enabled = true;

        // GameObject lightoutScreen = GameObject.Find("Lightout Screen");
        // Animation lightoutAnim = lightoutScreen.GetComponent<Animation>();
        // lightoutAnim.Play();
    }

    private IEnumerator AcidDamage()
    {
        while(onAcid)
        {
            acidDmgIndicator.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            acidDmgIndicator.SetActive(false);
            yield return new WaitForSeconds(0.2f); 
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if(col.gameObject.tag == "LobbyBox" && IsOwner)
        {
            PlayerStats.currentHealth = PlayerStats.maxHealth;
            health.Value = PlayerStats.currentHealth;
        }
        
        if(col.gameObject.tag == "Acid")
        {
            TakeDamage(0.5f);
            if(!onAcid)
            {
                onAcid = true;
                StartCoroutine(AcidDamage());  
            }
        }
        if(col.gameObject.tag == "Toxic")
        {
            TakeDamage(0.3f);
        }
        if (col.gameObject.tag == "Laser")
        {
            TakeDamage(8f);
        }
        if (col.gameObject.tag == "HealArea")
        {
            Heal(0.1f);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Acid")
        {
            onAcid = false;
            StopCoroutine(AcidDamage());  
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        //Enemies
        if(col.gameObject.tag == "Dash")
        {
            EnemyStatus enemyStatus = col.GetComponentInParent<EnemyStatus>();
            if(enemyStatus.imInsect)
            {
                int elementIndex = FindEnemyIndexByName("Insect Dash");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else if(enemyStatus.imRobot)
            {
                int elementIndex = FindEnemyIndexByName("Robot Dash");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else
            {
                int elementIndex = FindEnemyIndexByName("Enemy Dash");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
        }
        if(col.gameObject.tag == "Jump")
        {
            EnemyStatus enemyStatus = col.GetComponentInParent<EnemyStatus>();
            if(enemyStatus.imInsect)
            {
                int elementIndex = FindEnemyIndexByName("Insect Jump");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else
            {
                int elementIndex = FindEnemyIndexByName("Enemy Jump");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
        }
        if(col.gameObject.tag == "Melee")
        {
            Debug.Log("MELEE HIT");
            EnemyStatus enemyStatus = col.GetComponentInParent<EnemyStatus>();
            if(enemyStatus.imPlant)
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.plantMeleeAttack, transform);
                int elementIndex = FindEnemyIndexByName("Plant Melee");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else if(enemyStatus.imRobot)
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.robotMeleeAttack, transform);
                int elementIndex = FindEnemyIndexByName("Robot Melee");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else
            {
                int elementIndex = FindEnemyIndexByName("Enemy Jump");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
        }
        if(col.gameObject.tag == "EnemyBullet")
        {
            RangeBullet rangeBullet = col.gameObject.GetComponent<RangeBullet>();
            if(rangeBullet.imInsect)
            {
                int elementIndex = FindEnemyIndexByName("Insect Range");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else if(rangeBullet.imRobot)
            {
                int elementIndex = FindEnemyIndexByName("Robot Range");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else
            {
                int elementIndex = FindEnemyIndexByName("Enemy Range");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
        }
        if(col.gameObject.tag == "EnemyLaser")
        {
            int elementIndex = FindEnemyIndexByName("Robot Range");
            TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
        }
        if(col.gameObject.tag == "CannonAreaCollider")
        {
            AreaCollider areaCollider = col.gameObject.GetComponent<AreaCollider>();
            if(areaCollider.imPlant)
            {
                int elementIndex = FindEnemyIndexByName("Plant Cannon");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else
            {
                int elementIndex = FindEnemyIndexByName("Enemy Cannon");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
        }
        if(col.gameObject.tag == "Spike")
        {
            MageSpike mageSpike = col.gameObject.GetComponent<MageSpike>();
            if(mageSpike.imPlant)
            {
                int elementIndex = FindEnemyIndexByName("Plant Mage");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
            else
            {
                int elementIndex = FindEnemyIndexByName("Enemy Mage");
                TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
            }
        }

        //Bosses

        //Worm
        if(col.gameObject.tag == "Worm")
        {
            int elementIndex = FindEnemyIndexByName("Worm");
            TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
        }

        //Daisy
        if(col.gameObject.tag == "DaisySurround")
        {
            int elementIndex = FindEnemyIndexByName("Daisy");
            TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
        }
        if(col.gameObject.tag == "DaisyInstant")
        {
            int elementIndex = FindEnemyIndexByName("Daisy");
            TakeDamage(GetEnemyDamageByIndex(elementIndex, 1));
        }
        if(col.gameObject.tag == "DaisyAreaCollider")
        {
            int elementIndex = FindEnemyIndexByName("Daisy");
            TakeDamage(GetEnemyDamageByIndex(elementIndex, 2));
        }

        //Monitor
        if (col.gameObject.tag == "FallingObjects")
        {
            int elementIndex = FindEnemyIndexByName("Monitor");
            TakeDamage(GetEnemyDamageByIndex(elementIndex, 0));
        }

        //Reactions
        if(col.gameObject.tag == "CH4")
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = UnityEngine.Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            TakeDamage(damage / 5);
        }
        if(col.gameObject.tag == "CN" && !PlayerBuffs.Selfburn)
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = UnityEngine.Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            TakeDamage(damage / 5);
        }

        //Items
        if(col.gameObject.tag == "Coin")
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.coin, transform);

            Destroy(col.gameObject);
            PlayerInventory.goldAmount += PlayerInventory.goldValue;
        }

        if(col.gameObject.tag == "Vine")
        {
            Debug.Log("HIT VINE");
            TakeDamage(30f);
        }
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

    public float GetEnemyDamageByIndex(int index, int dmg)
    {
        if (index != -1)
        {
            return enemySO.enemies[index].damage[dmg];
        }
        else
        {
            Debug.LogWarning("Enemy with name " + gameObject.name + " not found.");
            return 0f; // or some default value
        }
    }
}
