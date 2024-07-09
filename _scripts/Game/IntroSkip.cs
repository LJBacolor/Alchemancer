using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class IntroSkip : NetworkBehaviour
{
    [SerializeField] private GameObject blackoutScreen;
    private bool isTriggered = false;

    public void skipIntro()
    {
        isTriggered = true;
        blackoutScreen.SetActive(true);
        Invoke("DelayLoadScene", 3f);
    }

    private void DelayLoadScene()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
