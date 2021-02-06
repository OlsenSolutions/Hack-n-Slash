using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStats", menuName = "Character/Stats", order = 1)]
public class CharacterStats_SO : ScriptableObject
{
    [System.Serializable]
    public class CharLevelUps
    {
        public int maxHealth;
        public int maxMana;
        public int currentDamage;
        public float experienceNeeded;
        public int charRenevalPoints;
        public int currentMagicDamage;
    }
    public bool setManually = false;
    public bool saveDataOnClose = false;

    public ItemPickUp misc1 { get; private set; }
    public ItemPickUp misc2 { get; private set; }

    public float maxHealth = 0;
    public float currentHealth = 0;

    public float maxMana = 0;
    public float currentMana = 0;

    public int currentDamage = 0;
    public int currentMagicDamage = 0;

    public float maxEncumbrance = 0f;
    public float currentEncumbrance = 0f;

    public float charExperience = 0;
    public int charLevel = 0;

    public int charRenevalPoints = 0;

    public CharLevelUps[] charLevelUps;

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

    public void ApplyExperience(int expAmount)
    {

        charExperience += expAmount;

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

    public void LevelUp()
    {
        charLevel += 1;
        maxHealth = charLevelUps[charLevel - 1].maxHealth;
        maxMana = charLevelUps[charLevel - 1].maxMana;
        currentDamage = charLevelUps[charLevel - 1].currentDamage;
        currentMagicDamage = charLevelUps[charLevel - 1].currentMagicDamage;
        charRenevalPoints = charLevelUps[charLevel - 1].charRenevalPoints;
        currentHealth = maxHealth;
        currentMana = maxMana;


    }

    public void saveCharacterData()
    {
        saveDataOnClose = true;
        //EditorUtility.SetDirty(this);
    }


}
