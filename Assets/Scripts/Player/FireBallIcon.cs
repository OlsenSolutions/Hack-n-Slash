using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallIcon : MonoBehaviour
{
    void MagicUsed()
    {
        GetComponent<Animator>().SetTrigger("Use");
    }
    void OnEnable()
    {
        EventManager.StartListening(EventManager.MagicUsed, MagicUsed);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventManager.MagicUsed, MagicUsed);

    }
}
