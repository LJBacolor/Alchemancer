using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyStatus : NetworkBehaviour
{
    [Header("Statuses")]
    [SerializeField] private GameObject statusCanvas;
    [SerializeField] private GameObject oxygenStatus;
    [SerializeField] private GameObject hydrogenStatus;
    [SerializeField] private GameObject nitrogenStatus;
    [SerializeField] private GameObject carbonStatus;
    [SerializeField] private GameObject poisonStatus;

    [SerializeField] private float statusCooldown = 3;

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

    private EnemyHealth enemyHealth;
    public GameObject cam;
    private float cooldownTimer;

    private float dotCooldown = 0.5f;
    private float nextDamageTime;

    private bool hasOxygen = false;
    private bool hasHydrogen = false;
    private bool hasNitrogen = false;
    private bool hasCarbon = false;

    private float latestReactDmg = 0f;
    private bool critReact = false;
    private bool devolved = false;

    public override void OnNetworkSpawn()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        cooldownTimer = statusCooldown;

        base.OnNetworkSpawn();
    }

    private void Update() 
    {
        if (cam)
        {
            CheckStatus();
        }
        else
        {
            cam = GameObject.Find("Main Camera");
        }

        CheckReaction();
    }

    public void setReactionDmg(float reactDmg,bool critR,bool dev)
    {
        latestReactDmg = reactDmg;
        critReact = critR;
        devolved = dev;
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
            if (hasOxygen)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if (tag == "Hydrogen")
        {
            hasHydrogen = true;
            if (hasHydrogen)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if (tag == "Nitrogen")
        {
            hasNitrogen = true;
            if (hasNitrogen)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if (tag == "Carbon")
        {
            hasCarbon = true;
            if (hasCarbon)
            {
                cooldownTimer = statusCooldown;
            }
        }
        else if (tag == "H2O")
        {
            //Damage
            float damage = latestReactDmg;
            if (critReact)
            {
                int num = Random.Range(1, 21);
                if (num <= 5) damage = latestReactDmg * 2;
            }

            if (imInsect)
            {
                enemyHealth.TakeDamage(damage);
            }
            else if (imRobot)
            {
                enemyHealth.TakeDamage(damage * 2);
            }
        }
        else if (tag == "CH4")
        {
            //Damage
            float damage = latestReactDmg;
            if (critReact)
            {
                int num = Random.Range(1, 21);
                if (num <= 5) damage = latestReactDmg * 2;
            }

            if (imInsect)
            {
                enemyHealth.TakeDamage(damage * 2);
            }
            else if (imPlant)
            {
                enemyHealth.TakeDamage(damage);
                StartCoroutine(ApplyDOT(5f, dotCooldown, 2));
            }
            else if (imRobot)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
        else if (tag == "CN")
        {
            //Damage
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = latestReactDmg * 2;
            }
            
            enemyHealth.TakeDamage(damage);
        }
    }

    private void OnTriggerStay(Collider col) 
    {
        if (Time.time >= nextDamageTime) 
        {
            if(col.tag == "NO")
            {
                enemyHealth.TakeDamage(latestReactDmg * 0.2f);
            }

            if(col.gameObject.tag == "Toxic")
            {
                enemyHealth.TakeDamage(10f);
            }
            nextDamageTime = Time.time + dotCooldown; // Set the next allowed damage time
        }
    }

    private void CheckStatus()
    {
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
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = latestReactDmg * 2;
            }

            hasOxygen = false;
            hasCarbon = false;
            // CO2/Heat - Single target (overheats the bots/ heals plants)
            // bot = damage 2x
            // plant = heal, spawn area oxygen and add status
            Debug.Log("CO2/Heat");

            if(devolved)
            {
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                if(imInsect)
                {
                    enemyHealth.TakeDamage(damage);
                }
                else if(imPlant)
                {
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.CO2, transform);
                    Instantiate(areaOxygen, transform.position, Quaternion.identity);
                    enemyHealth.Heal(damage);
                }
                else if(imRobot)
                {
                    enemyHealth.TakeDamage(damage * 2);
                }
            }
        }

        if(hasHydrogen && hasOxygen)
        {
            //Damage
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = latestReactDmg * 2;
            }

            hasHydrogen = false;
            hasOxygen = false;
            // H2O/Water - AOE Explosion of Water (stuns bots by shortcircuit / does nothing to plants)
            // bot = 5sec stun w/ small damage
            // plants = immune
            Debug.Log("H2O/Water");

            if(devolved)
            {
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.H2O, transform);
                if(imInsect)
                {
                    Instantiate(H2O, transform.position, Quaternion.identity);
                }
                else if(imPlant)
                {
                    Instantiate(H2O, transform.position, Quaternion.identity);
                }
                else if(imRobot)
                {
                    Instantiate(H2O, transform.position, Quaternion.identity);
                }
            }
        }

        if(hasNitrogen && hasOxygen)
        {
            //Damage
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = latestReactDmg * 2;
            }

            hasNitrogen = false;
            hasOxygen = false;
            // NO/Debuff - AOE Nitric Oxide Cloud (decreases defense of both bots and plants)
            // 5sec leave circle small dot and debuff
            Debug.Log("NO/Debuff");

            if(devolved)
            {
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NO, transform);
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
                    Instantiate(NO, transform.position, Quaternion.identity);
                }
            }
        }

        if(hasNitrogen && hasHydrogen)
        {
            //Damage
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = 20;
            }

            hasNitrogen = false;
            hasHydrogen = false;
            // NH3/Poison - Single target poison (does nothing on bots/ poisons plants)
            // bot = immune
            // plants = 5sec dot 
            Debug.Log("NH3/Poison");

            if(devolved)
            {
                //Damage
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                if(imInsect)
                {
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NH3, transform);
                    StartCoroutine(ApplyDOT(5f, dotCooldown, damage * 0.1f));
                }
                else if(imPlant)
                {
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NH3, transform);
                    StartCoroutine(ApplyDOT(5f, dotCooldown, damage * 0.2f));
                }
                else if(imRobot)
                {
                    
                }
            }
        }

        if(hasCarbon && hasHydrogen)
        {
            //Damage
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = latestReactDmg * 2;
            }

            hasCarbon = false;
            hasHydrogen = false;
            // CH4/Explosive - AOE Explosive (good for both bots and plants)
            // bots = raw dmg
            // plants = raw dmg, 5sec dot afterwards
            Debug.Log("CH4/Explosive");

            if(devolved)
            {
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.CH4, transform);
                if(imInsect)
                {
                    Instantiate(CH4, transform.position, Quaternion.identity);
                }
                else if(imPlant)
                {
                    Instantiate(CH4, transform.position, Quaternion.identity);
                    StartCoroutine(ApplyDOT(5f, dotCooldown, 2));
                }
                else if(imRobot)
                {
                    Instantiate(CH4, transform.position, Quaternion.identity);
                }
            }
        }

        if(hasCarbon && hasNitrogen)
        {
            //Damage
            float damage = latestReactDmg;
            if(critReact)
            {
                int num = Random.Range(1, 21);
                if(num <= 5) damage = latestReactDmg * 2;
            }

            hasCarbon = false;
            hasNitrogen = false;
            // CN-/Cyanide Cores > applying hydrogen detonates cores AOE burst of Hydrogen Cyanide gas 
            // (does damage to everything)
            Debug.Log("CN/Gas");

            if(devolved)
            {
                enemyHealth.TakeDamage(damage);
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
                    Instantiate(cyanideCore, transform.position, Quaternion.identity);
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
            enemyHealth.TakeDamage(damagePerInterval);
            yield return new WaitForSeconds(damageInterval);
        }
        poisonStatus.SetActive(false);
    }
}