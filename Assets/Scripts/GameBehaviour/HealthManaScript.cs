using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthManaScript : MonoBehaviour
{
    public Image HealthBar;
    public Image ManaBar;
    public Text Level;
    public Image expBar;
    public GameObject ui;
    public ClickAction click;
    public Text MonsterCounter;
    void DecreaseMonsterCounter()
    {
        var x = int.Parse(MonsterCounter.text);
        x = x - 1;
        if (x == 0)
            EventManager.TriggerEvent(EventManager.AllEnemiesDead);
        MonsterCounter.text = x.ToString();
    }

    void Show()
    {
        if (click != null)
            click.PauseGame(true);

        ui.gameObject.SetActive(true);
    }

    void OnEnable()
    {
        EventManager.StartListening(EventManager.PlayerDead, Show);

        EventManager.StartListening(EventManager.EnemyDead, DecreaseMonsterCounter);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventManager.PlayerDead, Show);

        EventManager.StopListening(EventManager.EnemyDead, DecreaseMonsterCounter);

    }
}
