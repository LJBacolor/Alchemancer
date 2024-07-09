using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : NetworkBehaviour
{
    [SerializeField] private GameObject enterText;
    [SerializeField] private GameObject blackoutScreen;
    private bool isTriggered = false;

    private void Start()
    {
        enterText.SetActive(true);
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Return) && !isTriggered)
        {
            isTriggered = true;
            blackoutScreen.SetActive(true);
            Invoke("DelayLoadScene", 3f);
        }
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
