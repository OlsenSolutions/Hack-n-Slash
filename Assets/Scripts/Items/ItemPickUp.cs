using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public ItemPickUps_SO itemDefinition;
    public CharacterStats charStats;
    public CharacterInventory charInventory;
    public GameObject foundPlayer;



    public ItemPickUp()
    {

    }

    void Start()
    {
        foundPlayer = GameObject.FindGameObjectWithTag("Player");
        charStats = foundPlayer.GetComponent<CharacterStats>();
        charInventory = foundPlayer.GetComponentInChildren<CharacterInventory>();

    }

    void StoreItem()
    {
        if (charInventory != null)
            charInventory.StoreItem(this);
    }

    public void UseItem()
    {
        switch (itemDefinition.itemType)
        {
            case ItemTypeDefinitions.HEALTH:
                charStats.ApplyHealth(itemDefinition.itemAmount);
                charStats.healthParticle.Play();
                break;
            case ItemTypeDefinitions.MANA:
                charStats.ApplyMana(itemDefinition.itemAmount);
                charStats.manaParticle.Play();

                break;
            case ItemTypeDefinitions.EXPERIENCE:
                charStats.GiveExperience(itemDefinition.itemAmount);
                charStats.expParticle.Play();

                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            if (itemDefinition.isStorable)
            {
                StoreItem();
            }
            else
            {
                UseItem();
            }
        }
    }
}
