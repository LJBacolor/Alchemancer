using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShield : NetworkBehaviour
{
    public static PlayerShield Instance;
    
    private MeshRenderer meshRenderer;
    private float startTimer = 0f;
    public float shieldCooldown = 20f;
    public bool canShield = true;

    private void Awake() 
    {
        Instance = this;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if(!canShield)
        {
            if(Time.time - startTimer >= shieldCooldown)
            {
                canShield = true;
            }
        }
    }

    public IEnumerator ActivateShield()
    {
        canShield = false;
        startTimer = Time.time;
        meshRenderer.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meshRenderer.enabled = false;

    }
}