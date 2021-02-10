using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Intro : MonoBehaviour
{
    private VideoPlayer VP;
    void Start()
    {
        VP = GetComponent<VideoPlayer>();
        InvokeRepeating("checkOver", .1f, .1f);

    }
    private void checkOver()
    {
        long playerCurrentFrame = VP.frame;
        long playerFrameCount = Convert.ToInt64(VP.GetComponent<VideoPlayer>().frameCount);

        if (playerCurrentFrame < playerFrameCount-1)
        {

        }
        else
        {
            SceneManager.LoadScene("MainMenu");

        }
    }
}
