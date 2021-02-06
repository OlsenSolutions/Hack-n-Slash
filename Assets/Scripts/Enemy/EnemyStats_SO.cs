using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStats", menuName = "EnemyStats", order = 1)]
public class EnemyStats_SO : ScriptableObject
{
    public float maxHealth = 0;
    public float currentHealth = 0;
    public float maxMana = 0;
    public float currentMana = 0;
    public int currentDamage = 0;
    public float normalSpeed;
    public int experienceAdded;

    public void ApplyHealth(int healthAmount)
    {
        if ((currentHealth + healthAmount) > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += healthAmount;
        }
    }

    public void ApplyMana(int manaAmount)
    {
        if ((currentMana + manaAmount) > maxMana)
        {
            currentMana = maxMana;
        }
        else
        {
            currentMana += manaAmount;
        }
    }

   
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    public void TakeMana(int amount)
    {
        currentMana -= amount;
        
        if (currentMana < 0)
        {
            currentMana = 0;
        }
    }

    private void Death()
    {
    }
}
