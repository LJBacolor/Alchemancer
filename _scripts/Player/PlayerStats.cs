using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStats
{
    //Player
    public static float baseMaxHealth = 100f;//for the buffs
    public static float maxHealth = 100f;
    public static float currentHealth = 100f;
    public static float healBuff = 0;

    public static float baseDefense = 10f;//for the buffs
    public static float defense = 10f;

    public static int maxXtraLife = 0;
    public static int currentXtraLife = 0;

    //Player Stat Lvl
    public static int hpLvl = 0;
    public static int defLvl = 0;
    public static int shieldLvl = 0;
    public static int xtraLifeLvl = 0;
    public static int dmgLvl = 0;
    public static int fireRateLvl = 0;
    public static int skillLvl = 0;

    //Weapon DMG
    public static float extraDamage = 0f;

    public static float baseReactionDamage = 60f;
    public static float newReactionDamage = 60f;
    public static float reactionDamage;

    public static float baseOxygenDamage = 15f;
    public static float newOxygenDamage = 15f;
    public static float oxygenDamage;

    public static float baseHydrogenDamage = 10f;
    public static float newHydrogenDamage = 10f;
    public static float hydrogenDamage;

    public static float baseNitrogenDamage = 20f;
    public static float newNitrogenDamage = 20f;
    public static float nitrogenDamage;

    public static float baseCarbonDamage = 20f;
    public static float newCarbonDamage = 20f;
    public static float carbonDamage;

    //Fire Rate
    public static float extraFireRate = 0f;

    public static float baseHydrogenFireRate = 0.5f;
    public static float newHydrogenFireRate = 0.5f;
    public static float hydrogenFireRate;

    public static float baseNitroCarbonFireRate = 1f;
    public static float newNitroCarbonFireRate = 1f;
    public static float nitroCarbonFireRate;

    //Skills
    public static float extraSkillDamage = 0f;
    public static int oxySkillCount = 1;
    public static int carbonSkillCount = 1;

    public static float baseElementStatusDuration = 5f;
    public static float elementStatusDuration = 5f;
}
