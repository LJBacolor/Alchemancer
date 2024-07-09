using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DummyStatus : NetworkBehaviour
{
    [Header("Statuses")]
    [SerializeField] private GameObject statusCanvas;
    [SerializeField] private GameObject oxygenStatus;
    [SerializeField] private GameObject hydrogenStatus;
    [SerializeField] private GameObject nitrogenStatus;
    [SerializeField] private GameObject carbonStatus;
    [SerializeField] private GameObject poisonStatus;
    [SerializeField] private float statusCooldown = 3;

    [Header("Reaction Labels")]
    [SerializeField] private GameObject CO2canvas;
    [SerializeField] private GameObject H2Ocanvas;
    [SerializeField] private GameObject NOcanvas;
    [SerializeField] private GameObject NH3canvas;
    [SerializeField] private GameObject CH4canvas;
    [SerializeField] private GameObject CNcanvas;

    [Header("Dummy types")]
    [SerializeField] private bool isInsect;
    [SerializeField] private bool isPlant;
    [SerializeField] private bool isRobot;

    [Header("Reaction Prefabs")]
    [SerializeField] private GameObject areaOxygen;
    [SerializeField] private GameObject H2O;
    [SerializeField] private GameObject NO;
    [SerializeField] private GameObject CH4;
    [SerializeField] private GameObject cyanideCore;
    [SerializeField] private GameObject CN;

    private DummyHealth dummyHealth;
    private Camera cam;
    private float cooldownTimer;

    private float dotCooldown = 0.5f;
    private float nextDamageTime;

    private bool hasOxygen = false;
    private bool hasHydrogen = false;
    private bool hasNitrogen = false;
    private bool hasCarbon = false;

    private float latestReactDmg = 0f;

    private void Awake()
    {
        dummyHealth = GetComponent<DummyHealth>();
        cam = Camera.main;
    }

    private void Start()
    {
        cooldownTimer = statusCooldown;
    }

    private void Update() 
    {
        CheckStatus();
        CheckReaction();
    }

    public void setReactionDmg(float reactDmg)
    {
        latestReactDmg = reactDmg;
    }

    private void OnTriggerEnter(Collider col) 
    {
        ApplyElementServerRpc(col.tag);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyElementServerRpc(string tag)
    {
        ApplyElementClientRpc(tag);
    }

    [ClientRpc]
    private void ApplyElementClientRpc(string tag)
    {
        if (tag == "Oxygen")
        {
            hasOxygen = true;
            if (hasOxygen)
            {
                cooldownTimer = statusCooldown;
            }

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 2)
            {
                TutorialManager.Instance.useO = true;
            }
        }
        else if (tag == "Hydrogen")
        {
            hasHydrogen = true;
            if (hasHydrogen)
            {
                cooldownTimer = statusCooldown;
            }

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 2)
            {
                TutorialManager.Instance.useH = true;
            }
        }
        else if (tag == "Nitrogen")
        {
            hasNitrogen = true;
            if (hasNitrogen)
            {
                cooldownTimer = statusCooldown;
            }

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 2)
            {
                TutorialManager.Instance.useN = true;
            }
        }
        else if (tag == "Carbon")
        {
            hasCarbon = true;
            if (hasCarbon)
            {
                cooldownTimer = statusCooldown;
            }

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 2)
            {
                TutorialManager.Instance.useC = true;
            }
        }
        else if (tag == "CN")
        {
            dummyHealth.TakeDamage(latestReactDmg);
        }
    }

    private void OnTriggerStay(Collider col) 
    {
        if (Time.time >= nextDamageTime) 
        {
            if(col.tag == "NO")
            {
                dummyHealth.TakeDamage(latestReactDmg * 0.2f);
            }
            nextDamageTime = Time.time + dotCooldown; // Set the next allowed damage time
        }
    }

    private void CheckStatus()
    {
        statusCanvas.transform.LookAt(statusCanvas.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        if(hasOxygen)
        {
            oxygenStatus.SetActive(true);
            hasOxygen = StatusCooldown();
        }
        else
        {
            oxygenStatus.SetActive(false);
        }

        if(hasHydrogen)
        {
            hydrogenStatus.SetActive(true);
            hasHydrogen = StatusCooldown();
        }
        else
        {
            hydrogenStatus.SetActive(false);
        }

        if(hasNitrogen)
        {
            nitrogenStatus.SetActive(true);
            hasNitrogen = StatusCooldown();
        }
        else
        {
            nitrogenStatus.SetActive(false);
        }

        if(hasCarbon)
        {
            carbonStatus.SetActive(true);
            hasCarbon = StatusCooldown();
        }
        else
        {
            carbonStatus.SetActive(false);
        }
    }

    private void CheckReaction()
    {
        if(hasOxygen && hasCarbon)
        {
            hasOxygen = false;
            hasCarbon = false;
            // CO2/Heat - Single target (overheats the bots/ heals plants)
            // bot = damage 2x
            // plant = heal, spawn area oxygen and add status
            if(isInsect)
            {
                dummyHealth.TakeDamage(latestReactDmg);
            }
            else if(isPlant)
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.CO2, transform);
                Instantiate(areaOxygen, transform.position, Quaternion.identity);
                dummyHealth.Heal(latestReactDmg / 2);
            }
            else if(isRobot)
            {
                dummyHealth.TakeDamage(latestReactDmg * 2);
            }

            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            GameObject CO2label = Instantiate(CO2canvas, newPos, Quaternion.identity);
            Rigidbody _rb = CO2label.GetComponent<Rigidbody>();
            _rb.AddForce(transform.up * 50f * 10f);

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 3)
            {
                TutorialManager.Instance.useCO2 = true;
            }
        }

        if(hasHydrogen && hasOxygen)
        {
            hasHydrogen = false;
            hasOxygen = false;
            // H2O/Water - AOE Explosion of Water (stuns bots by shortcircuit / does nothing to plants)
            // bot = 5sec stun w/ small damage
            // plants = immune
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.H2O, transform);
            if(isInsect)
            {
                Instantiate(H2O, transform.position, Quaternion.identity);
                dummyHealth.TakeDamage(latestReactDmg);
            }
            else if(isPlant)
            {
                Instantiate(H2O, transform.position, Quaternion.identity);
            }
            else if(isRobot)
            {
                Instantiate(H2O, transform.position, Quaternion.identity);
                dummyHealth.TakeDamage(latestReactDmg / 2);
            }

            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            GameObject H2Olabel = Instantiate(H2Ocanvas, newPos, Quaternion.identity);
            Rigidbody _rb = H2Olabel.GetComponent<Rigidbody>();
            _rb.AddForce(transform.up * 50f * 10f);

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 3)
            {
                TutorialManager.Instance.useH2O = true;
            }
        }

        if(hasNitrogen && hasOxygen)
        {
            hasNitrogen = false;
            hasOxygen = false;
            // NO/Debuff - AOE Nitric Oxide Cloud (decreases defense of both bots and plants)
            // 5sec leave circle small dot and debuff
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NO, transform);
            if(isInsect)
            {
                Instantiate(NO, transform.position, Quaternion.identity);
            }
            else if(isPlant)
            {
                Instantiate(NO, transform.position, Quaternion.identity);
            }
            else if(isRobot)
            {
                Instantiate(NO, transform.position, Quaternion.identity);
            }

            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            GameObject NOlabel = Instantiate(NOcanvas, newPos, Quaternion.identity);
            Rigidbody _rb = NOlabel.GetComponent<Rigidbody>();
            _rb.AddForce(transform.up * 50f * 10f);

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 3)
            {
                TutorialManager.Instance.useNO = true;
            }
        }

        if(hasNitrogen && hasHydrogen)
        {
            hasNitrogen = false;
            hasHydrogen = false;
            // NH3/Poison - Single target poison (does nothing on bots/ poisons plants)
            // bot = immune
            // plants = 5sec dot 
            if(isInsect)
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NH3, transform);
                StartCoroutine(ApplyDOT(5f, dotCooldown, latestReactDmg * 0.2f));

                Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
                GameObject NH3label = Instantiate(NH3canvas, newPos, Quaternion.identity);
                Rigidbody _rb = NH3label.GetComponent<Rigidbody>();
                _rb.AddForce(transform.up * 50f * 10f);
            }
            else if(isPlant)
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.NH3, transform);
                StartCoroutine(ApplyDOT(5f, dotCooldown, latestReactDmg * 0.3f));

                Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
                GameObject NH3label = Instantiate(NH3canvas, newPos, Quaternion.identity);
                Rigidbody _rb = NH3label.GetComponent<Rigidbody>();
                _rb.AddForce(transform.up * 50f * 10f);
            }
            else if(isRobot)
            {
                
            }

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 3)
            {
                TutorialManager.Instance.useNH3 = true;
            }
        }

        if(hasCarbon && hasHydrogen)
        {
            hasCarbon = false;
            hasHydrogen = false;
            // CH4/Explosive - AOE Explosive (good for both bots and plants)
            // bots = raw dmg
            // plants = raw dmg, 5sec dot afterwards
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.CH4, transform);
            if(isInsect)
            {
                Instantiate(CH4, transform.position, Quaternion.identity);
                dummyHealth.TakeDamage(latestReactDmg);
            }
            else if(isPlant)
            {
                Instantiate(CH4, transform.position, Quaternion.identity);
                dummyHealth.TakeDamage(latestReactDmg);
                StartCoroutine(ApplyDOT(5f, dotCooldown, 2));
            }
            else if(isRobot)
            {
                Instantiate(CH4, transform.position, Quaternion.identity);
                dummyHealth.TakeDamage(latestReactDmg);
            }

            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            GameObject CH4label = Instantiate(CH4canvas, newPos, Quaternion.identity);
            Rigidbody _rb = CH4label.GetComponent<Rigidbody>();
            _rb.AddForce(transform.up * 50f * 10f);

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 3)
            {
                TutorialManager.Instance.useCH4 = true;
            }
        }

        if(hasCarbon && hasNitrogen)
        {
            hasCarbon = false;
            hasNitrogen = false;
            // CN-/Cyanide Cores > applying hydrogen detonates cores AOE burst of Hydrogen Cyanide gas 
            // (does damage to everything)
            if(isInsect)
            {
                Instantiate(cyanideCore, transform.position, Quaternion.identity);
            }
            else if(isPlant)
            {
                Instantiate(cyanideCore, transform.position, Quaternion.identity);
            }
            else if(isRobot)
            {
                Instantiate(cyanideCore, transform.position, Quaternion.identity);
            }

            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            GameObject CNlabel = Instantiate(CNcanvas, newPos, Quaternion.identity);
            Rigidbody _rb = CNlabel.GetComponent<Rigidbody>();
            _rb.AddForce(transform.up * 50f * 10f);

            if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 3)
            {
                TutorialManager.Instance.useCN = true;
            }
        }
    }

    private bool StatusCooldown()
    {
        cooldownTimer -= Time.deltaTime;
        if(cooldownTimer <= 0)
        {
            cooldownTimer = statusCooldown;
            return false;
        }
        return true;
    }

    private IEnumerator ApplyDOT(float duration, float damageInterval, float damagePerInterval)
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            poisonStatus.SetActive(true);
            dummyHealth.TakeDamage(damagePerInterval);
            yield return new WaitForSeconds(damageInterval);
        }
        poisonStatus.SetActive(false);
    }
}