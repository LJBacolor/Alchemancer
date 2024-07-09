using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    [SerializeField] private Animation loadingAnim;
    [SerializeField] private string LoadSceneName;

    private Scene currentScene;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private bool isTriggered = false;
    
    private void Awake() 
    {
        currentScene = SceneManager.GetActiveScene();

        playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput.actions["Attack"];
    }

    private void Start()
    {
        //DataManager.Instance.NewGame();
    }

    private void Update()
    {
        if(currentScene.name == "Menu" && attackAction.triggered && !isTriggered)
        {
            isTriggered = true;
            loadingAnim.Play();
            Invoke("Blackout", 3f);
            Invoke("LoadScene", 4f);
        }
    }

    private void Blackout()
    {
        GameObject blackoutScreen = GameObject.Find("Blackout Screen");
        Animation blackoutAnim = blackoutScreen.GetComponent<Animation>();
        blackoutAnim.Play();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(LoadSceneName);
    }
}
