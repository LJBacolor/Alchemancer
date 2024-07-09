using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuffBox : NetworkBehaviour
{
    [SerializeField] public GameObject box;
    
    [SerializeField] private float playerDetectionRadius = 1f;
    [SerializeField] private GameObject Ecmp;
    [SerializeField] private bool Gizmos_Bool;

    private GameObject player;
    private Camera cam;
    private PlayerInput playerInput;
    private InputAction interactAction;

    private void Awake()
    {
        cam = Camera.main;

        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindActionMap("Inputs").Enable();
        interactAction = playerInput.actions["Interact"];
    }

    private void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            player = NetworkManager.LocalClient.PlayerObject.gameObject;
        }

        float playerDistance = Vector3.Distance(player.transform.position, transform.position);

        Ecmp.transform.LookAt(Ecmp.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        if(playerDistance <= playerDetectionRadius)
        {
            Ecmp.SetActive(true);

            if(interactAction.triggered)
            {
                player.GetComponent<BuffsManager>().OpenBuff();
                Destroy(gameObject);
            }
        }
        else
        {
            Ecmp.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (Gizmos_Bool == true)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
}
