using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Services.Relay;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async void StartClient(string joinCode)
    {
        JoinAllocation allocation = null;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.Log("Relay Join Code Request Failed");
            return;
        }

        var relayServerData = new RelayServerData(allocation, "dtls");

        StartCoroutine(DelayStartClient(relayServerData));
    }

    IEnumerator DelayStartClient(RelayServerData relayServerData)
    {
        ToggleNPCs(false);

        NetworkManager.Singleton.Shutdown();

        yield return new WaitUntil(() => !NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);

        NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        HostManager.Instance.isRelay = false;

        yield return new WaitUntil(() => NetworkManager.Singleton.IsClient);

        ToggleNPCs(true);
    }

    private void ToggleNPCs(bool Toggle)
    {
        GameObject[] NPCObjects = GameObject.FindGameObjectsWithTag("NPC");

        foreach (GameObject npc in NPCObjects)
        {
            npc.GetComponent<NPCInteract>().enabled = Toggle;
        }
    }
}
