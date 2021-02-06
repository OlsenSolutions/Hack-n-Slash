using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadScript : MonoBehaviour
{

    void Start()
    {
        this.gameObject.SetActive(false);
    }
    public void Reload()
    {
        EventManager.TriggerEvent("NotRestart");
    }
}
