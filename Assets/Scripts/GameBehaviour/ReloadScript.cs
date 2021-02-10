using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScript : MonoBehaviour
{

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void NotRestart()
    {
        EventManager.TriggerEvent(EventManager.NotRestart);
    }

    public void LoadMainMenu()
    {
        var sceneName = SceneManager.GetActiveScene();
        SceneManager.LoadScene("MainMenu");
        if (sceneName.name == "Training")
            SceneManager.UnloadSceneAsync("Training");
        else if (sceneName.name == "MapGeneration")
            SceneManager.UnloadSceneAsync("MapGeneration");

    }
}
