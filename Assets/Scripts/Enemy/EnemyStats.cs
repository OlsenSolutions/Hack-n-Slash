using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyStats_SO enemyDefinition;

    public GameObject HealthBar;
    public GameObject ManaBar;

    private void Update()
    {
        UpdateHealthMana();
    }

    private void UpdateHealthMana()
    {
        HealthBar.transform.localScale = new Vector3(GetHealth(), HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
        ManaBar.transform.localScale = new Vector3(GetMana(), ManaBar.transform.localScale.y, ManaBar.transform.localScale.z);

    }

    public void ApplyHealth(int healthAmount)
    {
        enemyDefinition.ApplyHealth(healthAmount);
    }

    public void ApplyMana(int manaAmount)
    {
        enemyDefinition.ApplyMana(manaAmount);
    }

    public void TakeDamage(int amount)
    {
        enemyDefinition.TakeDamage(amount);
    }

    public void TakeMana(int amount)
    {
        enemyDefinition.TakeMana(amount);
    }

    public float GetHealth()
    {
        return enemyDefinition.currentHealth / enemyDefinition.maxHealth;
    }

    public float GetMana()
    {
        return enemyDefinition.currentMana / enemyDefinition.maxMana;
    }


}