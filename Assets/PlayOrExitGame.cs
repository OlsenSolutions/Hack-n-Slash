using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayOrExitGame : MonoBehaviour
{
    // Start is called before the first frame update
    public void Play()
    {
        SceneManager.LoadScene("MapGeneration");
    }

    // Update is called once per frame
    public void Exit()
    {
        Application.Quit();

    }
}
