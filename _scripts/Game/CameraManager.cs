using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CameraManager : NetworkBehaviour
{
    private void Start()
    {
        if (!IsOwner)
        {
            Destroy(gameObject);
        }

        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (IsOwner)
        {
            if(sceneName.Contains("Boss_"))
            {
                gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 15;
            }
            else
            {
                gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 10;
            }
        }
    }

    private void Update()
    {
        if (gameObject.GetComponent<CinemachineVirtualCamera>().Follow == null)
        {
            Destroy(gameObject);
        }
    }
}
