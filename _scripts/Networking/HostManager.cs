using System;
using System.Collections;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;

public class HostManager : NetworkBehaviour
{
    public static HostManager Instance { get; private set; }


    [SerializeField] private int maxConnections = 2;

    public string joinCode { get; private set; }

    public bool isAuthenticated;

    public bool isRelay = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        DontDestroyOnLoad(gameObject);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        Debug.Log("Approval Checking");

        if (NetworkManager.Singleton.ConnectedClients.Count >= maxConnections+1)
        {
            response.Approved = false;
            response.Reason = "Server is Full";
        }
        else if (sceneName != "Lobby")
        {
            response.Approved = false;
            response.Reason = "Game Ongoing";
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (!IsServer && NetworkManager.DisconnectReason != string.Empty)
        {
            Debug.Log($"Approval Declined Reason: {NetworkManager.DisconnectReason}");
            StartHostLocal();
        }
        else if(clientId == NetworkManager.ServerClientId)
        {
            NetworkManager.Singleton.Shutdown();
            HostManager.Instance.isRelay = false;
            SceneManager.LoadScene("Lobby");
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartHostLocal()
    {
        joinCode = null;

        ToggleNPCs(false);

        StartCoroutine(DelayStartHostLocal());
        isRelay = false;
    }

    IEnumerator DelayStartHostLocal()
    {
        ToggleNPCs(false);
        NetworkManager.Singleton.Shutdown();

        yield return new WaitUntil(() => !NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");

        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;

        NetworkManager.Singleton.StartHost();

        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient);

        ToggleNPCs(true);
    }

    public async void StartHostRelay()
    {
        Allocation allocation = null;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        var relayServerData = new RelayServerData(allocation, "dtls");

        StartCoroutine(DelayStartHostRelay(relayServerData));
        isRelay = true;
    }

    private IEnumerator DelayStartHostRelay(RelayServerData relayServerData)
    {
        ToggleNPCs(false);
        NetworkManager.Singleton.Shutdown();

        yield return new WaitUntil(() => !NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        NetworkManager.Singleton.StartHost();

        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient);

        ToggleNPCs(true);
    }

    private void ToggleNPCs(bool Toggle)
    {
        NPCInteract[] NPCObjects = GameObject.FindObjectsOfType<NPCInteract>();

        foreach (NPCInteract npc in NPCObjects)
        {
            npc.enabled = Toggle;
        }
    }
}
