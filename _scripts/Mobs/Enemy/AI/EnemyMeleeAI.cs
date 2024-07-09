using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMeleeAI : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private float attackRange = 5;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool Gizmos_Bool;
    [HideInInspector] public bool isIdle = true;
    [HideInInspector] public bool isMove = false;
    [HideInInspector] public bool isAttack = false;
    private NavMeshAgent navAgent;
    private GameObject[] players;
    private Transform targetPlayer;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        navAgent = GetComponent<NavMeshAgent>();
        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        players = GameObject.FindGameObjectsWithTag("Player");

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!(NetworkManager.Singleton.ConnectedClientsIds.Count == 1))
        {
            float curDistance = Mathf.Infinity;

            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);

                if (distance < curDistance)
                {
                    targetPlayer = player.transform;
                    curDistance = distance;
                }
            }
        }

        float playerDistance = Vector3.Distance(targetPlayer.position, transform.position);

        Vector3 lookDirection = targetPlayer.position - transform.position;
        lookDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (!gameManager.deadTrigger)
        {
            if (playerDistance <= attackRange)
            {
                navAgent.ResetPath();
                navAgent.velocity = Vector3.zero;
                isAttacking();
            }
            else
            {
                Move();
                isMoving();
            }
        }
    }

    private void Move()
    {
        navAgent.SetDestination(targetPlayer.position);
    }

    private void OnDrawGizmos()
    {
        if (Gizmos_Bool == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    private void isMoving()
    {
        anim.SetBool("Attacking", false);
        anim.SetBool("Move", true);
    }

    private void isAttacking()
    {
        anim.SetBool("Attacking", true);
        anim.SetBool("Move", false);
    }
}