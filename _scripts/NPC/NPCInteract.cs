using System;
using Cinemachine;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NPCInteract : NetworkBehaviour
{
    [SerializeField] private float playerDetectionRadius = 1f;
    [SerializeField] private bool Gizmos_Bool;
    [SerializeField] private GameObject trigger;
    [SerializeField] private TextMeshProUGUI nameCmp;
    public float cameraSpeed;

    private Camera cam;
    private NPCDialogue dialogue;
    private GameObject targetLook;

    private Quaternion originalRotation;

    [NonSerialized] public bool isInteracting = false;

    private PlayerInput playerInput;
    private InputAction interactAction;

    private void Awake()
    {

        targetLook = GameObject.Find("TargetLook");
        cam = Camera.main;
        dialogue = GetComponent<NPCDialogue>();

        originalRotation = transform.rotation;

        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindActionMap("Inputs").Enable();
        interactAction = playerInput.actions["Interact"];;
    }

    private void Start()
    {
        trigger.GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        if(dialogue.imShop && !IsServer)
        {
            return;
        }
        
        if (isInteracting)
        {
            trigger.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            trigger.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private void RotateTowardsPlayer(GameObject player)
    {
        transform.LookAt(player.transform.position);
    }

    private void RotateTowardsNPC(GameObject player)
    {
        player.transform.LookAt(transform.position);       
    }

    private void OnDrawGizmos()
    {
        if (Gizmos_Bool == true)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            if (dialogue.imShop && !IsServer)
            {
                return;
            }

            if (!col.gameObject.GetComponent<NetworkObject>().IsOwner)
            {
                return;
            }

            if (!isInteracting)
            {
                Debug.Log("Interacted");
                isInteracting = true;
                GameManager.Instance.isInteracted = true;

                Cursor.lockState = CursorLockMode.None;

                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canMove = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canDash = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().healthCanvas.SetActive(false);
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerAttack>().enabled = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEquips>().enabled = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<IsoAim>().enabled = false;

                RotateTowardsPlayer(col.gameObject);
                RotateTowardsNPC(col.gameObject);

                nameCmp.text = gameObject.name;

                dialogue.StartDialogue();
            }
        }
    }
}
