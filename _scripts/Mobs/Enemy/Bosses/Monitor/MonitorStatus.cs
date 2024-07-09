using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MonitorStatus : NetworkBehaviour
{
    [Header("Statuses")]
    [SerializeField] private GameObject statusCanvas;
    [SerializeField] private GameObject oxygenStatus;
    [SerializeField] private GameObject hydrogenStatus;
    [SerializeField] private GameObject nitrogenStatus;
    [SerializeField] private GameObject carbonStatus;
    [SerializeField] private GameObject poisonStatus;

    private float statusCooldown;

    [Header("Enemy types")]
    [SerializeField] public bool imInsect;
    [SerializeField] public bool imPlant;
    [SerializeField] public bool imRobot;

    [Header("Reaction Prefabs")]
    [SerializeField] private GameObject areaOxygen;
    [SerializeField] private GameObject H2O;
    [SerializeField] private GameObject NO;
    [SerializeField] private GameObject CH4;
    [SerializeField] private GameObject cyanideCore;
    [SerializeField] private GameObject CN;

    private MonitorHealth enemyHealth;
    private Camera cam;
    private float cooldownTimer;

    private float dotCooldown = 0.5f;
    private float nextDamageTime;

    private string status;
    private bool hasOxygen, hasHydrogen, hasNitrogen, hasCarbon;
    private bool hasCO2, hasH2O, hasNO, hasNH3, hasCH4, hasCN;

    public override void OnNetworkSpawn()
    {
        enemyHealth = GetComponent<MonitorHealth>();
        cooldownTimer = statusCooldown;
        cam = Camera.main;
    }

    private void Update() 
    {
        CheckStatus();
        CheckReaction();
    }
    private void OnTriggerEnter(Collider col)
    {
        ApplyElementServerRpc(col.tag);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyElementServerRpc(string tag)
    {
        ApplyElementClientRpc(tag);
    }

    [ClientRpc]
    private void ApplyElementClientRpc(string tag)
    {
        if (tag == "Oxygen")
        {
            hasOxygen = true;
            if(hasOxygen)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if(tag == "Hydrogen")
        {
            hasHydrogen = true;
            if(hasHydrogen)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if(tag == "Nitrogen")
        {
            hasNitrogen = true;
            if(hasNitrogen)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if(tag == "Carbon")
        {
            hasCarbon = true;
            if(hasCarbon)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if(tag == "CN")
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            enemyHealth.TakeDamage(damage, "CN");
        }
    }

    private void OnTriggerStay(Collider col) 
    {
        if (Time.time >= nextDamageTime) 
        {
            if(col.tag == "NO")
            {
                //Damage
                float damage = 10;
                if(PlayerBuffs.Criteactions)
                {
                    int num = Random.Range(1, 21);
                    if(num <= 5) damage = 20;
                }

                enemyHealth.TakeDamage(damage, "NO");
            }

            if(col.gameObject.tag == "Toxic")
            {
                enemyHealth.TakeDamage(10f, "");
            }
            nextDamageTime = Time.time + dotCooldown; // Set the next allowed damage time
        }
    }

    private void CheckStatus()
    {
        statusCooldown = PlayerStats.elementStatusDuration;
        statusCanvas.transform.LookAt(statusCanvas.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        
        if(hasOxygen)
        {
            oxygenStatus.SetActive(true);
            hasOxygen = StatusCooldown();
        }
        else
        {
            oxygenStatus.SetActive(false);
        }

        if(hasHydrogen)
        {
            hydrogenStatus.SetActive(true);
            hasHydrogen = StatusCooldown();
        }
        else
        {
            hydrogenStatus.SetActive(false);
        }

        if(hasNitrogen)
        {
            nitrogenStatus.SetActive(true);
            hasNitrogen = StatusCooldown();
        }
        else
        {
            nitrogenStatus.SetActive(false);
        }

        if(hasCarbon)
        {
            carbonStatus.SetActive(true);
            hasCarbon = StatusCooldown();
        }
        else
        {
            carbonStatus.SetActive(false);
        }
    }

    private void CheckReaction()
    {
        if(hasOxygen && hasCarbon)
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            hasOxygen = false;
            hasCarbon = false;
            hasCO2 = true;
            // CO2/Heat - Single target (overheats the bots/ heals plants)
            // bot = damage 2x
            // plant = heal, spawn area oxygen and add status
            Debug.Log("CO2/Heat");

            if(PlayerBuffs.Devolved)
            {
                enemyHealth.TakeDamage(damage, "CO2");
            }
            else
            {
                if(imInsect)
                {
                    enemyHealth.TakeDamage(damage, "CO2");
                }
                else if(imPlant)
                {
                    Instantiate(areaOxygen, transform.position, Quaternion.identity);
                    enemyHealth.Heal(damage);
                }
                else if(imRobot)
                {
                    enemyHealth.TakeDamage(damage * 2, "CO2");
                }
            }
            
        }

        if(hasHydrogen && hasOxygen)
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            hasHydrogen = false;
            hasOxygen = false;
            hasH2O = true;
            // H2O/Water - AOE Explosion of Water (stuns bots by shortcircuit / does nothing to plants)
            // bot = 5sec stun w/ small damage
            // plants = immune
            Debug.Log("H2O/Water");

            if(PlayerBuffs.Devolved)
            {
                enemyHealth.TakeDamage(damage, "HO2");
            }
            else
            {
                if(imInsect)
                {
                    Instantiate(H2O, transform.position, Quaternion.identity);
                    enemyHealth.TakeDamage(damage, "HO2");
                }
                else if(imPlant)
                {
                    Instantiate(H2O, transform.position, Quaternion.identity);
                }
                else if(imRobot)
                {
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.H2O, transform);
                    Instantiate(H2O, transform.position, Quaternion.identity);
                    enemyHealth.TakeDamage(damage / 2, "HO2");
                }
            }
            
        }

        if(hasNitrogen && hasOxygen)
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            hasNitrogen = false;
            hasOxygen = false;
            hasNO = true;
            // NO/Debuff - AOE Nitric Oxide Cloud (decreases defense of both bots and plants)
            // 5sec leave circle small dot and debuff
            Debug.Log("NO/Debuff");

            if(PlayerBuffs.Devolved)
            {
                enemyHealth.TakeDamage(damage, "NO");
            }
            else
            {
                if(imInsect)
                {
                    Instantiate(NO, transform.position, Quaternion.identity);
                }
                else if(imPlant)
                {
                    Instantiate(NO, transform.position, Quaternion.identity);
                }
                else if(imRobot)
                {
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NO, transform);
                    Instantiate(NO, transform.position, Quaternion.identity);
                }
            }
            
        }

        if(hasNitrogen && hasHydrogen)
        {
            //Damage
            float damage = 10;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = 20;
            }

            hasNitrogen = false;
            hasHydrogen = false;
            hasNH3 = true;
            // NH3/Poison - Single target poison (does nothing on bots/ poisons plants)
            // bot = immune
            // plants = 5sec dot 
            Debug.Log("NH3/Poison");

            if(PlayerBuffs.Devolved)
            {
                //Damage
                float reacDamage = PlayerStats.reactionDamage;
                if(PlayerBuffs.Criteactions)
                {
                    int num = Random.Range(1, 21);
                    if(num <= 5) reacDamage = PlayerStats.reactionDamage * 2;
                }
                enemyHealth.TakeDamage(reacDamage, "NH#");
            }
            else
            {
                if(imInsect)
                {
                    StartCoroutine(ApplyDOT(5f, dotCooldown, damage));
                }
                else if(imPlant)
                {
                    StartCoroutine(ApplyDOT(5f, dotCooldown, damage));
                }
                else if(imRobot)
                {
                    
                }
            }
            
        }

        if(hasCarbon && hasHydrogen)
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            hasCarbon = false;
            hasHydrogen = false;
            hasCH4 = true;
            // CH4/Explosive - AOE Explosive (good for both bots and plants)
            // bots = raw dmg
            // plants = raw dmg, 5sec dot afterwards
            Debug.Log("CH4/Explosive");

            if(PlayerBuffs.Devolved)
            {
                enemyHealth.TakeDamage(damage, "CH4");
            }
            else
            {
                if(imInsect)
                {
                    Instantiate(CH4, transform.position, Quaternion.identity);
                    enemyHealth.TakeDamage(damage, "CH4");
                }
                else if(imPlant)
                {
                    Instantiate(CH4, transform.position, Quaternion.identity);
                    enemyHealth.TakeDamage(damage, "CH4");
                    StartCoroutine(ApplyDOT(5f, dotCooldown, 2));
                }
                else if(imRobot)
                {
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.CH4, transform);
                    Instantiate(CH4, transform.position, Quaternion.identity);
                    enemyHealth.TakeDamage(damage, "CH4");
                }
            }
           
        }

        if(hasCarbon && hasNitrogen)
        {
            //Damage
            float damage = PlayerStats.reactionDamage;
            if(PlayerBuffs.Criteactions)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = PlayerStats.reactionDamage * 2;
            }

            hasCarbon = false;
            hasNitrogen = false;
            hasCN = true;
            // CN-/Cyanide Cores > applying hydrogen detonates cores AOE burst of Hydrogen Cyanide gas 
            // (does damage to everything)
            Debug.Log("CN/Gas");

            if(PlayerBuffs.Devolved)
            {
                enemyHealth.TakeDamage(damage, "CN");
            }
            else
            {
                if(imInsect)
                {
                    Instantiate(cyanideCore, transform.position, Quaternion.identity);
                }
                else if(imPlant)
                {
                    Instantiate(cyanideCore, transform.position, Quaternion.identity);
                }
                else if(imRobot)
                {
                    Vector3 newPos = new Vector3(16.5f, 0.5f, 13.5f);
                    Instantiate(cyanideCore, newPos, Quaternion.identity);
                }
            }
            
        }
    }

    private bool StatusCooldown()
    {
        cooldownTimer -= Time.deltaTime;
        if(cooldownTimer <= 0)
        {
            cooldownTimer = statusCooldown;
            return false;
        }
        return true;
    }

    private IEnumerator ApplyDOT(float duration, float damageInterval, float damagePerInterval)
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            poisonStatus.SetActive(true);
            enemyHealth.TakeDamage(damagePerInterval, "NH3");
            yield return new WaitForSeconds(damageInterval);
        }
        poisonStatus.SetActive(false);
    }
}
