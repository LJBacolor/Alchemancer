using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject pauseLocalPanel;
    [SerializeField] private GameObject pauseRelayHostPanel;
    [SerializeField] private GameObject pauseRelayClientPanel;

    private PlayerInput playerInput;
    private InputAction pauseAction;

    private bool isPaused = false;
    public bool isInteracted = false;
    public bool deadTrigger = false;

    private void Awake()
    {
        if(Instance != null) Destroy(this);
        else Instance = this;

        Instance = this;
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindActionMap("Inputs").Enable();
        pauseAction = playerInput.actions["Escape"];
    }

    private void Start()
    {
        if (!IsClient)
        {
            HostManager.Instance.StartHost();
        }

        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerPointer>().ClearTarget();

        if (PlayerProgress.tutorialFinished)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().MoveToSpawn();
        }
    }

    private void Update()
    {
        if(pauseAction.triggered && !isInteracted)
        {
            if(!isPaused)
            {
                isPaused = true;

                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canMove = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canDash = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerAttack>().enabled = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEquips>().enabled = false;
                NetworkManager.LocalClient.PlayerObject.GetComponent<IsoAim>().enabled = false;

                if (IsHost)
                {
                    if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
                    {
                        Time.timeScale = 0;
                        pauseRelayClientPanel.SetActive(false);
                        pauseRelayHostPanel.SetActive(false);
                        pauseLocalPanel.SetActive(true);
                    }
                    else
                    {
                        pauseLocalPanel.SetActive(false);
                        pauseRelayClientPanel.SetActive(false);
                        pauseRelayHostPanel.SetActive(true);
                    }
                }
                else
                {
                    pauseLocalPanel.SetActive(false);
                    pauseRelayHostPanel.SetActive(false);
                    pauseRelayClientPanel.SetActive(true);
                }
            }
            else
            {
                isPaused = false;

                pauseLocalPanel.SetActive(false);
                pauseRelayClientPanel.SetActive(false);
                pauseRelayHostPanel.SetActive(false);

                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canMove = true;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canDash = true;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerAttack>().enabled = true;
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEquips>().enabled = true;
                NetworkManager.LocalClient.PlayerObject.GetComponent<IsoAim>().enabled = true;

                if (IsHost)
                {
                    if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
                    {
                        Time.timeScale = 1;

                    }
                }
            }
        }

        if (IsServer && SceneManager.GetActiveScene().name != "Lobby")
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            bool isAllDead = true;

            foreach (GameObject player in players)
            {
                if(player.GetComponent<PlayerHealth>() != null)
                {
                    if (!player.GetComponent<PlayerHealth>().isDead.Value)
                    {
                        isAllDead = false;
                    }
                }
            }

            if (isAllDead && PlayerStats.currentXtraLife <= 0 && !deadTrigger)
            {
                deadTrigger = true;
                
                BlackoutClientRpc();
                Invoke("BackToLobby", 3f);
            }
        }
    }

    [ClientRpc]
    private void BlackoutClientRpc()
    {
        GameObject blackoutScreen = GameObject.Find("Blackout Screen");
        Animation blackoutAnim = blackoutScreen.GetComponent<Animation>();
        blackoutAnim.Play();
    }

    public void BackToLobby()
    {
        if(IsServer)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject enemy in enemies)
            {
                enemy.GetComponent<NetworkObject>().Despawn();
            }

            GameObject[] explosives = GameObject.FindGameObjectsWithTag("Explosive");
            foreach(GameObject explosive in explosives)
            {
                explosive.GetComponent<NetworkObject>().Despawn();
            }

            NetworkManager.Singleton.SceneManager.LoadScene("Lobby",LoadSceneMode.Single);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Leave()
    {
        pauseRelayClientPanel.gameObject.SetActive(false);
        pauseRelayHostPanel.gameObject.SetActive(false);
        HostManager.Instance.StartHostLocal();
    }

    public void ReturnToLobby()
    {
        if (IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
            {
                Time.timeScale = 1;

            }
        }
        BlackoutClientRpc();
        Invoke("BackToLobby", 3f);
    }
}
