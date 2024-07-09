using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Unity.Netcode;

public class BuffsManager : NetworkBehaviour
{
    [SerializeField] private BuffsSriptableObject buffsSO;
    [SerializeField] private GameObject buffsCanvas;
    [SerializeField] private Image[] buffPanel;
    [SerializeField] private Sprite[] buffSpriteUI;
    [SerializeField] private TextMeshProUGUI[] buffUIName;
    [SerializeField] private TextMeshProUGUI[] buffUIDecription;
    [SerializeField] private TextMeshProUGUI[] buffUIRarity;
    private List<string> buffNames = new List<string>();
    private List<string> buffDescriptions = new List<string>();
    private List<string> buffRarity = new List<string>();
    private string selectedBuffName, selectedBuffDescription, selectedBuffRarity;
    private bool buffValue;

    private float baseCommonRate = 0.6f;
    private float baseRareRate = 0.3f;
    private float baseLegendaryRate = 0.1f;
    private float baseCursedRate = 0.05f;
    private float commonRate;
    private float rareRate;
    private float legendaryRate;
    private float cursedRate;

    private void Awake()
    {
        commonRate = baseCommonRate;
        rareRate = baseRareRate;
        legendaryRate = baseLegendaryRate;
        cursedRate = baseCursedRate;
    }

    private void Start()
    {
        buffsCanvas.SetActive(false);

        Scene currentscene = SceneManager.GetActiveScene();
        if(currentscene.name == "Lobby")
        {
            PlayerBuffs.resetAllBuffs();
        }

        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(sceneName == "Lobby")
        {
            PlayerBuffs.resetAllBuffs();
        }
    }

    private void Update()
    {
        GiveBuffs();

        if(Input.GetKeyDown(KeyCode.B))
        {
            //OpenBuff();
        }
    }

    public void OpenBuff()
    {
        buffsCanvas.SetActive(true);
        SetBuffsInfo();
    }

    private void SetBuffsInfo()
    {
        buffNames.Clear();
        buffDescriptions.Clear();
        buffRarity.Clear();

        for (int i = 0; i < 3; i++)
        {
            do
            {
                (selectedBuffName,  selectedBuffDescription, selectedBuffRarity) = BuffsSelection();
                string newBuffName = selectedBuffName.Replace(" ", "");
                //check if buff is true
                System.Type playerBuffsType = typeof(PlayerBuffs);
                System.Reflection.FieldInfo fieldInfo = playerBuffsType.GetField(newBuffName);
                // Check if fieldInfo is not null and it represents a boolean field
                if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
                {
                    // Get the value of the boolean field
                    buffValue = (bool)fieldInfo.GetValue(null);
                }
                else
                {
                    // The field is either not found or it's not a boolean field
                    Debug.LogWarning($"Field '{newBuffName}' is not a boolean field or not found.");
                    continue;
                }
            }
            while(buffValue);
            

            while (buffNames.Contains(selectedBuffName))
            {
                (selectedBuffName, selectedBuffDescription, selectedBuffRarity) = BuffsSelection();
            }

            buffNames.Add(selectedBuffName);
            buffDescriptions.Add(selectedBuffDescription);
            buffRarity.Add(selectedBuffRarity);
            SetBuffSpriteUI(i);
        }

        // Set the UI text values using a loop
        for (int i = 0; i < 3; i++)
        {
            buffUIName[i].text = buffNames[i];
            buffUIDecription[i].text = buffDescriptions[i];
            buffUIRarity[i].text = buffRarity[i];
        }
    }

    private (string, string, string) BuffsSelection()
    {
        // Calculate the total Rate
        float totalRate = commonRate + rareRate + legendaryRate + cursedRate;

        // Generate a random number between 0 and the total Rate
        float randomValue = Random.Range(0f, totalRate);

        // Pick a buff based on the random number
        if (randomValue < commonRate)
        {
            // Pick a common buff
            int index = Random.Range(0, buffsSO.commonBuffs.Length);
            return (buffsSO.commonBuffs[index].Name, buffsSO.commonBuffs[index].Description, "Common");
        }
        else if (randomValue < commonRate + rareRate)
        {
            // Pick a rare buff
            int index = Random.Range(0, buffsSO.rareBuffs.Length);
            return (buffsSO.rareBuffs[index].Name, buffsSO.rareBuffs[index].Description, "Rare");
        }
        else if (randomValue < commonRate + rareRate + legendaryRate)
        {
            // Pick a legendary buff
            int index = Random.Range(0, buffsSO.legendaryBuffs.Length);
            return (buffsSO.legendaryBuffs[index].Name, buffsSO.legendaryBuffs[index].Description, "Legendary");
        }
        else
        {
            // Pick a cursed buff
            int index = Random.Range(0, buffsSO.cursedBuffs.Length);
            return (buffsSO.cursedBuffs[index].Name, buffsSO.cursedBuffs[index].Description, "Cursed");
        }
    }

    private void SetBuffSpriteUI(int i)
    {
        switch(selectedBuffRarity)
        {
            case "Common":
                buffPanel[i].GetComponent<Image>().sprite = buffSpriteUI[0];
                break;
            case "Rare":
                buffPanel[i].GetComponent<Image>().sprite = buffSpriteUI[1];
                break;
            case "Legendary":
                buffPanel[i].GetComponent<Image>().sprite = buffSpriteUI[2];
                break;
            case "Cursed":
                buffPanel[i].GetComponent<Image>().sprite = buffSpriteUI[3];
                break;
            default:
                Debug.LogWarning("Can't assign sprite");
                break;
        }
    }

    //Buttons
    public void BuffButton(TextMeshProUGUI _buffName)
    {
        string newBuffName = _buffName.text.Replace(" ", "");

        System.Type playerBuffsType = typeof(PlayerBuffs);
        System.Reflection.FieldInfo fieldInfo = playerBuffsType.GetField(newBuffName);
        if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
        {
            fieldInfo.SetValue(null, true);
        }
        //Debug.Log(fieldInfo.GetValue(null));
        //Debug.Log(_buffName.text);
        Debug.Log("baseMaxHealth: " + PlayerStats.baseMaxHealth);
        Debug.Log("maxHealth: " + PlayerStats.maxHealth);
        Debug.Log("currentHealth: " + PlayerStats.currentHealth);
        Debug.Log("healBuff: " + PlayerStats.healBuff);
        Debug.Log("baseDefense: " + PlayerStats.baseDefense);
        Debug.Log("defense: " + PlayerStats.defense);
        Debug.Log("maxXtraLife: " + PlayerStats.maxXtraLife);
        Debug.Log("currentXtraLife: " + PlayerStats.currentXtraLife);
        Debug.Log("extraDamage: " + PlayerStats.extraDamage);
        Debug.Log("baseReactionDamage: " + PlayerStats.baseReactionDamage);
        Debug.Log("newReactionDamage: " + PlayerStats.newReactionDamage);
        Debug.Log("reactionDamage: " + PlayerStats.reactionDamage);
        Debug.Log("baseOxygenDamage: " + PlayerStats.baseOxygenDamage);
        Debug.Log("newOxygenDamage: " + PlayerStats.newOxygenDamage);
        Debug.Log("oxygenDamage: " + PlayerStats.oxygenDamage);
        Debug.Log("baseHydrogenDamage: " + PlayerStats.baseHydrogenDamage);
        Debug.Log("newHydrogenDamage: " + PlayerStats.newHydrogenDamage);
        Debug.Log("hydrogenDamage: " + PlayerStats.hydrogenDamage);
        Debug.Log("baseNitrogenDamage: " + PlayerStats.baseNitrogenDamage);
        Debug.Log("newNitrogenDamage: " + PlayerStats.newNitrogenDamage);
        Debug.Log("nitrogenDamage: " + PlayerStats.nitrogenDamage);
        Debug.Log("baseCarbonDamage: " + PlayerStats.baseCarbonDamage);
        Debug.Log("newCarbonDamage: " + PlayerStats.newCarbonDamage);
        Debug.Log("carbonDamage: " + PlayerStats.carbonDamage);
        Debug.Log("extraFireRate: " + PlayerStats.extraFireRate);
        Debug.Log("baseHydrogenFireRate: " + PlayerStats.baseHydrogenFireRate);
        Debug.Log("newHydrogenFireRate: " + PlayerStats.newHydrogenFireRate);
        Debug.Log("hydrogenFireRate: " + PlayerStats.hydrogenFireRate);
        Debug.Log("baseNitroCarbonFireRate: " + PlayerStats.baseNitroCarbonFireRate);
        Debug.Log("newNitroCarbonFireRate: " + PlayerStats.newNitroCarbonFireRate);
        Debug.Log("nitroCarbonFireRate: " + PlayerStats.nitroCarbonFireRate);
        Debug.Log("baseElementStatusDuration: " + PlayerStats.baseElementStatusDuration);
        Debug.Log("elementStatusDuration: " + PlayerStats.elementStatusDuration);

        buffsCanvas.SetActive(false);
    }

    public void GiveBuffs()
    {
        //Common
        if(PlayerBuffs.LifeSerumI)
        {
            //Increase Max HP by 5%
            float hpIncrease = PlayerStats.baseMaxHealth * 0.05f;
            PlayerStats.maxHealth = PlayerStats.baseMaxHealth + hpIncrease;
        }
        if(PlayerBuffs.BronzePlating)
        {
            //Increase DEF by 25
            PlayerStats.defense = PlayerStats.baseDefense + 25;
        }
        if(PlayerBuffs.Chemicreact)
        {
            //Increase Reaction Damage by 10%
            float reacPercentage = 0.1f;

            if(PlayerBuffs.BigBody)
            {
                float newReactPercentage = reacPercentage -= 0.5f;
                float reacIncrease = PlayerStats.baseReactionDamage * newReactPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
            else
            {
                float reacIncrease = PlayerStats.baseReactionDamage * reacPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
        }
        if(PlayerBuffs.BRRzzt)
        {
            //Increase Weapon Fire Rate by 10%
            float hyIncrease = PlayerStats.baseHydrogenFireRate * 0.1f;
            PlayerStats.newHydrogenFireRate = PlayerStats.baseHydrogenFireRate - hyIncrease;

            float ncIncrease = PlayerStats.baseNitroCarbonFireRate * 0.1f;
            PlayerStats.newNitroCarbonFireRate = PlayerStats.baseNitroCarbonFireRate - ncIncrease;
        }
        if(PlayerBuffs.ElemBent)
        {
            //Increase Elemental Status Duration by 50%
            float erIncrease = PlayerStats.baseElementStatusDuration * 0.5f;
            PlayerStats.elementStatusDuration = PlayerStats.baseElementStatusDuration + erIncrease;
        }
        if(PlayerBuffs.ShooterBoosterI)
        {
            //Increase Weapon Damage by 25%
            float oxyIncrease = (PlayerStats.baseOxygenDamage + PlayerStats.extraDamage) * 0.25f;
            PlayerStats.newOxygenDamage = PlayerStats.baseOxygenDamage + oxyIncrease;

            float hydroIncrease = (PlayerStats.baseHydrogenDamage + PlayerStats.extraDamage) * 0.25f;
            PlayerStats.newHydrogenDamage = PlayerStats.baseHydrogenDamage + hydroIncrease;

            float nitroIncrease = (PlayerStats.baseNitrogenDamage + PlayerStats.extraDamage) * 0.25f;
            PlayerStats.newNitrogenDamage = PlayerStats.baseNitrogenDamage + nitroIncrease;

            float carbonIncrease = (PlayerStats.baseCarbonDamage + PlayerStats.extraDamage) * 0.25f;
            PlayerStats.newCarbonDamage = PlayerStats.baseCarbonDamage + carbonIncrease;
        }
        // if(PlayerBuffs.Smartuse)
        // {
        //     //Decrease Skill CD by 10%
        //     float hyIncrease = PlayerAttack.Instance.baseHydroSkillCooldownTime * 0.1f;
        //     PlayerAttack.Instance.hydroSkillCooldownTime = PlayerAttack.Instance.baseHydroSkillCooldownTime - hyIncrease;

        //     float niIncrease = PlayerAttack.Instance.baseNitroSkillCooldownTime * 0.1f;
        //     PlayerAttack.Instance.nitroSkillCooldownTime = PlayerAttack.Instance.baseNitroSkillCooldownTime - niIncrease;
        // }
        if(PlayerBuffs.Justkeepgroovin)
        {
            //Reduce Damage recieved by 5% while moving
            //
            //Implemented on PlayerHealth script
            //
        }
        if(PlayerBuffs.Refresh)
        {
            //Heals 5% of max HP when entering a new level
            PlayerStats.healBuff = PlayerStats.maxHealth * 0.05f;
        }

        //Rare
        if(PlayerBuffs.Boostleg)
        {
            //Gain 1 Extra Dash
            PlayerController.Instance.doubleDashUnlocked = true;
        }
        if(PlayerBuffs.MadChemist)
        {
            //Killing an Enemy has a 25% chance to heal the player based on 2% of their Max HP
            //
            //Implemented on every EnemyHealth
            //
        }
        if(PlayerBuffs.LifeSerumII)
        {
            //Increase Max HP by 15%
            float percentage = 0.15f;

            if(PlayerBuffs.LifeSerumI) percentage = 0.20f;

            float hpIncrease = PlayerStats.baseMaxHealth * percentage;
            PlayerStats.maxHealth = PlayerStats.baseMaxHealth + hpIncrease;

        }
        if(PlayerBuffs.IronPlating)
        {
            //Increase DEF by 10%
            float percentage = 0.1f;
            
            float defIncrease = PlayerStats.baseDefense * percentage;
            if (PlayerBuffs.BronzePlating) defIncrease += 25;

            PlayerStats.defense = PlayerStats.baseDefense + defIncrease;
        }
        if(PlayerBuffs.CHEMboi)
        {
            //Increase Reaction Damage by 25%
            float reacPercentage = 0.25f;

            if(PlayerBuffs.Chemicreact) reacPercentage = 0.35f;
            
            if(PlayerBuffs.BigBody)
            {
                float newReactPercentage = reacPercentage -= 0.5f;
                float reacIncrease = PlayerStats.baseReactionDamage * newReactPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
            else
            {
                float reacIncrease = PlayerStats.baseReactionDamage * reacPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
        }
        if(PlayerBuffs.BRRRRzzt)
        {
            //Increase Weapon Fire Rate by 25%
            float percentage = 0.25f;

            if(PlayerBuffs.BRRzzt) percentage = 0.35f;

            float hyIncrease = PlayerStats.baseHydrogenFireRate * percentage;
            PlayerStats.newHydrogenDamage = PlayerStats.baseHydrogenFireRate - hyIncrease;

            float ncIncrease = PlayerStats.baseNitroCarbonFireRate * percentage;
            PlayerStats.newNitroCarbonFireRate = PlayerStats.baseNitroCarbonFireRate - ncIncrease;
        }
        if(PlayerBuffs.ShooterBoosterII)
        {
            //Increase Weapon Damage by 75%
            float percentage = 0.75f;

            if(PlayerBuffs.ShooterBoosterI) percentage = 1f;

            float oxyIncrease = (PlayerStats.baseOxygenDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newOxygenDamage = PlayerStats.baseOxygenDamage + oxyIncrease;

            float hydroIncrease = (PlayerStats.baseHydrogenDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newHydrogenDamage = PlayerStats.baseHydrogenDamage + hydroIncrease;

            float nitroIncrease = (PlayerStats.baseNitrogenDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newNitrogenDamage = PlayerStats.baseNitrogenDamage + nitroIncrease;

            float carbonIncrease = (PlayerStats.baseCarbonDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newCarbonDamage = PlayerStats.baseCarbonDamage + carbonIncrease;
        }
        if(PlayerBuffs.Selfburn)
        {
            //Cyanide Cores no longer deal damage to player
            //
            //Implemented on PlayerHealth script
            //
        }
        if(PlayerBuffs.Restore)
        {
            //Heals 10% of max HP when entering a new level
            if(PlayerBuffs.Refresh) PlayerStats.healBuff = PlayerStats.maxHealth * 0.15f;
            else PlayerStats.healBuff = PlayerStats.maxHealth * 0.1f;
        }
        
        //Legendary
        // if(PlayerBuffs.TimeInaBottle)
        // {
        //     //Decrease Skill CD by 25%
        //     float percentage = 0.25f;

        //     if(PlayerBuffs.Smartuse) percentage = 0.35f;

        //     float hyIncrease = PlayerAttack.Instance.baseHydroSkillCooldownTime * percentage;
        //     PlayerAttack.Instance.hydroSkillCooldownTime = PlayerAttack.Instance.baseHydroSkillCooldownTime - hyIncrease;

        //     float niIncrease = PlayerAttack.Instance.baseNitroSkillCooldownTime * percentage;
        //     PlayerAttack.Instance.nitroSkillCooldownTime = PlayerAttack.Instance.baseNitroSkillCooldownTime - niIncrease;
        // }
        if(PlayerBuffs.Supercharged)
        {
            //Increase Reaction Damage by 50%
            float reacPercentage = 0.5f;

            if(PlayerBuffs.CHEMboi && PlayerBuffs.Chemicreact) reacPercentage = 0.85f;
            else if(PlayerBuffs.CHEMboi) reacPercentage = 0.75f;
            else if(PlayerBuffs.Chemicreact) reacPercentage = 0.6f;
            
            if(PlayerBuffs.BigBody)
            {
                float newReactPercentage = reacPercentage -= 0.5f;
                float reacIncrease = PlayerStats.baseReactionDamage * newReactPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
            else
            {
                float reacIncrease = PlayerStats.baseReactionDamage * reacPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
        }
        if(PlayerBuffs.LifeSerumIII)
        {
            //Increase Max HP by 30%
            float percentage = 0.30f;

            if(PlayerBuffs.LifeSerumII && PlayerBuffs.LifeSerumI) percentage = 0.5f;
            else if(PlayerBuffs.LifeSerumII) percentage = 0.45f;
            else if(PlayerBuffs.LifeSerumI) percentage = 0.35f;

            float hpIncrease = PlayerStats.baseMaxHealth * percentage;
            PlayerStats.maxHealth = PlayerStats.baseMaxHealth + hpIncrease;
        }
        if(PlayerBuffs.SteelPlating)
        {
            //Increase DEF by 20%
            float percentage = 0.2f;

            if(PlayerBuffs.IronPlating) percentage = 0.3f;
            
            float defIncrease = PlayerStats.baseDefense * percentage;
            if (PlayerBuffs.BronzePlating) defIncrease += 25;
            PlayerStats.defense = PlayerStats.baseDefense + defIncrease;
        }
        if(PlayerBuffs.ShooterBoosterIII)
        {
            //Increase Weapon Damage by 150%
            float percentage = 1.5f;

            if(PlayerBuffs.ShooterBoosterII && PlayerBuffs.ShooterBoosterI) percentage = 2.5f;
            else if(PlayerBuffs.ShooterBoosterII) percentage = 2.25f;
            else if(PlayerBuffs.ShooterBoosterI) percentage = 1.75f;

            float oxyIncrease = (PlayerStats.baseOxygenDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newOxygenDamage = PlayerStats.baseOxygenDamage + oxyIncrease;

            float hydroIncrease = (PlayerStats.baseHydrogenDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newHydrogenDamage = PlayerStats.baseHydrogenDamage + hydroIncrease;

            float nitroIncrease = (PlayerStats.baseNitrogenDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newNitrogenDamage = PlayerStats.baseNitrogenDamage + nitroIncrease;

            float carbonIncrease = (PlayerStats.baseCarbonDamage + PlayerStats.extraDamage) * percentage;
            PlayerStats.newCarbonDamage = PlayerStats.baseCarbonDamage + carbonIncrease;
        }
        if(PlayerBuffs.Criteactions)
        {
            //Reactions have a 25% chance to deal double damage
            //
            //Implemented on EnemyStatus, MonitorStatus, and PlayerHealth script
            //
        }
        if(PlayerBuffs.Meditation)
        {
            //Heals 20% of max HP when entering a new level
            if(PlayerBuffs.Restore && PlayerBuffs.Refresh) PlayerStats.healBuff = PlayerStats.maxHealth * 0.35f;
            else if(PlayerBuffs.Restore) PlayerStats.healBuff = PlayerStats.maxHealth * 0.3f;
            else if(PlayerBuffs.Refresh) PlayerStats.healBuff = PlayerStats.maxHealth * 0.25f;
            else PlayerStats.healBuff = PlayerStats.maxHealth * 0.20f;
        }

        //Cursed
        if(PlayerBuffs.Surprise)
        {
            //Increase chance for better Buffs but options reduced to 2
            buffPanel[0].gameObject.SetActive(false);

            commonRate = 0.4f;
            rareRate = 0.35f;
            legendaryRate = 0.15f;
            cursedRate = 0.1f;
        }
        if(PlayerBuffs.Overcharged)
        {
            //Increase Reaction Damage by 100% but reduce DEF and HP by 50%

            //Reaction
            float reacPercentage = 1f;

            if(PlayerBuffs.Supercharged && PlayerBuffs.CHEMboi && PlayerBuffs.Chemicreact) reacPercentage = 1.85f;
            else if(PlayerBuffs.Supercharged && PlayerBuffs.CHEMboi) reacPercentage = 1.75f;
            else if(PlayerBuffs.Supercharged && PlayerBuffs.Chemicreact) reacPercentage = 1.6f;
            else if(PlayerBuffs.CHEMboi && PlayerBuffs.Chemicreact) reacPercentage = 1.35f;
            else if(PlayerBuffs.Supercharged) reacPercentage = 1.5f;
            else if(PlayerBuffs.CHEMboi) reacPercentage = 1.25f;
            else if(PlayerBuffs.Chemicreact) reacPercentage = 1.1f;

            if(PlayerBuffs.BigBody)
            {
                float newReactPercentage = reacPercentage -= 0.5f;
                float reacIncrease = PlayerStats.baseReactionDamage * newReactPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }
            else
            {
                float reacIncrease = PlayerStats.baseReactionDamage * reacPercentage;
                PlayerStats.newReactionDamage = PlayerStats.baseReactionDamage + reacIncrease;
            }

            //DEF
            float defPercentage = 0.5f;

            if(PlayerBuffs.SteelPlating && PlayerBuffs.IronPlating) defPercentage = 0.2f;
            else if(PlayerBuffs.SteelPlating) defPercentage = 0.3f;
            else if(PlayerBuffs.IronPlating) defPercentage = 0.4f;
            
            float defDecrease = PlayerStats.baseDefense * defPercentage;
            PlayerStats.defense = PlayerStats.baseDefense - defDecrease;

            //HP
            float hpPercentage = 0.5f;

            if(PlayerBuffs.LifeSerumIII && PlayerBuffs.LifeSerumII && PlayerBuffs.LifeSerumI) hpPercentage = 0;
            else if(PlayerBuffs.LifeSerumIII && PlayerBuffs.LifeSerumII) hpPercentage = 0.05f;
            else if(PlayerBuffs.LifeSerumIII && PlayerBuffs.LifeSerumI) hpPercentage = 0.15f;
            else if(PlayerBuffs.LifeSerumII && PlayerBuffs.LifeSerumI) hpPercentage = 0.3f;
            else if(PlayerBuffs.LifeSerumIII) hpPercentage = 0.2f;
            else if(PlayerBuffs.LifeSerumII) hpPercentage = 0.35f;
            else if(PlayerBuffs.LifeSerumI) hpPercentage = 0.45f;

            float hpDecrease = PlayerStats.baseMaxHealth * hpPercentage;
            PlayerStats.maxHealth = PlayerStats.baseMaxHealth - hpDecrease;
        }
        if(PlayerBuffs.Devolved)
        {
            //You no longer do reactions, convert reaction damage to weapon damage
            //
            //Implemented on Enemy and MonitorStatus scripts
            //
        }
        if(PlayerBuffs.BigBody)
        {
            //Increase Max HP by 75% but Decrease Reaction Damge by 50%
            
            //HP
            float hpPercentage = 0.75f; // 0.05 // 0.15 // 0.30

            if(PlayerBuffs.LifeSerumIII && PlayerBuffs.LifeSerumII && PlayerBuffs.LifeSerumI) hpPercentage = 1.25f;
            else if(PlayerBuffs.LifeSerumIII && PlayerBuffs.LifeSerumII) hpPercentage = 1.15f;
            else if(PlayerBuffs.LifeSerumIII && PlayerBuffs.LifeSerumI) hpPercentage = 1.1f;
            else if(PlayerBuffs.LifeSerumII && PlayerBuffs.LifeSerumI) hpPercentage = 0.95f;
            else if(PlayerBuffs.LifeSerumIII) hpPercentage = 1.05f;
            else if(PlayerBuffs.LifeSerumII) hpPercentage = 0.90f;
            else if(PlayerBuffs.LifeSerumI) hpPercentage = 0.80f;

            if(PlayerBuffs.Overcharged)
            {
                float newHpPercentage = hpPercentage -= 0.5f;
                float hpIncrease = PlayerStats.baseMaxHealth * newHpPercentage;
                PlayerStats.maxHealth = PlayerStats.baseMaxHealth + hpIncrease;
            }
            else
            {
                float hpIncrease = PlayerStats.baseMaxHealth * hpPercentage;
                PlayerStats.maxHealth = PlayerStats.baseMaxHealth + hpIncrease;
            }

            //Reaction
            //
            //Implemented on every reaction buff
            //
        }
        if(PlayerBuffs.BustorAllIn)
        {
            //Increase AU drops by 100% but decrease def by 50%

            //Gold
            PlayerInventory.goldValue = PlayerInventory.baseGoldValue * 2;

            //DEF
            float defPercentage = 0.5f;

            if(PlayerBuffs.Overcharged && PlayerBuffs.SteelPlating && PlayerBuffs.IronPlating) defPercentage = 0.7f;
            else if(PlayerBuffs.Overcharged && PlayerBuffs.SteelPlating) defPercentage = 0.8f;
            else if(PlayerBuffs.Overcharged && PlayerBuffs.IronPlating) defPercentage = 0.9f;
            else if(PlayerBuffs.SteelPlating && PlayerBuffs.IronPlating) defPercentage = 0.7f;
            else if(PlayerBuffs.Overcharged) defPercentage = 1f;
            else if(PlayerBuffs.SteelPlating) defPercentage = 0.3f;
            else if(PlayerBuffs.IronPlating) defPercentage = 0.4f;
            
            float defDecrease = PlayerStats.baseDefense * defPercentage;
            PlayerStats.defense = PlayerStats.baseDefense - defDecrease;
        }
    }
}
