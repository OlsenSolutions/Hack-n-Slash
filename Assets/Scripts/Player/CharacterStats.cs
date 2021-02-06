using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public CharacterStats_SO characterDefinition;
    public CharacterInventory charInv;
    public HealthManaScript healthManaScript;
    private Animator anim;

     public ParticleSystem healthParticle;
    public ParticleSystem manaParticle;
    public ParticleSystem expParticle;

    public ParticleSystem LevelUpParticle;
    public CharacterStats()
    {
        charInv = CharacterInventory.instance;
    }
    void Start()
    {


        if (!characterDefinition.setManually)
        {
            characterDefinition.maxHealth = 100;
            characterDefinition.currentHealth = 100;

            characterDefinition.maxMana = 25;
            characterDefinition.currentMana = 25;

            characterDefinition.currentDamage = 10;
            characterDefinition.currentMagicDamage = 10;

            characterDefinition.maxEncumbrance = 100f;
            characterDefinition.currentEncumbrance = 0f;

            characterDefinition.charExperience = 0;
            characterDefinition.charLevel = 1;
            characterDefinition.charRenevalPoints = 1;
        }

        healthManaScript = FindObjectOfType<HealthManaScript>();

    }

    private void Update()
    {
        UpdateHealthMana();
        CheckLevel();
    }

    private void CheckLevel()
    {
        if (characterDefinition.charExperience >= characterDefinition.charLevelUps[characterDefinition.charLevel - 1].experienceNeeded)
        {
            LevelUpParticle.Play();
            characterDefinition.LevelUp();

        }
    }

    private void UpdateHealthMana()
    {
        healthManaScript.HealthBar.GetComponent<Image>().fillAmount = GetHealth();
        healthManaScript.ManaBar.GetComponent<Image>().fillAmount = GetMana();
        healthManaScript.expBar.GetComponent<Image>().fillAmount = GetExp();
        healthManaScript.Level.GetComponent<Text>().text = GetLevel().ToString();

    }

    public void ApplyHealth(int healthAmount)
    {
        characterDefinition.ApplyHealth(healthAmount);
    }

    public void ApplyMana(int manaAmount)
    {
        characterDefinition.ApplyMana(manaAmount);
    }

    public void GiveExperience(int charExperience)
    {
        characterDefinition.ApplyExperience(charExperience);
    }

    public void TakeDamage(int amount)
    {
        characterDefinition.TakeDamage(amount);
    }

    public void TakeMana(int amount)
    {
        characterDefinition.TakeMana(amount);
    }

    public float GetHealth()
    {
        float health = (characterDefinition.currentHealth / characterDefinition.maxHealth);
        return health;
    }

    public float GetMana()
    {
        return characterDefinition.currentMana / characterDefinition.maxMana;
    }

    public float GetExp()
    {
        return characterDefinition.charExperience / characterDefinition.charLevelUps[characterDefinition.charLevel - 1].experienceNeeded;
    }
    public int GetLevel()
    {
        return characterDefinition.charLevel;
    }


}