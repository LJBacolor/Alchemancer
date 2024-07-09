using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadData : MonoBehaviour, IDataPersistence
{
    public void LoadData(GameData data)
    {
        //Stats Lvl
        //Player
        PlayerStats.hpLvl = data.hp;
        PlayerStats.defLvl = data.def;
        PlayerStats.shieldLvl = data.shield;
        PlayerStats.xtraLifeLvl = data.xtraLife;

        //Weapon
        PlayerStats.dmgLvl = data.dmg;
        PlayerStats.fireRateLvl = data.fireRate;

        //PLayerInventory
        PlayerInventory.goldAmount = data.gold;

        //PlayerProgress
        PlayerProgress.tutorialFinished = data.tutorialFinished;
    }

    public void SaveData(GameData data)
    {
        //Stats Lvl
        //Player
        data.hp = PlayerStats.hpLvl;
        data.def = PlayerStats.defLvl;
        data.shield = PlayerStats.shieldLvl;
        data.xtraLife = PlayerStats.xtraLifeLvl;

        //Weapon
        data.dmg = PlayerStats.dmgLvl;
        data.fireRate = PlayerStats.fireRateLvl;

        //PLayerInventory
        data.gold = PlayerInventory.goldAmount;

        //PlayerProgress
        data.tutorialFinished = PlayerProgress.tutorialFinished;
    }
}