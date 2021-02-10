using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    public GameObject tipCanvas;
    public GameObject nextTrigger;

    public GameObject activeNext;

    public GameObject platform;
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            tipCanvas.SetActive(true);
            if (nextTrigger != null)
                nextTrigger.SetActive(true);
            if (activeNext != null)
                activeNext.SetActive(true);
            if (platform != null)
                platform.GetComponent<Platform>().AllEnemiesAreDead();
            this.gameObject.SetActive(false);
            Time.timeScale = 0f;
        }
    }

}
