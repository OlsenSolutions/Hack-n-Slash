using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipsWindow : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           if(this.gameObject.activeSelf)
           {
               this.gameObject.SetActive(false);
           }

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        }
    }


}
