using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyJumpAI : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private NetworkAnimator netAnim;
    [SerializeField] private float attackRange = 5;
    [SerializeField] private float limitRange = 4;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool Gizmos_Bool;
    [HideInInspector] public bool isIdle = true;
    [HideInInspector] public bool isMove = false;
    [HideInInspector] public bool isAttack = false;
    [SerializeField] private LayerMask layerMask;
    private float jumpCooldown = 5f;
    private float currentJumpCooldown = 0f;
    private Rigidbody rb;
    private Collider col;
    private NavMeshAgent navAgent;
    private Transform targetPlayer;
    private GameObject[] players;
    private bool grounded = true;
    private bool isFollowing = false;
    private bool isFalling = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        navAgent = GetComponent<NavMeshAgent>();

        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        players = GameObject.FindGameObjectsWithTag("Player");

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (currentJumpCooldown > 0f)
        {
            currentJumpCooldown -= Time.deltaTime;
        }

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

        if(GameObject.FindGameObjectWithTag("Player"))
        {
            if (playerDistance <= attackRange && playerDistance >= limitRange)
            {
                navAgent.ResetPath();
                isAttacking();

                if (currentJumpCooldown <= 0f && grounded)
                {
                    Jump();
                }
            }
            else if (playerDistance >= attackRange && grounded)
            {
                rb.velocity = Vector3.zero;
                Move();
            }
            else if(playerDistance <= limitRange && grounded)
            {
                MoveBack();
            }

            if(navAgent.velocity.magnitude >= 0.1f)
            {
                isMoving();
            }
            else
            {
                isIdling();
            }

            if(isFollowing)
            {
                transform.position = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
            }

            if (isFalling)
            {
                rb.AddForce(Vector3.down * 20);
            }
        }
        else
        {
            Invoke("Reset", 0.5f);
        }
    }

    private void Reset()
    {
        navAgent.ResetPath();
        rb.velocity = Vector3.zero;
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

    private void Jump()
    {
        TriggerAttackAnim();
        IgnoreCollision();
        
        grounded = false;
        if (navAgent.enabled)
        {
            // disable the navAgent
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
            navAgent.isStopped = true;
        }
        // make the jump
        SFXManager.Instance.PlaySFXClip(SFXManager.Instance.insectJumpAttack, transform);

        rb.useGravity = true;
        rb.AddForce((transform.forward + Vector3.up * 20) * jumpForce);
        
        StartCoroutine(Follow());
    }

    IEnumerator Follow()
    {
        yield return new WaitForSeconds(1f);
        col.isTrigger = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        isFollowing = true;

        yield return new WaitForSeconds(3);
        col.isTrigger = false;
        rb.useGravity = true;
        isFalling = true;
        isFollowing = false;

        currentJumpCooldown = jumpCooldown;
    }

    private void IgnoreCollision()
    {
        Physics.IgnoreLayerCollision(6, 7, true);
    }

    private void ResetCollision()
    {
        Physics.IgnoreLayerCollision(6, 7, false);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.collider != null && col.collider.tag == "Ground")
        {
            if (!grounded)
            {
                if (navAgent.enabled)
                {
                    navAgent.updatePosition = true;
                    navAgent.updateRotation = true;
                    navAgent.isStopped = false;
                }
                rb.useGravity = false;
                grounded = true;
                isFalling = false;
                ResetCollision();
                transform.position = col.contacts[0].point;
            }
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

    public void TriggerAttackAnim()
    {
        netAnim.SetTrigger("Attack");
    }

    private void isIdling()
    {
        anim.SetBool("Idle", true);
        anim.SetBool("Move", false);
    }

    private void isMoving()
    {
        anim.SetBool("Idle", false);
        anim.SetBool("Move", true);

    }

    private void isAttacking()
    {
        isIdle = false;
        isMove = false;
        isAttack = true;
    }
}