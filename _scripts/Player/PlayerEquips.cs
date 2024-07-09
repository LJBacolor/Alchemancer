using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerEquips : NetworkBehaviour
{
    public static PlayerEquips Instance;

    [Header("Player Hair")]
    [SerializeField] private GameObject ValHair;
    [SerializeField] private GameObject VikHair;

    [Header("Player Label")]
    [SerializeField] private GameObject ValLabel;
    [SerializeField] private GameObject VikLabel;
    public GameObject cam;

    [Header("Player Weapons")]
    [SerializeField] private GameObject oxyblastLauncher;
    [SerializeField] private GameObject hydrogenFumeThrower;
    [SerializeField] private GameObject nitrochargedBolts;
    [SerializeField] private GameObject carbonShurikens;
    [SerializeField] private GameObject carbonShurikens2;

    [Header("Weapon UI")]
    [SerializeField] private GameObject weaponCanvas;
    [SerializeField] private GameObject oxygenWeaponUI;
    [SerializeField] private GameObject hydrogenWeaponUI;
    [SerializeField] private GameObject nitrogenWeaponUI;
    [SerializeField] private GameObject carbonWeaponUI;

    [Header("Weapon UI Cover")]
    [SerializeField] private GameObject oxygenCover;
    [SerializeField] private GameObject hydrogenCover;
    [SerializeField] private GameObject nitrogenCover;
    [SerializeField] private GameObject carbonCover;

    [Header("Skill UI")]
    [SerializeField] private GameObject oxygenSkill;
    [SerializeField] private GameObject hydrogenSkill;
    [SerializeField] private GameObject nitrogenSkill;
    [SerializeField] private GameObject carbonSkill;

    private PlayerInput playerInput;
    private InputAction backspaceAction;

    private PlayerController playerController;

    // Inputs Action Map
    private InputAction useOxyblastLauncherAction;
    private InputAction useHydrogenFumeThrowerAction;
    private InputAction useNitrochargedBoltsAction;
    private InputAction useCarbonShurikensAction;

    //Unlock Weapons
    public bool unlockOxyblastLauncher = false;
    public bool unlockHydrogenFumeThrower = false;
    public bool unlockNitrochargedBolts = false;
    public bool unlockCarbonShurikens = false;

    //Using Weapons
    public NetworkVariable<bool> usingOxyblastLauncher = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> usingHydrogenFumeThrower = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> usingNitrochargedBolts = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> usingCarbonShurikens = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> usingNothing = new NetworkVariable<bool>(true,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool canUseWeapons = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            weaponCanvas.SetActive(false);
        }

        playerInput = GetComponent<PlayerInput>();

        playerInput.actions.FindActionMap("Inputs").Enable();
        backspaceAction = playerInput.actions["Backspace"];

        // Inputs Action Map
        useOxyblastLauncherAction = playerInput.actions["UseOxyblastLauncher"];
        useHydrogenFumeThrowerAction = playerInput.actions["UseHydrogenFumeThrower"];
        useNitrochargedBoltsAction = playerInput.actions["UseNitrochargedBolts"];
        useCarbonShurikensAction = playerInput.actions["UseCarbonShurikens"];

        if(!IsServer && IsOwner){
            ToggleHairServerRpc();
        }

        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleHairServerRpc()
    {
        ToggleHairClientRpc();
    }

    [ClientRpc]
    private void ToggleHairClientRpc()
    {
        ValHair.SetActive(true);
        VikHair.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
        canUseWeapons = false;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(sceneName == "Lobby")
        {
            canUseWeapons = false;
        }
        else
        {
            canUseWeapons = true;
        }
    }

    private void Update()
    {
        cam = GameObject.Find("Main Camera");

        if (GameObject.Find("Main Camera"))
        {
            VikLabel.transform.LookAt(VikLabel.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
            ValLabel.transform.LookAt(ValLabel.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        }

        if (!IsOwner) return;
        UnlockWeapon();
        UseWeapons();
        CheckUnlock();
    }

    private void UnlockWeapon()
    {
        if(canUseWeapons)
        {
            unlockOxyblastLauncher = true;
            unlockHydrogenFumeThrower = true;
            unlockNitrochargedBolts = true;
            unlockCarbonShurikens = true;
        }
        else
        {
            unlockOxyblastLauncher = false;
            unlockHydrogenFumeThrower = false;
            unlockNitrochargedBolts = false;
            unlockCarbonShurikens = false;

            usingOxyblastLauncher.Value = false;
            usingHydrogenFumeThrower.Value = false;
            usingNitrochargedBolts.Value = false;
            usingCarbonShurikens.Value = false;

            PlayerInventory.usingCarbonSkill = false;
        }
    }

    private void CheckUnlock()
    {
        //Check is the weapon is unlocked
        if(unlockOxyblastLauncher)
        {
            oxygenWeaponUI.SetActive(true);
        }
        else
        {
            oxygenWeaponUI.SetActive(false);
        }

        if(unlockHydrogenFumeThrower)
        {
            hydrogenWeaponUI.SetActive(true);
        }
        else
        {
            hydrogenWeaponUI.SetActive(false);
        }

        if(unlockNitrochargedBolts)
        {
            nitrogenWeaponUI.SetActive(true);
        }
        else
        {
            nitrogenWeaponUI.SetActive(false);
        }

        if(unlockCarbonShurikens)
        {
            carbonWeaponUI.SetActive(true);
        }
        else
        {
            carbonWeaponUI.SetActive(false);
        }
    }

    private void UseWeapons()
    {
        if(useOxyblastLauncherAction.triggered && unlockOxyblastLauncher)
        {
            usingOxyblastLauncher.Value = true;
            usingHydrogenFumeThrower.Value = false;
            usingNitrochargedBolts.Value = false;
            usingCarbonShurikens.Value = false;
        }
        else if(useHydrogenFumeThrowerAction.triggered && unlockHydrogenFumeThrower)
        {
            usingOxyblastLauncher.Value = false;
            usingHydrogenFumeThrower.Value = true;
            usingNitrochargedBolts.Value = false;
            usingCarbonShurikens.Value = false;
        }
        else if(useNitrochargedBoltsAction.triggered && unlockNitrochargedBolts)
        {
            usingOxyblastLauncher.Value = false;
            usingHydrogenFumeThrower.Value = false;
            usingNitrochargedBolts.Value = true;
            usingCarbonShurikens.Value = false;
        }
        else if(useCarbonShurikensAction.triggered && unlockCarbonShurikens)
        {
            usingOxyblastLauncher.Value = false;
            usingHydrogenFumeThrower.Value = false;
            usingNitrochargedBolts.Value = false;
            usingCarbonShurikens.Value = true;
        }

        if(usingOxyblastLauncher.Value || usingHydrogenFumeThrower.Value || 
        usingNitrochargedBolts.Value || usingCarbonShurikens.Value)
        {
            usingNothing.Value = false;
        }
        else
        {
            usingNothing.Value = true;
        }

        ShowWeaponsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowWeaponsServerRpc()
    {
        ShowWeaponsClientRpc();
    }

    [ClientRpc]
    private void ShowWeaponsClientRpc()
    {
        //To show the weapon that being used
        if(usingOxyblastLauncher.Value)
        {
            oxyblastLauncher.SetActive(true);
            hydrogenFumeThrower.SetActive(false);
            nitrochargedBolts.SetActive(false);
            carbonShurikens.SetActive(false);
            carbonShurikens2.SetActive(false);

            oxygenCover.SetActive(false);
            hydrogenCover.SetActive(true);
            nitrogenCover.SetActive(true);
            carbonCover.SetActive(true);

            oxygenSkill.SetActive(true);
            hydrogenSkill.SetActive(false);
            nitrogenSkill.SetActive(false);
            carbonSkill.SetActive(false);
        }
        else if(usingHydrogenFumeThrower.Value)
        {
            oxyblastLauncher.SetActive(false);
            hydrogenFumeThrower.SetActive(true);
            nitrochargedBolts.SetActive(false);
            carbonShurikens.SetActive(false);
            carbonShurikens2.SetActive(false);

            oxygenCover.SetActive(true);
            hydrogenCover.SetActive(false);
            nitrogenCover.SetActive(true);
            carbonCover.SetActive(true);

            oxygenSkill.SetActive(false);
            hydrogenSkill.SetActive(true);
            nitrogenSkill.SetActive(false);
            carbonSkill.SetActive(false);
        }
        else if(usingNitrochargedBolts.Value)
        {
            oxyblastLauncher.SetActive(false);
            hydrogenFumeThrower.SetActive(false);
            nitrochargedBolts.SetActive(true);
            carbonShurikens.SetActive(false);
            carbonShurikens2.SetActive(false);

            oxygenCover.SetActive(true);
            hydrogenCover.SetActive(true);
            nitrogenCover.SetActive(false);
            carbonCover.SetActive(true);

            oxygenSkill.SetActive(false);
            hydrogenSkill.SetActive(false);
            nitrogenSkill.SetActive(true);
            carbonSkill.SetActive(false);
        }
        else if(usingCarbonShurikens.Value)
        {
            oxyblastLauncher.SetActive(false);
            hydrogenFumeThrower.SetActive(false);
            nitrochargedBolts.SetActive(false);
            carbonShurikens.SetActive(true);
            carbonShurikens2.SetActive(true);

            oxygenCover.SetActive(true);
            hydrogenCover.SetActive(true);
            nitrogenCover.SetActive(true);
            carbonCover.SetActive(false);

            oxygenSkill.SetActive(false);
            hydrogenSkill.SetActive(false);
            nitrogenSkill.SetActive(false);
            carbonSkill.SetActive(true);
        }
        else if(usingNothing.Value)
        {
            oxyblastLauncher.SetActive(false);
            hydrogenFumeThrower.SetActive(false);
            nitrochargedBolts.SetActive(false);
            carbonShurikens.SetActive(false);
            carbonShurikens2.SetActive(false);

            oxygenCover.SetActive(true);
            hydrogenCover.SetActive(true);
            nitrogenCover.SetActive(true);
            carbonCover.SetActive(true);

            oxygenSkill.SetActive(false);
            hydrogenSkill.SetActive(false);
            nitrogenSkill.SetActive(false);
            carbonSkill.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if(col.gameObject.tag == "LobbyBox")
        {
            canUseWeapons = false;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "LobbyBox")
        {
            canUseWeapons = true;
            PlayerAttack.Instance.UpdateSKill();
        }
    }

    public void AnimReset()
    {
        playerController.anim.SetBool("isRunO", false);
        playerController.anim.SetBool("isRunH", false);
        playerController.anim.SetBool("isRunN", false);
        playerController.anim.SetBool("isRunC", false);

        playerController.anim.SetBool("isIdleO", false);
        playerController.anim.SetBool("isIdleH", false);
        playerController.anim.SetBool("isIdleN", false);
        playerController.anim.SetBool("isIdleC", false);
    }
}
