using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.VFX;
using Unity.Netcode;
using UnityEngine.UIElements;
using System;

public class PlayerAttack : NetworkBehaviour
{
    public static PlayerAttack Instance;
    [Header("Weapons")]
    [SerializeField] private GameObject oxygenBullet;
    [SerializeField] private GameObject nitrogenBullet;
    [SerializeField] private GameObject carbonBullet;
    [SerializeField] private GameObject hydrogenFume;
    [SerializeField] private GameObject oxyLauncher;
    [SerializeField] private GameObject oxyDmgAreaInScene;
    [SerializeField] private GameObject oxyDmgAreaInPrefab;
    [SerializeField] private Transform bulletSpawn;

    [Header("Weapon Stats")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletForce = 10f;
    private float attackCooldownTime;
    [SerializeField] public float bulletDmg = 20f;

    [Header("Skills")]

    [Header("Oxygen")]
    [SerializeField] private GameObject oxySkillBullet;
    [SerializeField] private GameObject oxySkillCover;
    [SerializeField] private TextMeshProUGUI oxyCountText;
    private int oxygenSkillCount;

    [Header("Hydrogen")]
    [SerializeField] private GameObject hydroSkillBullet;
    [SerializeField] private GameObject hydroSkillCollider;
    [SerializeField] private GameObject hydroSkillCover;
    [SerializeField] private GameObject hydroCooldownCover;
    [SerializeField] private TextMeshProUGUI hydroCooldownText;
    [NonSerialized] public float baseHydroSkillCooldownTime = 30.0f;
    [NonSerialized] public float hydroSkillCooldownTime = 30.0f;

    [Header("Nitrogen")]
    [SerializeField] private GameObject nitroSkillBullet;
    [SerializeField] private GameObject nitroSkillCover;
    [SerializeField] private GameObject nitroCooldownCover;
    [SerializeField] private TextMeshProUGUI nitroCooldownText;
    [NonSerialized] public float baseNitroSkillCooldownTime = 30.0f;
    [NonSerialized] public float nitroSkillCooldownTime = 30.0f;
    private float skillRemainingCooldown;

    [Header("Carbon")]
    [SerializeField] private GameObject carbonSkillSurrounder;
    [SerializeField] private GameObject carbonSkillCover;


    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction skillAction;
    public NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private float distanceMultiplier;

    //Attack
    private float startAttackCooldown = 0f;
    public bool canAttack = true;
    private NetworkVariable<bool> inOxyRange = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> fumeActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //Skills
    private float startHydroSkillCooldown = 0f;
    private float startNitroSkillCooldown = 0f;
    private bool canHydroSkill = true;
    public bool canNitroSkill = true;

    private PlayerEquips playerEquips;

    private void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
        playerEquips = GetComponent<PlayerEquips>();

        attackAction = playerInput.actions["Attack"];
        skillAction = playerInput.actions["Skill"];
    }

    private void Start()
    {
        UpdateSKill();
    }

    private void Update()
    {
        if (!IsOwner) return;
        attackCooldownTime = PlayerStats.nitroCarbonFireRate;

        AttackCheck();
        CheckShoot();
        CheckFumeServerRpc();
        SetFumeDmgServerRpc(PlayerStats.hydrogenDamage,PlayerStats.hydrogenFireRate,PlayerStats.reactionDamage,PlayerBuffs.Criteactions,PlayerBuffs.Devolved);
    }

    private void AttackCheck()
    {
        if(canAttack)
        {
            GetComponent<PlayerController>().playerSpeed = 10f;

            if(attackAction.inProgress)
            {
                Shoot();
            }
            else
            {
                if(playerEquips.usingHydrogenFumeThrower.Value)
                {
                    fumeActive.Value = false;
                }
            }
        }
        else
        {
            GetComponent<PlayerController>().playerSpeed = 8f;

            float currentTime = Time.time;
            float elapsedTime = currentTime - startAttackCooldown;
            float remainingCooldownTime = Mathf.Max(0f, attackCooldownTime - elapsedTime);
            //Calculate the remaining cooldown percentage
            float cooldownPercentage = remainingCooldownTime / attackCooldownTime;

            if(currentTime >= startAttackCooldown + attackCooldownTime)
            {
                canAttack = true;
            }
        }

        //SKills
        // if(playerEquips.usingOxyblastLauncher.Value || playerEquips.usingCarbonShurikens.Value)
        // {
        //     if(skillAction.triggered)
        //     {
        //         UseSkill();
        //     }
        // }
        // else
        // {
        //     if(canHydroSkill)
        //     {
        //         if(skillAction.triggered)
        //         {
        //             UseSkill();
        //         }
        //         hydroSkillCover.SetActive(false);
        //         hydroCooldownCover.SetActive(false);
        //     }
        //     else
        //     {
        //         float currentTime = Time.time;
        //         float elapsedTime = currentTime - startHydroSkillCooldown;
        //         skillRemainingCooldown = Mathf.Max(0f, hydroSkillCooldownTime - elapsedTime);
        //         //Calculate the remaining cooldown percentage
        //         float cooldownPercentage = skillRemainingCooldown / hydroSkillCooldownTime;

        //         if(currentTime >= startHydroSkillCooldown + hydroSkillCooldownTime)
        //         {
        //             canHydroSkill = true;
        //         }
        //         hydroSkillCover.SetActive(true);
        //         hydroCooldownCover.SetActive(true);
        //         hydroCooldownText.text = Mathf.RoundToInt(skillRemainingCooldown).ToString();
        //     }

        //     if(canNitroSkill)
        //     {
        //         if(skillAction.triggered)
        //         {
        //             UseSkill();
        //         }

        //         nitroSkillCover.SetActive(false);
        //         nitroCooldownCover.SetActive(false);
        //     }
        //     else
        //     {
        //         float currentTime = Time.time;
        //         float elapsedTime = currentTime - startNitroSkillCooldown;
        //         skillRemainingCooldown = Mathf.Max(0f, nitroSkillCooldownTime - elapsedTime);
        //         //Calculate the remaining cooldown percentage
        //         float cooldownPercentage = skillRemainingCooldown / nitroSkillCooldownTime;

        //         if(currentTime >= startNitroSkillCooldown + nitroSkillCooldownTime)
        //         {
        //             canNitroSkill = true;
        //         }

        //         nitroSkillCover.SetActive(true);
        //         nitroCooldownCover.SetActive(true);
        //         nitroCooldownText.text = Mathf.RoundToInt(skillRemainingCooldown).ToString();
        //     }
        // }
        

        // SkillCheck();
    }

    private void Shoot()
    {
        if(!PlayerController.Instance.isDashing)
        {
            if (playerEquips.usingHydrogenFumeThrower.Value)
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.hydrogen, transform);
                fumeActive.Value = true;
            }
            else
            {
                ShootServerRpc();
            }
        }
        
        canAttack = false;
        startAttackCooldown = Time.time;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc(ServerRpcParams serverRpcParams = default)
    {   
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (playerEquips.usingOxyblastLauncher.Value && inOxyRange.Value)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.oxygen, transform);
            GameObject bulletObject = Instantiate(oxygenBullet, bulletSpawn.position, bulletSpawn.rotation);
            NetworkObject bulletNetworkObject = bulletObject.GetComponent<NetworkObject>();
            bulletNetworkObject.SpawnWithOwnership(clientId);
        }
        else if (playerEquips.usingNitrochargedBolts.Value)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.nitrogen, transform);
            GameObject bulletObject = Instantiate(nitrogenBullet, bulletSpawn.position, bulletSpawn.rotation);
            NetworkObject bulletNetworkObject = bulletObject.GetComponent<NetworkObject>();
            bulletNetworkObject.SpawnWithOwnership(clientId);
        }
        else if (playerEquips.usingCarbonShurikens.Value)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.carbon, transform);
            GameObject bulletObject = Instantiate(carbonBullet, bulletSpawn.position, bulletSpawn.rotation);
            NetworkObject bulletNetworkObject = bulletObject.GetComponent<NetworkObject>();
            bulletNetworkObject.SpawnWithOwnership(clientId);
            PlayerInventory.usingCarbonSkill = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckFumeServerRpc()
    {
        CheckFumeClientRpc();
    }

    [ClientRpc]
    private void CheckFumeClientRpc()
    {
        if(PlayerController.Instance.isDashing && IsOwner)
        {
            fumeActive.Value = false;
        }

        if (fumeActive.Value)
        {
            hydrogenFume.SetActive(true);
        }
        else
        {
            hydrogenFume.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetFumeDmgServerRpc(float hydDmg,float fireRate,float reactDmg,bool critR,bool dev)
    {
        hydrogenFume.GetComponent<FumeBullet>().hydroDmg = hydDmg;
        hydrogenFume.GetComponent<FumeBullet>().damageCooldown = fireRate;
        hydrogenFume.GetComponent<FumeBullet>().reactDmg = reactDmg;
        hydrogenFume.GetComponent<FumeBullet>().critReact = critR;
        hydrogenFume.GetComponent<FumeBullet>().devolved = dev;
    }

    private void UseSkill()
    {
        if (playerEquips.usingOxyblastLauncher.Value && inOxyRange.Value && 
        GameObject.Find("OxygenBullet(Clone)" ) == null && GameObject.Find("OxySkillBullet(Clone)") == null && 
        oxygenSkillCount > 0)
        {
            oxygenSkillCount--;
            Vector3 upwardOffset = new Vector3(0f, 2f, 0f);
            GameObject bullet = Instantiate(oxySkillBullet, bulletSpawn.position, bulletSpawn.rotation);
            Destroy(bullet, 5f);
            Instantiate(oxyDmgAreaInPrefab, targetPosition.Value, Quaternion.identity);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.AddForce((transform.forward + upwardOffset) * (distanceMultiplier * bulletForce));
        }
        else if(playerEquips.usingHydrogenFumeThrower.Value)
        {
            GameObject hydroSkillObj = Instantiate(hydroSkillBullet, bulletSpawn.position, bulletSpawn.rotation);
            StartCoroutine(StopHydroSkill(hydroSkillObj));

            GameObject hydroSkillCol = Instantiate(hydroSkillCollider, bulletSpawn.position, bulletSpawn.rotation);
            Rigidbody hydroSkillRb = hydroSkillCol.GetComponent<Rigidbody>();
            hydroSkillRb.AddForce(transform.forward * 8 * 100);
        }
        else if(playerEquips.usingNitrochargedBolts.Value)
        {
            GameObject bullet = Instantiate(nitroSkillBullet, bulletSpawn.position, bulletSpawn.rotation);
            Destroy(bullet, 3f);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.AddForce(transform.forward * bulletSpeed * 100);
        }
        else if(playerEquips.usingCarbonShurikens.Value && !PlayerInventory.usingCarbonSkill)
        {
            PlayerInventory.usingCarbonSkill = true;
            Instantiate(carbonSkillSurrounder, transform.position, Quaternion.identity);
        }
    }
    private IEnumerator StopHydroSkill(GameObject hydroSkillObj)
    {
        yield return new WaitForSeconds(0.3f);
        hydroSkillObj.GetComponent<VisualEffect>().Stop();
    }

    private void SkillCheck()
    {
        //Oxy
        oxyCountText.text = oxygenSkillCount.ToString();
        if(oxygenSkillCount > 0)
        {
            oxySkillCover.SetActive(false);
        }
        else
        {
            oxySkillCover.SetActive(true);
        }

        //Carbon
        if(PlayerInventory.usingCarbonSkill)
        {
            carbonSkillCover.SetActive(true);
        }
        else
        {
            carbonSkillCover.SetActive(false);
        }
    }

    public void UpdateSKill()
    {
        oxygenSkillCount = PlayerStats.oxySkillCount;
    }

    private void CheckShoot()
    {
        if(playerEquips.usingOxyblastLauncher.Value)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            
            // Create a RaycastHit variable to store hit information
            RaycastHit hit;

            LayerMask ignoreLayer = LayerMask.GetMask("Enemy");

            // Check if the ray hits something
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignoreLayer))
            {
                // Get the point where the ray hits the floor
                targetPosition.Value = hit.point;

                // Calculate distance-based multiplier
                distanceMultiplier = Vector3.Distance(bulletSpawn.position, targetPosition.Value);

                // Check if the distance is within the allowed range (10 units)
                if (distanceMultiplier <= 12f)
                {
                    inOxyRange.Value = true;
                    oxyDmgAreaInScene.SetActive(true);
                    oxyDmgAreaInScene.transform.position = targetPosition.Value;
                    oxyLauncher.transform.localRotation = Quaternion.identity * Quaternion.Euler((distanceMultiplier/12f) * 45f - 90f, 0, 0);
                }
                else
                {
                    inOxyRange.Value = false;
                    oxyDmgAreaInScene.SetActive(false);
                    // Handle the case where the target is beyond the allowed range
                }
            }
            else
            {
                // If the ray doesn't hit anything, you can set a default direction or handle it as needed
            } 
        }
        else
        {
            oxyDmgAreaInScene.SetActive(false);
        }
    }
}