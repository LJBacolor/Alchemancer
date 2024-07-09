using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DaisyAttack : NetworkBehaviour
{
    [SerializeField] private float attackCooldown = 10f;
    [SerializeField] private GameObject surroundAttack;
    private DaisyAI daisyAI;
    private float currentTime = 0;
    private bool isAttacking = false;

    private void Awake()
    {
        daisyAI = GetComponent<DaisyAI>();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (daisyAI.canAttack)
        {
            if (!isAttacking && !daisyAI.onShield)
            {
                StartAttack();
            }
            else if(isAttacking && Time.time >= currentTime + attackCooldown)
            {
                isAttacking = false;
            }
        }
    }

    private void StartAttack()
    {
        // Attack
        daisyAI.TriggerAttackAnim();
        SurroundAttack();
        isAttacking = true;

        currentTime = Time.time;
    }

    private void SurroundAttack()
    {
        GameObject attackObject = Instantiate(surroundAttack, transform.position, Quaternion.identity);
        NetworkObject attackNetworkObject = attackObject.GetComponent<NetworkObject>();
        attackNetworkObject.Spawn();
    }
}
