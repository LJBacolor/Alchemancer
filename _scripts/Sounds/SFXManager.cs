using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SFXManager : NetworkBehaviour
{
    public static SFXManager Instance;
    [SerializeField] private AudioSource sxfObject;

    [Header("Player")]
    [SerializeField] public AudioClip hurt;
    [SerializeField] public AudioClip dash;
    [SerializeField] public AudioClip oxygen;
    [SerializeField] public AudioClip hydrogen;
    [SerializeField] public AudioClip nitrogen;
    [SerializeField] public AudioClip carbon;
    [SerializeField] public AudioClip CO2;
    [SerializeField] public AudioClip H2O;
    [SerializeField] public AudioClip NO;
    [SerializeField] public AudioClip NH3;
    [SerializeField] public AudioClip CH4;
    [SerializeField] public AudioClip CN;

    [Header("Objects")]
    [SerializeField] public AudioClip coin;
    [SerializeField] public AudioClip buff;
    [SerializeField] public AudioClip doorOpen;
    [SerializeField] public AudioClip doorClose;
    [SerializeField] public AudioClip button;
    [SerializeField] public AudioClip explosion;
    [SerializeField] public AudioClip ping;

    [Header("Bosses")]
    [SerializeField] public AudioClip wormDig;
    [SerializeField] public AudioClip daisySurround;
    [SerializeField] public AudioClip daisyInstant;
    [SerializeField] public AudioClip daisyOnShield;
    [SerializeField] public AudioClip buzzer;
    [SerializeField] public AudioClip beeps;
    [SerializeField] public AudioClip rotatingLaser;
    [SerializeField] public AudioClip bigLaser;
    [SerializeField] public AudioClip fallingBreak;

    [Header("Insect")]
    [SerializeField] public AudioClip insectDashAttack;
    [SerializeField] public AudioClip insectRangeAttack;
    [SerializeField] public AudioClip insectJumpAttack;

    [Header("Plant")]
    [SerializeField] public AudioClip plantCannonAttack;
    [SerializeField] public AudioClip plantMageAttack;
    [SerializeField] public AudioClip plantMeleeAttack;

    [Header("Robot")]
    [SerializeField] public AudioClip robotDashAttack;
    [SerializeField] public AudioClip robotLaserAttack;
    [SerializeField] public AudioClip robotMeleeAttack;
    [SerializeField] public AudioClip robotShootAttack;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySFXClip(AudioClip audioClip, Transform spawnTransform)
    {
        AudioClip clipToPlay = null;

        switch (audioClip.name)
        {
            case "Hurt":
                clipToPlay = hurt;
                break;
            case "Dash":
                clipToPlay = dash;
                break;
            case "CO2":
                clipToPlay = CO2;
                break;
            case "H2O":
                clipToPlay = H2O;
                break;
            case "NO and Acid":
                clipToPlay = NO;
                break;
            case "NH3":
                clipToPlay = NH3;
                break;
            case "CH4":
                clipToPlay = CH4;
                break;
            case "CN":
                clipToPlay = CN;
                break;
            case "Open Buff":
                clipToPlay = buff;
                break;
            case "Button click":
                clipToPlay = button;
                break;
            default:
                PlaySFXClipClientRpc(audioClip.name, spawnTransform.position);
                break;
        }

        if (clipToPlay != null)
        {
            AudioSource audioSource = Instantiate(sxfObject, spawnTransform.position, Quaternion.identity);
            audioSource.clip = clipToPlay;
            audioSource.Play();
            float clipLength = clipToPlay.length;
            Destroy(audioSource.gameObject, clipLength);
        }
    }

    [ClientRpc]
    public void PlaySFXClipClientRpc(string clipName, Vector3 spawnTransform)
    {
        AudioClip clipToPlay = null;

        switch (clipName)
        {
            case "Oxygen Attack":
                clipToPlay = oxygen;
                break;
            case "Hydrogen Attack":
                clipToPlay = hydrogen;
                break;
            case "Nitrocharged bolt":
                clipToPlay = nitrogen;
                break;
            case "Shuriken Attack":
                clipToPlay = carbon;
                break;
            case "Door open":
                clipToPlay = doorOpen;
                break;
            case "Door close":
                clipToPlay = doorClose;
                break;
            case "Button click":
                clipToPlay = button;
                break;
            case "Explosion":
                clipToPlay = explosion;
                break;
            case "Ping":
                clipToPlay = ping;
                break;
            case "Worm Dig":
                clipToPlay = wormDig;
                break;
            case "Daisy Surround Attack":
                clipToPlay = daisySurround;
                break;
            case "Daisy Instant Attack":
                clipToPlay = daisyInstant;
                break;
            case "Daisy OnShield":
                clipToPlay = daisyOnShield;
                break;
            case "buzzer":
                clipToPlay = buzzer;
                break;
            case "beeps":
                clipToPlay = beeps;
                break;
            case "Elementor LaserRotating":
                clipToPlay = rotatingLaser;
                break;
            case "Elementor BigLaser":
                clipToPlay = bigLaser;
                break;
            case "Elementor DebrisBreak":
                clipToPlay = fallingBreak;
                break;
            case "Insect Dash Attack":
                clipToPlay = insectDashAttack;
                break;
            case "Insect Ranged Attack":
                clipToPlay = insectRangeAttack;
                break;
            case "Insect Jump Attack":
                clipToPlay = insectJumpAttack;
                break;
            case "Plant_Cannon_Attack":
                clipToPlay = plantCannonAttack;
                break;
            case "Plant_Mage_Attack":
                clipToPlay = plantMageAttack;
                break;
            case "Plant_Melee_Punch_1":
                clipToPlay = plantMeleeAttack;
                break;
            case "Robot Dash Attack":
                clipToPlay = robotDashAttack;
                break;
            case "Robot Laser Attack":
                clipToPlay = robotLaserAttack;
                break;
            case "Robot Melee Attack":
                clipToPlay = robotMeleeAttack;
                break;
            case "Robot Shoot Attack":
                clipToPlay = robotShootAttack;
                break;
            default:
                Debug.Log("Audio clip not found: " + clipName);
                break;
        }

        if (clipToPlay != null)
        {
            AudioSource audioSource = Instantiate(sxfObject, spawnTransform, Quaternion.identity);
            audioSource.clip = clipToPlay;
            audioSource.Play();
            float clipLength = clipToPlay.length;
            Destroy(audioSource.gameObject, clipLength);
        }
    }
}
