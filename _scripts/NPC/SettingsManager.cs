using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;
using Unity.Netcode;

public class SettingsManager : NetworkBehaviour
{
    [Header("Shop Info")]
    [SerializeField] private CinemachineVirtualCamera mainVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera settingsVirtualCamera;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject generalPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject displayPanel;
    [SerializeField] private GameObject resetPanel;
    [SerializeField] private Image generalBtn;
    [SerializeField] private Image audioBtn;
    [SerializeField] private Image displayBtn;
    private bool buttonPressed = false;

    private void Start()
    {
        settingsPanel.SetActive(false);
        GeneralBtn();
    }

    private void Update()
    {
        
    }

    public void OpenSettings()
    {
        mainVirtualCamera = GameObject.Find("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        settingsPanel.SetActive(true);

        mainVirtualCamera.Priority = 0;
        settingsVirtualCamera.Priority = 1;
    }

    public void CloseSettings()
    {
        mainVirtualCamera = GameObject.Find("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();

        settingsPanel.SetActive(false);

        NPCDialogue npcDialogue = GetComponent<NPCDialogue>();
        npcDialogue.EndDialogue();
        npcDialogue.EnablePlayer();

        mainVirtualCamera.Priority = 1;
        settingsVirtualCamera.Priority = 0;

        GeneralBtn();
        DataManager.Instance.SaveGame();
    }

    public void GeneralBtn()
    {
        generalBtn.color = Color.white;
        audioBtn.color = Color.grey;
        displayBtn.color = Color.grey;

        generalPanel.SetActive(true);
        audioPanel.SetActive(false);
        displayPanel.SetActive(false);

        resetPanel.SetActive(false);
    }

    public void AudioBtn()
    {
        generalBtn.color = Color.grey;
        audioBtn.color = Color.white;
        displayBtn.color = Color.grey;

        generalPanel.SetActive(false);
        audioPanel.SetActive(true);
        displayPanel.SetActive(false);

        resetPanel.SetActive(false);
    }

    public void DisplayBtn()
    {
        generalBtn.color = Color.grey;
        audioBtn.color = Color.grey;
        displayBtn.color = Color.white;

        generalPanel.SetActive(false);
        audioPanel.SetActive(false);
        displayPanel.SetActive(true);

        resetPanel.SetActive(false);
    }

    public void ResetBtn()
    {
        generalPanel.SetActive(false);
        resetPanel.SetActive(true);
    }

    public void YesBtn()
    {
        if(!buttonPressed)
        {
            buttonPressed = true;
            Invoke("MovePlayer", 1.5f);
            Invoke("CloseSettings", 2f);
            GameObject blackoutScreen = GameObject.Find("Blackout Screen");
            Animation blackoutAnim = blackoutScreen.GetComponent<Animation>();
            blackoutAnim.Play();
            Invoke("BackToLobby", 3f);
            
        }
    }

    public void NoBtn()
    {
        generalPanel.SetActive(true);
        resetPanel.SetActive(false);
    }

    public void BackToLobby()
    {
        if(IsServer)
        {
            DataManager.Instance.NewGame();

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

    private void MovePlayer()
    {
        GameObject.Find("Player(Clone)").transform.position = GameObject.Find("Tutorial Spawn").transform.position;
    }
}