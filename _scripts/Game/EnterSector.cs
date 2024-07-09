using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EnterSector : NetworkBehaviour
{
    [SerializeField] private Animation blackoutAnim;
    private List<int> loadedScenes = new List<int>();
    private bool isReady = false;
    private int playersReady = 0;
    private Scene currentScene;

    private void Awake() 
    {
        currentScene = SceneManager.GetActiveScene();
    }

    private void Start() 
    {
        if(currentScene.name == "Lobby")
        {
            PlayerProgress.currentSector = 0;
            PlayerProgress.currentLevel = 0;
            DataManager.Instance.SaveGame();
        }
    }

    private void OnTriggerEnter(Collider col) 
    {
        if(col.gameObject.tag == "Player" && IsServer)
        {
            playersReady++;

            if (NetworkManager.Singleton.ConnectedClientsIds.Count == playersReady)
            {
                ReadyToLoad();
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player" && IsServer)
        {
            playersReady--;
        }
    }

    private void ReadyToLoad()
    {
        blackoutAnim.Play();

        if (GameObject.FindGameObjectWithTag("Explosive"))
        {
            GameObject[] explosives = GameObject.FindGameObjectsWithTag("Explosive");

            foreach (GameObject explosive in explosives)
            {
                explosive.GetComponent<NetworkObject>().Despawn();
            }
        }

        Invoke("LoadScene", 1f);
    }

    private void LoadScene()
    {
        if(currentScene.name == "Lobby")
        {
            PlayerProgress.currentSector = 1;
            PlayerProgress.currentLevel = 1;

            LoadRandomScene();
        }
        else
        {
            if(PlayerProgress.currentLevel == 4)
            {
                PlayerProgress.currentSector += 1;
                PlayerProgress.currentLevel = 1;
                loadedScenes.Clear();
                LoadRandomScene();
            }
            else if(PlayerProgress.currentLevel == 3)
            {
                PlayerProgress.currentLevel += 1;

                string bossScene = "Boss_" + PlayerProgress.currentSector.ToString();
                NetworkManager.Singleton.SceneManager.LoadScene(bossScene, LoadSceneMode.Single);
            }
            else
            {
                LoadRandomScene();
                PlayerProgress.currentLevel += 1;
            }
        }
    }

    private void LoadRandomScene()
    {
        int randomSceneIndex;
        do
        {
            randomSceneIndex = Random.Range(1, 7);
        }   while (loadedScenes.Contains(randomSceneIndex));

        loadedScenes.Add(randomSceneIndex);
        string sceneName = "S" + PlayerProgress.currentSector.ToString() + "_" + randomSceneIndex.ToString();

        if(sceneName == SceneManager.GetActiveScene().name)
        {
            LoadRandomScene();
        }

        if(IsServer)
        {
            ClearPointerClientRPC();
        }

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }

    [ClientRpc]
    private void ClearPointerClientRPC()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerPointer>().ClearTarget();
    }
}
