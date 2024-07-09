using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class CoopManager : NetworkBehaviour
{
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject coopPanel;
    [SerializeField] private GameObject connectionFailedPanel;

    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject inLobbyPanel;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private GameObject lobbyCodePanel;
    [SerializeField] private TextMeshProUGUI lobbyCodeTxt;

    public async void AuthenticateConnection()
    {
        if (!HostManager.Instance.isAuthenticated)
        {
            connectingPanel.SetActive(true);
            connectionFailedPanel.SetActive(false);
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                connectionFailedPanel.SetActive(true);
                connectingPanel.SetActive(false);
                return;
            }
        }

        coopPanel.SetActive(true);
        connectingPanel.SetActive(false);
        HostManager.Instance.isAuthenticated = true;
    }

    public void Host()
    {
        HostManager.Instance.StartHostRelay();
    }

    public void Client()
    {
        ClientManager.Instance.StartClient(joinCodeInputField.text);
        HostManager.Instance.isRelay = true;
    }

    public void Leave()
    {
        HostManager.Instance.StartHostLocal();
    }

    public void CloseCoop()
    {
        NPCDialogue npcDialogue = GetComponent<NPCDialogue>();
        npcDialogue.EndDialogue();
        npcDialogue.EnablePlayer();
    }

    private void Update()
    {
        if (IsHost && HostManager.Instance.isRelay)
        {
            inLobbyPanel.SetActive(true);
            lobbyCodePanel.SetActive(true);
            lobbyPanel.SetActive(false);
            lobbyCodeTxt.text = HostManager.Instance.joinCode;
        }
        else if (IsClient && HostManager.Instance.isRelay)
        {
            inLobbyPanel.SetActive(true);
            lobbyCodePanel.SetActive(false);
            lobbyPanel.SetActive(false);
            lobbyCodeTxt.text = "";
        }
        else
        {
            lobbyPanel.SetActive(true);
            inLobbyPanel.SetActive(false);
            lobbyCodePanel.SetActive(false);
            lobbyCodeTxt.text = "";
        }
    }
}
