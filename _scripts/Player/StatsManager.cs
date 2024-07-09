using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatsManager : NetworkBehaviour
{
    public static StatsManager Instance;
    
    [Header("PLAYER")]

    [Header("HP")]
    public float[] hpStats;
    [NonSerialized] public int hpMaxLvl = 5;

    [Header("DEF")]
    public int[] defStats;
    [NonSerialized] public int defMaxLvl = 5;

    [Header("SHIELD")]
    public float[] shieldStats;
    [NonSerialized] public int shieldMaxLvl = 3;

    [Header("XTRA LIFE")]
    [NonSerialized] public int xtraLifeMaxLvl = 3;

    [Header("WEAPON")]

    [Header("DMG")]
    public int[] dmgStats;
    [NonSerialized] public int dmgMaxLvl = 5;

    [Header("FIRE RATE")]
    public float[] fireRateStats;
    [NonSerialized] public int fireRateMaxLvl = 5;

    [Header("SKILL")]
    public float[] skillStats;
    [NonSerialized] public int skillMaxLvl = 5;
    

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if(IsServer)
        {
            //Player Stats
            UpdateHpStatClientRpc(PlayerStats.hpLvl);

            UpdateDefStatClientRpc(PlayerStats.defLvl);

            UpdateShieldStatClientRpc(PlayerStats.shieldLvl);

            UpdateXtraLifeStatClientRpc(PlayerStats.xtraLifeLvl);

            //Weapon Stats
            UpdateDmgStatClientRpc(PlayerStats.dmgLvl);
            CalculateDamageClientRpc();
            
            UpdateFireRateStatClientRpc(PlayerStats.fireRateLvl);
            CalculateFireRateClientRpc();

            UpdateSkillStatClientRpc();
        }
    }

    //Player HP
    [ClientRpc]
    private void UpdateHpStatClientRpc(int hpLvl)
    {
        if(hpLvl > 0)
        {
            PlayerStats.maxHealth = hpStats[hpLvl - 1];
            PlayerStats.baseMaxHealth = hpStats[hpLvl - 1];
        }
        else
        {
            PlayerStats.maxHealth = 100f;
            PlayerStats.baseMaxHealth = 100f;
        }
    }

    //Player DEF
    [ClientRpc]
    private void UpdateDefStatClientRpc(int defLvl)
    {
        if(defLvl > 0)
        {
            PlayerStats.defense = defStats[defLvl - 1];
            PlayerStats.baseDefense = defStats[defLvl - 1];
        }
    }

    //Player SHIELD
    [ClientRpc]
    private void UpdateShieldStatClientRpc(int shieldLvl)
    {
        if(shieldLvl > 0)
        {
            PlayerInventory.hasShield = true;
            PlayerShield.Instance.shieldCooldown = shieldStats[shieldLvl - 1];
        }
    }

    //Player XTRA LIFE
    [ClientRpc]
    private void UpdateXtraLifeStatClientRpc(int xtraLifeLvl)
    {
        PlayerStats.maxXtraLife = xtraLifeLvl;
    }

    //Weapon DMG
    [ClientRpc]
    private void UpdateDmgStatClientRpc(int dmgLvl)
    {
        if(dmgLvl > 0)
        {
            PlayerStats.extraDamage = dmgStats[dmgLvl - 1];
        }
    }

    //Weapon FIRE RATE
    [ClientRpc]
    private void UpdateFireRateStatClientRpc(int fireRateLvl)
    {
        if(fireRateLvl > 0)
        {
            PlayerStats.extraFireRate = fireRateStats[fireRateLvl - 1];
        }
    }

    [ClientRpc]
    private void CalculateDamageClientRpc()
    {
        PlayerStats.reactionDamage = PlayerStats.newReactionDamage + (PlayerStats.extraDamage * 2);
        PlayerStats.oxygenDamage = PlayerStats.newOxygenDamage + PlayerStats.extraDamage;
        PlayerStats.hydrogenDamage = PlayerStats.newHydrogenDamage + (PlayerStats.extraDamage / 5);
        PlayerStats.nitrogenDamage = PlayerStats.newNitrogenDamage + PlayerStats.extraDamage;
        PlayerStats.carbonDamage = PlayerStats.newCarbonDamage + PlayerStats.extraDamage;
    }

    [ClientRpc]
    private void CalculateFireRateClientRpc()
    {
        PlayerStats.hydrogenFireRate = PlayerStats.newHydrogenFireRate - (PlayerStats.extraFireRate / 2);
        PlayerStats.nitroCarbonFireRate = PlayerStats.newNitroCarbonFireRate - PlayerStats.extraFireRate;
    }

    //Weapon SKILL
    [ClientRpc]
    private void UpdateSkillStatClientRpc()
    {
        if(PlayerStats.skillLvl > 0)
        {
            switch(PlayerStats.skillLvl)
            {
                case 1:
                    PlayerStats.extraSkillDamage = skillStats[PlayerStats.skillLvl - 1];
                    PlayerAttack.Instance.hydroSkillCooldownTime = 27f;
                    PlayerAttack.Instance.nitroSkillCooldownTime = 27f;
                    PlayerStats.carbonSkillCount = PlayerStats.skillLvl + 1;
                    break;
                case 2:
                    PlayerStats.extraSkillDamage = skillStats[PlayerStats.skillLvl - 1];
                    PlayerAttack.Instance.hydroSkillCooldownTime = 24f;
                    PlayerAttack.Instance.nitroSkillCooldownTime = 24f;
                    PlayerStats.carbonSkillCount = PlayerStats.skillLvl + 1;
                    break;
                case 3:
                    PlayerStats.extraSkillDamage = skillStats[PlayerStats.skillLvl - 1];
                    PlayerStats.oxySkillCount = 2;
                    PlayerAttack.Instance.hydroSkillCooldownTime = 21f;
                    PlayerAttack.Instance.nitroSkillCooldownTime = 21f;
                    PlayerStats.carbonSkillCount = PlayerStats.skillLvl + 1;
                    break;
                case 4:
                    PlayerStats.extraSkillDamage = skillStats[PlayerStats.skillLvl - 1];
                    PlayerAttack.Instance.hydroSkillCooldownTime = 18f;
                    PlayerAttack.Instance.nitroSkillCooldownTime = 18f;
                    PlayerStats.carbonSkillCount = PlayerStats.skillLvl + 1;
                    break;
                case 5:
                    PlayerStats.oxySkillCount = 3;
                    PlayerAttack.Instance.hydroSkillCooldownTime = 15f;
                    PlayerAttack.Instance.nitroSkillCooldownTime = 15f;
                    PlayerStats.carbonSkillCount = PlayerStats.skillLvl + 1;
                    break;
                default:
                    break;
            }
        }
    }
}
