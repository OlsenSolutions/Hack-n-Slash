using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayOrExitGame : MonoBehaviour
{
    public GameObject firstMenu;
    public GameObject secondMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (firstMenu.activeSelf && !secondMenu.activeSelf )
            {
                Exit();
            }
            else if (!firstMenu.activeSelf && secondMenu.activeSelf)
            {
               firstMenu.SetActive(true);
               secondMenu.SetActive(false);
            }
        }

    }
    public void Play()
    {
        SceneManager.LoadScene("MapGeneration");
    }
    public void Exit()
    {
        Application.Quit();

    }

    public void LoadTraining()
    {
        SceneManager.LoadScene("Training");
    }
}
