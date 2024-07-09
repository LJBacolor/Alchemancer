using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Info")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject playerStatsPanel;
    [SerializeField] private GameObject weaponStatsPanel;
    [SerializeField] private Image playerBtn;
    [SerializeField] private Image weaponBtn;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private CinemachineVirtualCamera mainVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera shopVirtualCamera;

    [Header("PLAYER")]

    [Header("Player HP")]
    [SerializeField] private GameObject hpUpgradeBtn;
    [SerializeField] private Image[] hpLvlImg;
    [SerializeField] private TextMeshProUGUI hpUpgradeCostText;
    public int[] hpUpgradeCost;

    [Header("Player DEF")]
    [SerializeField] private GameObject defUpgradeBtn;
    [SerializeField] private Image[] defLvlImg;
    [SerializeField] private TextMeshProUGUI defUpgradeCostText;
    public int[] defUpgradeCost;

    [Header("Player SHIELD")]
    [SerializeField] private GameObject shieldPanel;
    [SerializeField] private GameObject shieldUpgradeBtn;
    [SerializeField] private Image[] shieldLvlImg;
    [SerializeField] private TextMeshProUGUI shieldUpgradeCostText;
    public int[] shieldUpgradeCost;

    [Header("Player XTRA LIFE")]
    [SerializeField] private GameObject xtraLifePanel;
    [SerializeField] private GameObject xtraLifeUpgradeBtn;
    [SerializeField] private Image[] xtraLifeLvlImg;
    [SerializeField] private TextMeshProUGUI xtraLifeUpgradeCostText;
    public int[] xtraLifeUpgradeCost;

    [Header("WEAPON")]

    [Header("Weapon DMG")]
    [SerializeField] private GameObject dmgUpgradeBtn;
    [SerializeField] private Image[] dmgLvlImg;
    [SerializeField] private TextMeshProUGUI dmgUpgradeCostText;
    public int[] dmgUpgradeCost;

    [Header("Weapon FIRE RATE")]
    [SerializeField] private GameObject fireRateUpgradeBtn;
    [SerializeField] private Image[] fireRateLvlImg;
    [SerializeField] private TextMeshProUGUI fireRateUpgradeCostText;
    public int[] fireRateUpgradeCost;

    [Header("Weapon SKILL")]
    [SerializeField] private GameObject skillUpgradeBtn;
    [SerializeField] private Image[] skillLvlImg;
    [SerializeField] private TextMeshProUGUI skillUpgradeCostText;
    public int[] skillUpgradeCost;

    private void Start()
    {
        shopPanel.SetActive(false);
        PlayerStatsBtn();
    }

    private void Update()
    {
        goldText.text = PlayerInventory.goldAmount.ToString();

        //Player Stat
        UpdateHpLvl();
        UpdateHpUpgradeCost();

        UpdateDefLvl();
        UpdateDefUpgradeCost();

        UpdateShieldLvl();
        UpdateShieldUpgradeCost();

        UpdateXtraLifeLvl();
        UpdateXtraLifeUpgradeCost();

        //Weapon Stats
        UpdateDmgLvl();
        UpdateDmgUpgradeCost();

        UpdateFireRateLvl();
        UpdateFireRateUpgradeCost();

        UpdateSkillLvl();
        UpdateSkillUpgradeCost();
    }

    //Shop Info
    public void OpenShop()
    {
        mainVirtualCamera = GameObject.Find("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        shopPanel.SetActive(true);

        mainVirtualCamera.Priority = 0;
        shopVirtualCamera.Priority = 1;
    }

    public void CloseShop()
    {
        mainVirtualCamera = GameObject.Find("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();

        shopPanel.SetActive(false);

        NPCDialogue npcDialogue = GetComponent<NPCDialogue>();
        npcDialogue.EndDialogue();
        npcDialogue.EnablePlayer();

        mainVirtualCamera.Priority = 1;
        shopVirtualCamera.Priority = 0;

        PlayerStatsBtn();
        DataManager.Instance.SaveGame();
    }

    public void PlayerStatsBtn()
    {
        playerBtn.color = Color.white;
        weaponBtn.color = Color.grey;
        playerStatsPanel.SetActive(true);
        weaponStatsPanel.SetActive(false);
    }

    public void WeaponStatsBtn()
    {
        playerBtn.color = Color.grey;
        weaponBtn.color = Color.white;
        playerStatsPanel.SetActive(false);
        weaponStatsPanel.SetActive(true);
    }


    //Player HP
    public void HpUpgradeBtn()
    {
        if(PlayerStats.hpLvl < StatsManager.Instance.hpMaxLvl)
        {
            int upgradeCost = hpUpgradeCost[PlayerStats.hpLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.hpLvl++;
            }
        }
    }

    public void UpdateHpLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int hpLevel = PlayerStats.hpLvl;

        for (int i = 0; i < hpLvlImg.Length; i++)
        {
            hpLvlImg[i].color = (i < hpLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateHpUpgradeCost()
    {
        if(PlayerStats.hpLvl < StatsManager.Instance.hpMaxLvl)
        {
            hpUpgradeCostText.text = hpUpgradeCost[PlayerStats.hpLvl].ToString();
        }
        else
        {
            hpUpgradeCostText.text = "MAX";
        }
    }

    //Player DEF
    public void DefUpgradeBtn()
    {
        if(PlayerStats.defLvl < StatsManager.Instance.defMaxLvl)
        {
            int upgradeCost = defUpgradeCost[PlayerStats.defLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.defLvl++;
            }
        }
    }

    public void UpdateDefLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int defLevel = PlayerStats.defLvl;

        for (int i = 0; i < defLvlImg.Length; i++)
        {
            defLvlImg[i].color = (i < defLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateDefUpgradeCost()
    {
        if(PlayerStats.defLvl < StatsManager.Instance.defMaxLvl)
        {
            defUpgradeCostText.text = defUpgradeCost[PlayerStats.defLvl].ToString();
        }
        else
        {
            defUpgradeCostText.text = "MAX";
        }
    }

    //Player SHIELD
    public void ShieldUpgradeBtn()
    {
        if(PlayerStats.shieldLvl < StatsManager.Instance.shieldMaxLvl)
        {
            int upgradeCost = shieldUpgradeCost[PlayerStats.shieldLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.shieldLvl++;
            }
        }
    }

    public void UpdateShieldLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int shieldLevel = PlayerStats.shieldLvl;

        for (int i = 0; i < shieldLvlImg.Length; i++)
        {
            shieldLvlImg[i].color = (i < shieldLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateShieldUpgradeCost()
    {
        if(PlayerStats.shieldLvl < StatsManager.Instance.shieldMaxLvl)
        {
            shieldUpgradeCostText.text = shieldUpgradeCost[PlayerStats.shieldLvl].ToString();
        }
        else
        {
            shieldUpgradeCostText.text = "MAX";
        }
    }

    //Player XTRA LIFE
    public void XtraLifeUpgradeBtn()
    {
        if(PlayerStats.xtraLifeLvl < StatsManager.Instance.xtraLifeMaxLvl)
        {
            int upgradeCost = xtraLifeUpgradeCost[PlayerStats.xtraLifeLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.xtraLifeLvl++;
            }
        }
    }

    public void UpdateXtraLifeLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int xtraLifeLevel = PlayerStats.xtraLifeLvl;

        for (int i = 0; i < xtraLifeLvlImg.Length; i++)
        {
            xtraLifeLvlImg[i].color = (i < xtraLifeLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateXtraLifeUpgradeCost()
    {
        if(PlayerStats.xtraLifeLvl < StatsManager.Instance.xtraLifeMaxLvl)
        {
            xtraLifeUpgradeCostText.text = xtraLifeUpgradeCost[PlayerStats.xtraLifeLvl].ToString();
        }
        else
        {
            xtraLifeUpgradeCostText.text = "MAX";
        }
    }

    //Wepaon DMG
    public void DmgUpgradeBtn()
    {
        if(PlayerStats.dmgLvl < StatsManager.Instance.dmgMaxLvl)
        {
            int upgradeCost = dmgUpgradeCost[PlayerStats.dmgLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.dmgLvl++;
            }
        }
    }

    public void UpdateDmgLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int dmgLevel = PlayerStats.dmgLvl;

        for (int i = 0; i < dmgLvlImg.Length; i++)
        {
            dmgLvlImg[i].color = (i < dmgLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateDmgUpgradeCost()
    {
        if(PlayerStats.dmgLvl < StatsManager.Instance.dmgMaxLvl)
        {
            dmgUpgradeCostText.text = dmgUpgradeCost[PlayerStats.dmgLvl].ToString();
        }
        else
        {
            dmgUpgradeCostText.text = "MAX";
        }
    }

    //Wepaon FIRE RATE
    public void FireRateUpgradeBtn()
    {
        if(PlayerStats.fireRateLvl < StatsManager.Instance.fireRateMaxLvl)
        {
            int upgradeCost = fireRateUpgradeCost[PlayerStats.fireRateLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.fireRateLvl++;
            }
        }
    }

    public void UpdateFireRateLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int fireRateLevel = PlayerStats.fireRateLvl;

        for (int i = 0; i < fireRateLvlImg.Length; i++)
        {
            fireRateLvlImg[i].color = (i < fireRateLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateFireRateUpgradeCost()
    {
        if(PlayerStats.fireRateLvl < StatsManager.Instance.fireRateMaxLvl)
        {
            fireRateUpgradeCostText.text = fireRateUpgradeCost[PlayerStats.fireRateLvl].ToString();
        }
        else
        {
            fireRateUpgradeCostText.text = "MAX";
        }
    }

    //Wepaon SKILL
    public void SkillUpgradeBtn()
    {
        if(PlayerStats.skillLvl < StatsManager.Instance.skillMaxLvl)
        {
            int upgradeCost = skillUpgradeCost[PlayerStats.skillLvl];
            if(upgradeCost <= PlayerInventory.goldAmount)
            {
                PlayerInventory.goldAmount -= upgradeCost;
                PlayerStats.skillLvl++;
                PlayerAttack.Instance.UpdateSKill();
            }
        }
    }

    public void UpdateSkillLvl()
    {
        Color activeColor = Color.red;
        Color inactiveColor = Color.grey;
    
        int skillLevel = PlayerStats.skillLvl;

        for (int i = 0; i < skillLvlImg.Length; i++)
        {
            skillLvlImg[i].color = (i < skillLevel) ? activeColor : inactiveColor;
        }
    }

    public void UpdateSkillUpgradeCost()
    {
        if(PlayerStats.skillLvl < StatsManager.Instance.skillMaxLvl)
        {
            skillUpgradeCostText.text = skillUpgradeCost[PlayerStats.skillLvl].ToString();
        }
        else
        {
            skillUpgradeCostText.text = "MAX";
        }
    }
}