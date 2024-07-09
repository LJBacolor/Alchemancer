using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDashAI : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private NetworkAnimator netAnim;
    [SerializeField] private float attackRange = 5;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private bool Gizmos_Bool;
    [HideInInspector] public bool isIdle = true;
    [HideInInspector] public bool isMove = false;
    [HideInInspector] public bool isAttack = false;
    private float dashCooldown = 3f;
    private float currentDashCooldown = 0f;
    private Rigidbody rb;
    private NavMeshAgent navAgent;
    private GameObject[] players;
    private Transform targetPlayer;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();

        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        players = GameObject.FindGameObjectsWithTag("Player");

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (currentDashCooldown > 0f)
        {
            currentDashCooldown -= Time.deltaTime;
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
            if (playerDistance <= attackRange)
            {
                navAgent.ResetPath();
                isAttacking();

                if (currentDashCooldown <= 0f)
                {
                    StartCoroutine(Dash());
                }
            }
            else
            {
                rb.velocity = Vector3.zero;
                Move();

                if (navAgent.velocity.magnitude == 0f)
                {
                    isIdling();
                }
                else if (navAgent.velocity.magnitude >= 0.1f)
                {
                    isMoving();
                }
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

    private IEnumerator Dash()
    {
        yield return new WaitForSeconds(0.5f);

        Vector3 dashDirection = (targetPlayer.position - transform.position).normalized;

        rb.AddForce(dashDirection * 20, ForceMode.Impulse);
        TriggerAttackAnim();
        if(GetComponent<EnemyStatus>().imInsect) SFXManager.Instance.PlaySFXClip(SFXManager.Instance.insectDashAttack, transform);
        else SFXManager.Instance.PlaySFXClip(SFXManager.Instance.robotDashAttack, transform);
        

        IgnoreCollision();
        Invoke("ResetCollision", 1f);

        currentDashCooldown = dashCooldown;
        StopAllCoroutines();
    }

    private void IgnoreCollision()
    {
        Physics.IgnoreLayerCollision(6, 7, true);
    }

    private void ResetCollision()
    {
        Physics.IgnoreLayerCollision(6, 7, false);
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
        isIdle = true;
        isMove = false;
        isAttack = false;
    }

    private void isMoving()
    {
        isIdle = false;
        isMove = true;
        isAttack = false;
    }

    private void isAttacking()
    {
        isIdle = false;
        isMove = false;
        isAttack = true;
    }
}