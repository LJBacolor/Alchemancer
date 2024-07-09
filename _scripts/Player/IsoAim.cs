using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class IsoAim : NetworkBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject warningPing;
    [SerializeField] private GameObject attackPing;
    [SerializeField] private GameObject helpPing;
    [SerializeField] private float pingCooldown = 2f;

    Vector3 mousePos;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private InputAction warningPAction;
    private InputAction attackPAction;
    private InputAction helpPAction;
    private float pingCooldownTime;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        // Player Action Map
        warningPAction = playerInput.actions["WarningPing"];
        attackPAction = playerInput.actions["AttackPing"];
        helpPAction = playerInput.actions["HelpPing"];
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
        else
        {
            Aim();
        }

        // Check cooldowns
        bool canUsePing = Time.time >= pingCooldownTime;

        Vector3 spawnPos = new Vector3(mousePos.x, mousePos.y + 0.5f, mousePos.z);

        if (canUsePing && warningPAction.triggered)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.ping, transform);
            PingServerRpc(spawnPos, "Warn");
            pingCooldownTime = Time.time + pingCooldown;
        }
        else if (canUsePing && attackPAction.triggered)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.ping, transform);
            PingServerRpc(spawnPos, "Attack");
            pingCooldownTime = Time.time + pingCooldown;
        }
        else if (canUsePing && helpPAction.triggered)
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.ping, transform);
            PingServerRpc(spawnPos, "Help");
            pingCooldownTime = Time.time + pingCooldown;
        }
    }

    private void Aim()
    {
        mousePos = GetMousePosition();

        // Calculate the direction
        Vector3 direction = mousePos - transform.position;

        // You might want to delete this line.
        // Ignore the height difference.
        direction.y = 0;

        // Make the transform look in the direction.
        transform.forward = direction;
    }

    private Vector3 GetMousePosition()
    {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            // The Raycast hit something, return with the position.
            return hitInfo.point;
        }
        else
        {
            // The Raycast did not hit anything.
            return Vector3.zero;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PingServerRpc(Vector3 spawnPos, string ping)
    {
        PingClientRpc(spawnPos, ping);
    }

    [ClientRpc]
    private void PingClientRpc(Vector3 spawnPos, string ping)
    {
        switch (ping)
        {
            case "Warn":
                Instantiate(warningPing, spawnPos, Quaternion.identity);
            break;
            case "Attack":
                Instantiate(attackPing, spawnPos, Quaternion.identity);
            break;
            case "Help":
                Instantiate(helpPing, spawnPos, Quaternion.identity);
            break;
        }
    }
}