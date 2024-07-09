using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;

public class DaisyAI : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private NetworkAnimator netAnim;
    [SerializeField] private float attackRange = 10;
    [SerializeField] private bool Gizmos_Bool;
    [SerializeField] private DaisyBossEnemiesSpawn daisyBossEnemiesSpawn;
    [SerializeField] private MeshRenderer shield; 
    private EnemyHealth enemyHealth;
    private Transform player;
    public bool onShield = false;
    public bool onShieldMusic = false;
    [NonSerialized] public bool canAttack = false;
    [NonSerialized] public bool firstPhase = false;
    [NonSerialized] public bool secondPhase = false;

    private void Update()
    {
        if (!IsServer) return;

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (!gameManager.deadTrigger)
        {
            canAttack = true;
        }
        else
        {  
            canAttack = false;
        }

        if(onShield) shield.enabled = true;
        else shield.enabled = false;
    }

    private void OnDrawGizmos()
    {
        if (Gizmos_Bool == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    public void testPhase()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        if (!firstPhase)
        {
            if(enemyHealth.health.Value <= (enemyHealth.maxHealth.Value * 0.66f) && enemyHealth.health.Value > (enemyHealth.maxHealth.Value * 0.33f))
            {
                onShield = true;
                firstPhase = true;
                daisyBossEnemiesSpawn.SpawnFirstWaveServerRpc();
                if(!onShieldMusic)
                {
                    onShieldMusic = true;
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.daisyOnShield, transform);
                }
            }
        }
        else
        {
            if(onShield)
            {
                enemyHealth.health.Value = enemyHealth.maxHealth.Value * 0.66f;
            }
        }

        if(!secondPhase)
        {
            if(enemyHealth.health.Value <= (enemyHealth.maxHealth.Value * 0.33f))
            {
                onShield = true;
                secondPhase = true;
                daisyBossEnemiesSpawn.SpawnSecondWaveServerRpc();

                if(!onShieldMusic)
                {
                    onShieldMusic = true;
                    SFXManager.Instance.PlaySFXClip(SFXManager.Instance.daisyOnShield, transform);
                }
            }
        }
        else
        {
            if(onShield)
            {
                enemyHealth.health.Value = enemyHealth.maxHealth.Value * 0.33f;
            }
        }
    }

    public void TriggerAttackAnim()
    {
        netAnim.SetTrigger("Attack");
    }
}