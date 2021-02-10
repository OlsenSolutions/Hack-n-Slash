using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class ClickAction : MonoBehaviour
{

    public LayerMask clickableLayer;
    public Texture2D pointer;
    public Texture2D target;
    public Texture2D doorway;
    public Texture2D combat;
    public EventVector3 OnClickEnvironment;

    void Start()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }


    void Update()
    {
        CursorBehaviour();
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0f;
        }
        else if (!pause)
        {
            Time.timeScale = 1;
        }
    }

    public void Exit()
    {
        Application.Quit();

    }

    void CursorBehaviour()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 500, clickableLayer.value))
        {
            if (hit.collider.gameObject.tag == "Enemy" || hit.collider.gameObject.tag == "EnemyWeapon")
            {
                Cursor.SetCursor(combat, new Vector2(16, 16), CursorMode.Auto);
            }

            else if (hit.collider.gameObject.tag == "BoxCollective")
            {
                Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
            }

            else
            {
                Cursor.SetCursor(pointer, new Vector2(16, 16), CursorMode.Auto);
            }
        }
        else
        {
            Cursor.SetCursor(pointer, Vector2.zero, CursorMode.Auto);
        }
    }

    [System.Serializable]
    public class EventVector3 : UnityEvent<Vector3> { }

}