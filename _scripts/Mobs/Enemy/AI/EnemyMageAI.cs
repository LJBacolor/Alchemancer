using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMageAI : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private NetworkAnimator netAnim;
    [SerializeField] private float attackRange = 10;
    [SerializeField] private float limitRange = 8;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private bool Gizmos_Bool;
    [HideInInspector] public bool canShoot = false;
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

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (!gameManager.deadTrigger)
        {
            if (playerDistance <= attackRange && playerDistance >= limitRange)
            {
                navAgent.ResetPath();
                
                canShoot = true;
            }
            else if(playerDistance >= attackRange)
            {
                Move();
                canShoot = false;
            }
            // else if(playerDistance <= limitRange)
            // {
            //     MoveBack();
            //     canShoot = true;
            // }

            if(navAgent.velocity.magnitude >= 0.1f)
            {
                isMoving();
                
                Vector3 lookDirection = targetPlayer.position - transform.position;
                lookDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                isAttacking();
            }
        }
        else
        {  
            canShoot = false;
        }
    }

    private void Move()
    {
        navAgent.SetDestination(targetPlayer.position);
    }

    private void MoveBack()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -transform.forward + Vector3.down * 0.4f;

        if(Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 10, layerMask))
        {
            Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.blue);

            Vector3 hitPos = hit.point;
            navAgent.SetDestination(hitPos);
        }
        else
        {
            Debug.DrawRay(rayOrigin, rayDirection * 10, Color.blue);
        }
    }

    private void OnDrawGizmos()
    {
        if (Gizmos_Bool == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    private void isIdling()
    {
        isIdle = true;
        isMove = false;
        isAttack = false;
    }

    private void isMoving()
    {
        anim.SetBool("Move", true);
        netAnim.SetTrigger("Attack");
    }

    private void isAttacking()
    {
        anim.SetBool("Move", false);
        netAnim.SetTrigger("Attack");
    }
    public Vector3 getTarget()
    {
        return targetPlayer.position;
    }
}