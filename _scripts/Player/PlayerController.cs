using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : NetworkBehaviour
{
    public static PlayerController Instance { get; private set; }
    [SerializeField] public Animator anim;
    [SerializeField] public NetworkAnimator netAnim;
    [SerializeField] public float playerSpeed = 10;
    [SerializeField] private float dashDistance = 4;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float doubleDashCooldown = 0.5f;
    [SerializeField] public bool doubleDashUnlocked = false;

    private PlayerInput playerInput;
    private CharacterController controller;

    [NonSerialized] public Vector2 input;
    private Vector3 move;

    private PlayerEquips playerEquips;

    // Player Action Map
    private InputAction moveAction;
    private InputAction dashAction;

    [NonSerialized] public bool isMoving = false;
    [NonSerialized] public bool isDashing = false;
    private float lastDashTime;

    public bool canDash = true;
    public bool canMove = true;

    private void Awake()
    {
        Instance = this;
        
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        playerEquips = GetComponent<PlayerEquips>();

        // Player Action Map
        moveAction = playerInput.actions["Move"];
        dashAction = playerInput.actions["Dash"];
    }

    private void Start()
    {
        IgnoreCollision();
        canMove = false;

        if(PlayerProgress.tutorialFinished) Invoke("CanMoveTrue", 1f);

        if(!PlayerProgress.tutorialFinished)
        {
            transform.position = GameObject.Find("Tutorial Spawn").transform.position;
        }
        else
        {
            transform.position = GameObject.Find("PlayerSpawn").transform.position;
        }
    }

    public void MoveToSpawn()
    {
        StartCoroutine(ValidateSpawn());
    }

    IEnumerator ValidateSpawn()
    {
        yield return new WaitForSeconds(0.3f);
        GameObject playerSpawn = GameObject.Find("PlayerSpawn");
        transform.position = playerSpawn.transform.position;
        IgnoreCollision();
        canMove = false;
        if (PlayerProgress.tutorialFinished) Invoke("CanMoveTrue", 1f);
        yield return new WaitForSeconds(0.5f);
        if (playerSpawn.transform.position != transform.position)
        {
            transform.position = playerSpawn.transform.position;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        Animations();

        input = moveAction.ReadValue<Vector2>();

        if(canMove)
        {
            move = new Vector3(input.x, 0f, input.y);
        }
        else
        {
            move = Vector3.zero;
        }

        if (!isDashing && canMove)
        {
            controller.Move(move * Time.deltaTime * playerSpeed);
        }

        if(move.magnitude > 0.1f) isMoving = true;
        else    isMoving = false;

        if(dashAction.triggered)
        {
            TryDash();
        }

        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0.95f, 1.05f), transform.position.z);
    }

    private void TryDash()
    {
        if (canDash)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 dashDirection = new Vector3(input.x, 0f, input.y).normalized;

            if (dashDirection.magnitude > 0.1f) // Ensure the player is moving in a direction
            {
                SFXManager.Instance.PlaySFXClip(SFXManager.Instance.dash, transform);

                if (doubleDashUnlocked)
                {
                    if (Time.time - lastDashTime <= doubleDashCooldown)
                    {
                        StartCoroutine(PerformDash(dashDirection, dashCooldown));
                    }
                    else
                    {
                        StartCoroutine(PerformDash(dashDirection, 0f));
                    }

                    lastDashTime = Time.time;
                }
                else
                {
                    StartCoroutine(PerformDash(dashDirection, dashCooldown));
                }

                if(!PlayerProgress.tutorialFinished && TutorialManager.Instance.progress == 1)
                {
                    TutorialManager.Instance.dashes++;
                }
            }
        }
    }

    private IEnumerator PerformDash(Vector3 dashDirection, float cooldown)
    {
        isDashing = true;
        canDash = false;

        if (dashDirection.magnitude > 0.1f) // Ensure the player is moving in a direction
        {
            netAnim.SetTrigger("isDash");
        }

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDirection * Time.deltaTime * (dashDistance / dashDuration));
            yield return null;
        }

        isDashing = false;

        yield return new WaitForSeconds(cooldown);

        canDash = true;
    }

    private void CanMoveTrue()
    {
        canMove = true;
    }

    private void IgnoreCollision()
    {
        //Physics.IgnoreLayerCollision(6, 7);
    }

    private void Animations()
    {

        if(playerEquips.usingNothing.Value)
        {
            playerEquips.AnimReset();

            if(isMoving)
            {
                anim.SetBool("isIdle", false);
                anim.SetBool("isRun", true);
            }
            else
            {
                anim.SetBool("isIdle", true);
                anim.SetBool("isRun", false);
            }
        }
        else
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isRun", false);

            if(!isMoving)
            {
                anim.SetBool("isRunO", false);
                anim.SetBool("isRunH", false);
                anim.SetBool("isRunN", false);
                anim.SetBool("isRunC", false);

                if(playerEquips.usingOxyblastLauncher.Value)
                {
                    anim.SetBool("isIdleO", true);
                    anim.SetBool("isIdleH", false);
                    anim.SetBool("isIdleN", false);
                    anim.SetBool("isIdleC", false);
                }
                else if(playerEquips.usingHydrogenFumeThrower.Value)
                {
                    anim.SetBool("isIdleO", false);
                    anim.SetBool("isIdleH", true);
                    anim.SetBool("isIdleN", false);
                    anim.SetBool("isIdleC", false);
                }
                else if(playerEquips.usingNitrochargedBolts.Value)
                {
                    anim.SetBool("isIdleO", false);
                    anim.SetBool("isIdleH", false);
                    anim.SetBool("isIdleN", true);
                    anim.SetBool("isIdleC", false);
                }
                else if(playerEquips.usingCarbonShurikens.Value)
                {
                    anim.SetBool("isIdleO", false);
                    anim.SetBool("isIdleH", false);
                    anim.SetBool("isIdleN", false);
                    anim.SetBool("isIdleC", true);
                }
            }
            else
            {
                anim.SetBool("isIdleO", false);
                anim.SetBool("isIdleH", false);
                anim.SetBool("isIdleN", false);
                anim.SetBool("isIdleC", false);

                if(playerEquips.usingOxyblastLauncher.Value)
                {
                    anim.SetBool("isRunO", true);
                    anim.SetBool("isRunH", false);
                    anim.SetBool("isRunN", false);
                    anim.SetBool("isRunC", false);
                }
                else if(playerEquips.usingHydrogenFumeThrower.Value)
                {
                    anim.SetBool("isRunO", false);
                    anim.SetBool("isRunH", true);
                    anim.SetBool("isRunN", false);
                    anim.SetBool("isRunC", false);
                }
                else if(playerEquips.usingNitrochargedBolts.Value)
                {
                    anim.SetBool("isRunO", false);
                    anim.SetBool("isRunH", false);
                    anim.SetBool("isRunN", true);
                    anim.SetBool("isRunC", false);
                }
                else if(playerEquips.usingCarbonShurikens.Value)
                {
                    anim.SetBool("isRunO", false);
                    anim.SetBool("isRunH", false);
                    anim.SetBool("isRunN", false);
                    anim.SetBool("isRunC", true);
                }
            }
        }
    }
}
