using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        Time.timeScale = 1f;
        loadingScreen.SetActive(false);
    }

    public void LoadScene(string sceneToLoad)
    {
        loadingScreen.SetActive(true);

        StartCoroutine(LoadSceneASync(sceneToLoad));
    }

    IEnumerator LoadSceneASync(string sceneToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        
        loadOperation.allowSceneActivation = false;
        yield return new WaitForSeconds(1f);
        
        while(!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingText.text = progressValue * 100f + "%";

            yield return null;

            if (loadOperation.progress >= 0.9f)
            {
                loadOperation.allowSceneActivation = true;
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
