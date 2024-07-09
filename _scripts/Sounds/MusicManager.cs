using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : NetworkBehaviour
{
    public static MusicManager instance;

    [Header("Music Audio Clips")]
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip lobbyClip;
    [SerializeField] private AudioClip sector1Clip;
    [SerializeField] private AudioClip sector2Clip;
    [SerializeField] private AudioClip sector3Clip;
    [SerializeField] private AudioClip boss1Clip;
    [SerializeField] private AudioClip boss2Clip;
    [SerializeField] private AudioClip boss3Clip;
    private AudioClip clipToPlay;
    private AudioSource musicSource;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        musicSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        // Unregister the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Initial scene setup
        HandleSceneChange(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Handle the scene change
        HandleSceneChange(scene.name);
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        HandleSceneChange(sceneName);
    }

    private void HandleSceneChange(string sceneName)
    {
        switch (sceneName)
        {
            case "Menu":
                clipToPlay = menuClip;
                break;

            case "Intro":
                clipToPlay = introClip;
                break;

            case "Lobby":
                clipToPlay = lobbyClip;
                break;

            case "Boss_1":
                clipToPlay = boss1Clip;
                break;
            
            case "Boss_2":
                clipToPlay = boss2Clip;
                break;
            
            case "Boss_3":
                clipToPlay = boss3Clip;
                break;

            default:
                if(sceneName.Contains("S1") && PlayerProgress.currentLevel == 1)
                {
                    clipToPlay = sector1Clip;
                }
                else if(sceneName.Contains("S2") && PlayerProgress.currentLevel == 1)
                {
                    clipToPlay = sector2Clip;
                }
                else if(sceneName.Contains("S3") && PlayerProgress.currentLevel == 1)
                {
                    clipToPlay = sector3Clip;
                }
                break;
        }

        if (musicSource.clip != clipToPlay)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
        }
    }
}
