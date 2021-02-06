using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{

    public ParticleSystem nextLevelParticle;
    public ParticleSystem restartGameParticle;
    private bool AllEnemiesDead;
    public GameObject ReloadMapUI;
    private bool CanRestartGame;
    void Start()

    {
        AllEnemiesDead = false;
        CanRestartGame = false;
        StartCounting();
    }

    void StartCounting()
    {
        StartCoroutine("Wait");
    }

    IEnumerator Wait()
    {
        restartGameParticle.Stop();
        CanRestartGame = false;
        yield return new WaitForSeconds(5);
        restartGameParticle.Play();
        CanRestartGame = true;

    }

    void OnTriggerEnter(Collider trigger)
    {
        if (AllEnemiesDead && trigger.gameObject.tag == "Player")
        {
            var MapGeneratorScript = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
            if (MapGeneratorScript != null)
            {
                MapGeneratorScript.GenerateNextMap(trigger.gameObject);
            }
        }
        else if (CanRestartGame && trigger.gameObject.tag == "Player")
        {
            ReloadMapUI.SetActive(true);
            CanRestartGame = false;
            restartGameParticle.Stop();
            var gamemanager = GameObject.FindGameObjectWithTag("GameManager");
            if (gamemanager!=null)
                gamemanager.GetComponent<ClickAction>().PauseGame(true);
        }
    }

    void AllEnemiesAreDead()
    {
        AllEnemiesDead = true;
        restartGameParticle.Stop();
        nextLevelParticle.Play();
    }

    void OnEnable()
    {
        EventManager.StartListening(EventManager.AllEnemiesDead, AllEnemiesAreDead);
        EventManager.StartListening(EventManager.NotRestart, StartCounting);

    }

    void OnDisable()
    {
        EventManager.StopListening(EventManager.AllEnemiesDead, AllEnemiesAreDead);
        EventManager.StopListening(EventManager.NotRestart, StartCounting);

    }

}
