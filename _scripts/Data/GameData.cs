using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //Stats Lvl
    //Player
    public int hp;
    public int def;
    public int shield;
    public int xtraLife;

    //Weapon
    public int dmg;
    public int fireRate;

    //PlayerInventory
    public int gold;

    //PlayerProgress
    public bool tutorialFinished;

    //GameAudio
    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;

    // The values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData()
    {
        //Stats Lvl
        //Player
        this.hp = 0;
        this.def = 0;
        this.shield = 0;
        this.xtraLife = 0;

        //Weapon
        this.dmg = 0;
        this.fireRate = 0;

        //PLayerInventory
        this.gold = 0;

        //PlayerProgress
        this.tutorialFinished = false;

        //GameAudio
        this.masterVolume = 1f;
        this.sfxVolume = 1f;
        this.musicVolume = 1f;
    }
}
