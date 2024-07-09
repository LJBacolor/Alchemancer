using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public class MonitorAI : NetworkBehaviour
{
    [SerializeField] private MonitorPhase monitorPhase;
    [SerializeField] private TextMeshProUGUI[] monitorText;
    [SerializeField] private GameObject[] faceSprites;
    private float changeTypeCooldown = 10f;
    private float startTime = 0;
    private string[] damageTypes = 
    {"O", "H", "N", "C", "CO2", "H2O", "NO", "CH4", "CN"};
    //0    1    2    3     4      5     6      7      8     
    private MonitorHealth enemyHealth;
    private MonitorAttack monitorAttack;
    private Transform player;
    [NonSerialized] public bool onShield = false;
    [NonSerialized] public bool canAttack = false;
    [NonSerialized] public bool firstPhase = false;
    [NonSerialized] public bool secondPhase = false;
    [NonSerialized] public string damageType;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        enemyHealth = GetComponent<MonitorHealth>();
        monitorAttack = GetComponent<MonitorAttack>();
        ChooseDamageType();

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        Immunity();

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Monitor Attack
        if (!gameManager.deadTrigger && !onShield)
        {
            canAttack = true;
        }
        else
        {  
            canAttack = false;
        }

        //Monitor Damage Type
        if(onShield)
        {
            damageType = "XXX";
        }
        else
        {
            if(Time.time >= startTime + changeTypeCooldown)
            {
                ChooseDamageType();
            }
        }

        if(!GetComponent<MonitorHealth>().isDead) setDamageTypeClientRpc(damageType);

        //Harder Attacks
        if (firstPhase) 
        {
            faceSprites[1].SetActive(true);
            monitorAttack.fallingObjectsTimer = 2f;
            monitorAttack.rotateSpeed = 30f;
            monitorAttack.safeZoneCount = 2;
        } 
        else if(secondPhase) 
        {
            faceSprites[2].SetActive(true);
            monitorAttack.fallingObjectsTimer = 1f;
            monitorAttack.rotateSpeed = 35f;
            monitorAttack.safeZoneCount = 1;
        }

        if(GetComponent<MonitorHealth>().isDead)
        {
            faceSprites[4].SetActive(true);
            foreach(var monitorTexts in monitorText)
            {
                monitorTexts.text = "X"; 
            }

            canAttack = false;
        }
    }

    public void testPhase()
    {
        if(!firstPhase)
        {
            if(enemyHealth.health.Value <= (enemyHealth.maxHealth.Value * 0.66f) && enemyHealth.health.Value > (enemyHealth.maxHealth.Value * 0.33f))
            {
                onShield = true;
                firstPhase = true;
                monitorPhase.SpawnFirstWave();
            }
        }

        if(!secondPhase)
        {
            if(enemyHealth.health.Value <= (enemyHealth.maxHealth.Value * 0.33f))
            {
                onShield = true;
                firstPhase = false;
                secondPhase = true;
                monitorPhase.SpawnSecondWave();
            }
        }
    }

    private void Immunity()
    {
        if(firstPhase)
        {
            if(onShield)
            {
                enemyHealth.health.Value = enemyHealth.maxHealth.Value * 0.66f;
            }
        }
        else if(secondPhase)
        {
            if(onShield)
            {
                enemyHealth.health.Value = enemyHealth.maxHealth.Value * 0.33f;
            }
        }
    }

    private void ChooseDamageType()
    {
        int index = UnityEngine.Random.Range(0, 9);
        damageType = damageTypes[index];

        startTime = Time.time;
    }

    [ClientRpc]
    private void setDamageTypeClientRpc(string damageType)
    {
        foreach(var monitorTexts in monitorText)
        {
            monitorTexts.text = damageType; 
        }
    }
}