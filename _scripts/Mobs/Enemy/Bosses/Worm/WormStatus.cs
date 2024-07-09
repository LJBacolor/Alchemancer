using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WormStatus : MonoBehaviour
{
    [SerializeField] private GameObject statusCanvas;
    [SerializeField] private GameObject oxygenStatus;
    [SerializeField] private GameObject hydrogenStatus;
    [SerializeField] private GameObject nitrogenStatus;
    [SerializeField] private GameObject carbonStatus;

    [SerializeField] private float statusCooldown = 3;

    private EnemyHealth enemyHealth;
    private GameObject cam;
    private float cooldownTimer;

    public bool hasOxygen = false;
    public bool hasHydrogen = false;
    public bool hasNitrogen = false;
    public bool hasCarbon = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        cooldownTimer = statusCooldown;
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

    private void OnTriggerEnter(Collider col) 
    {
        ApplyElementServerRpc(col.tag);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyElementServerRpc(string tag)
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
            hasOxygen = false;
            hasCarbon = false;
            // CO2/Heat - Single target (overheats the bots/ heals plants)
            Debug.Log("CO2/Heat");
            enemyHealth.TakeDamage(PlayerStats.reactionDamage * 2);
        }

        if(hasHydrogen && hasOxygen)
        {
            hasHydrogen = false;
            hasOxygen = false;
            // H2O/Water - AOE Explosion of Water (stuns bots by shortcircuit / does nothing to plants)
            Debug.Log("H2O/Water");
            enemyHealth.TakeDamage(PlayerStats.reactionDamage * 2);
        }

        if(hasNitrogen && hasOxygen)
        {
            hasNitrogen = false;
            hasOxygen = false;
            // NO/Debuff - AOE Nitric Oxide Cloud (decreases defense of both bots and plants)
            Debug.Log("NO/Debuff");
            enemyHealth.TakeDamage(PlayerStats.reactionDamage * 2);
        }

        if(hasNitrogen && hasHydrogen)
        {
            hasNitrogen = false;
            hasHydrogen = false;
            // NH3/Poison - Single target poison (does nothing on bots/ poisons plants)
            Debug.Log("NH3/Poison");
            enemyHealth.TakeDamage(PlayerStats.reactionDamage * 2);
        }

        if(hasCarbon && hasHydrogen)
        {
            hasCarbon = false;
            hasHydrogen = false;
            // CH4/Explosive - AOE Explosive (good for both bots and plants)
            Debug.Log("CH4/Explosive");
            enemyHealth.TakeDamage(PlayerStats.reactionDamage * 2);
        }

        if(hasCarbon && hasNitrogen)
        {
            hasCarbon = false;
            hasNitrogen = false;
            // CN/Gas - applying Cyanide gas (does damage to everything but drains the player's hp)
            Debug.Log("CN/Gas");
            enemyHealth.TakeDamage(PlayerStats.reactionDamage * 2);
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
}