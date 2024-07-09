using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool initializeDataIfNull = false;

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    public static DataManager Instance {get; private set;}

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        Debug.Log("Create a new data");
        this.gameData = new GameData();
        dataHandler.Save(this.gameData);
        //SaveGame();
    }

    public void LoadGame()
    {
        Debug.Log("Load Game");
        // TODO - Load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();

        // Start a new game if the data is null and we're configured to initialize data for debugging purposes
        if(this.gameData == null && initializeDataIfNull)
        {
            NewGame();
        }

        // If no data can be loaded, don't continue
        if(this.gameData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }

        // TODO - Push the loaded data to all other scripts that need it
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(this.gameData);
        }
    }

    public void SaveGame()
    {
        // If we don't have any data to save, log a warning here
        if(this.gameData == null)
        {
            Debug.LogWarning("No Data was found. A New Game needs to be started before data can be saved.");
            return;
        }

        // TODO - Pass the data to other scripts so they can update it
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(this.gameData);
        }

        // TODO - Save that data to a file using the data handler
        dataHandler.Save(this.gameData);
        Debug.Log("Game Saved!");
    }

    private void OnApplicationQuit() 
    {
        //SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return gameData != null;
    }
}