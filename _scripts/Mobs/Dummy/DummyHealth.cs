using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class DummyHealth : NetworkBehaviour
{
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private Image healthBar;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private Canvas hpPopup;
    [SerializeField] private float defense = 30;
    private NetworkVariable<float> health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private float minHealth = 0f;
    private Camera cam;
    private Transform player;
    private float rotationSpeed = 10f;

    private void Awake()
    {
        health.Value = maxHealth;
        cam = Camera.main;
    }

    private void Update()
    {
        UpdateHealthBar();

        if (!IsHost) return;
        HealthLimiter();
        IsDead();
    }

    public void TakeDamage(float damage)
    {
        if(IsServer)
        TakeDamageServerRpc(damage);
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        if (health.Value > minHealth)
        {
            health.Value -= damage * (1 - (defense / 300));

            if(damage > 0)
            {
                HPpopClientRpc(damage);
            }
        }
    }

    [ClientRpc]
    public void HPpopClientRpc(float damage)
    {
        Vector3 offset = new Vector3(1f, 2f, 0.1f);
        Canvas hpPop = Instantiate(hpPopup, transform.position + offset, Quaternion.identity);
        TMP_Text hpPopText = hpPop.GetComponentInChildren<TMP_Text>();
        int dmg = Mathf.RoundToInt(damage);
        hpPopText.text = dmg.ToString();

        hpPop.transform.LookAt(hpPop.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        Vector3 sideOffset = new Vector3(Random.Range(-0.5f,0.5f), 0f, 0f);
        Rigidbody hpPopRb = hpPop.gameObject.GetComponent<Rigidbody>();
        hpPopRb.AddForce((transform.up + sideOffset) * 150f);

        Destroy(hpPop, 0.5f);
    }
    
    public void Heal(float amount)
    {
        if(IsServer)
            health.Value += amount;
    }

    public void UpdateHealthBar()
    {
        healthCanvas.transform.LookAt(healthCanvas.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        healthBar.fillAmount = health.Value / maxHealth;
    }

    private void HealthLimiter()
    {
        health.Value = Mathf.Clamp(health.Value, minHealth, maxHealth);
    }

    private void IsDead()
    {
        if(health.Value <= 0)
        {
            health.Value = maxHealth;
        }
    }
}