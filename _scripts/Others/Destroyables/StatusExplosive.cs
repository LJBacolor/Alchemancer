using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StatusExplosive : NetworkBehaviour
{
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject explosionVFX;
    private float maxHealth = 75;
    private NetworkVariable<float> health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private GameObject cam;

    public override void OnNetworkSpawn()
    {
        health.Value = maxHealth;
    }

    private void Update()
    {
        if (cam)
        {
            UpdateHealthBar();
        }
        else
        {
            cam = GameObject.Find("Main Camera");
        }


        if (health.Value <= 0 && IsServer)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.explosion, transform);
            DestroyObjServerRpc();
        }
    }

    [ServerRpc]
    private void DestroyObjServerRpc()
    {
        Instantiate(explosionVFX, transform.position, Quaternion.identity);
        GetComponent<NetworkObject>().Despawn();
    }

    public void TakeDamage(float damage)
    {
        if (IsServer)
            TakeDamageServerRpc(damage);
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        health.Value -= damage;
    }

    public void UpdateHealthBar()
    {
        healthCanvas.SetActive(health.Value != maxHealth);

        healthCanvas.transform.LookAt(healthCanvas.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        healthBar.fillAmount = health.Value / maxHealth;
    }
}
