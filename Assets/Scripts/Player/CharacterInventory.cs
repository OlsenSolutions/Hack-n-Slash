using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInventory : MonoBehaviour
{
    public static CharacterInventory instance;

    public CharacterStats playerStats;
    GameObject foundPlayer;

    public Image[] hotBarDisplayHolders = new Image[4];

    int inventoryItemCap = 20;
    int idCount = 1;
    bool addedItem = true;

    public Dictionary<int, InventoryEntry> itemsInInventory = new Dictionary<int, InventoryEntry>();
    public InventoryEntry itemEntry;

    void Start()
    {
        instance = this;
        itemEntry = new InventoryEntry(0, null, null);
        itemsInInventory.Clear();

        foundPlayer = GameObject.FindGameObjectWithTag("Player");
        playerStats = foundPlayer.GetComponent<CharacterStats>();
    }

    void Update()
    {

        if (Input.GetKeyDown("1"))
        {
            TriggerItemUse(101);
        }
        if (Input.GetKeyDown("2"))
        {
            TriggerItemUse(102);
        }
        if (Input.GetKeyDown("3"))
        {
            TriggerItemUse(103);
        }
        if (Input.GetKeyDown("4"))
        {
            TriggerItemUse(104);
        }

        if (!addedItem)
        {
            TryPickUp();
        }
    }



    public void StoreItem(ItemPickUp itemToStore)
    {
        // addedItem = false;
        if ((playerStats.characterDefinition.currentEncumbrance + itemToStore.itemDefinition.itemWeight) <= playerStats.characterDefinition.maxEncumbrance)
        {
            itemEntry.invEntry = itemToStore;
            itemEntry.stackSize = 1;
            itemEntry.hbSprite = itemToStore.itemDefinition.itemIcon;
            addedItem = false;
            itemToStore.gameObject.SetActive(false);
        }

    }

    void TryPickUp()
    {
        bool itsIn = true;

        if (itemEntry.invEntry)
        {
            if (itemsInInventory.Count == 0)
            {
                addedItem = AddItemToInv(addedItem);
            }
            else
            {
                if (itemEntry.invEntry.itemDefinition.isStackable)
                {
                    foreach (KeyValuePair<int, InventoryEntry> x in itemsInInventory)
                    {
                        if (itemEntry.invEntry.itemDefinition == x.Value.invEntry.itemDefinition)
                        {
                            x.Value.stackSize += 1;
                            AddItemToHotBar(x.Value);
                            itsIn = true;
                            Destroy(itemEntry.invEntry.gameObject);
                            break;

                        }
                        else
                        {
                            itsIn = false;
                        }
                    }
                }
                else
                {
                    itsIn = false;

                    if (itemsInInventory.Count == inventoryItemCap)
                    {
                        itemEntry.invEntry.gameObject.SetActive(true);
                        Debug.Log("Inventory is Full");
                    }

                }

                if (!itsIn)
                {
                    addedItem = AddItemToInv(addedItem);
                    itsIn = true;
                }

            }

        }

    }

    bool AddItemToInv(bool finishedAdding)
    {
        itemsInInventory.Add(idCount, new InventoryEntry(itemEntry.stackSize, Instantiate(itemEntry.invEntry), itemEntry.hbSprite));
        Destroy(itemEntry.invEntry.gameObject);
        AddItemToHotBar(itemsInInventory[idCount]);
        idCount = IncreaseID(idCount);
        finishedAdding = true;
        return finishedAdding;
    }

    private int IncreaseID(int idCount)
    {
        int newID = 1;
        for (int i = 1; i <= itemsInInventory.Count; i++)
        {
            if (itemsInInventory.ContainsKey(newID))
                newID += 1;
            else return newID;
        }
        return newID;
    }

    private void AddItemToHotBar(InventoryEntry itemForHotBar)
    {
        int barCount = 0;
        bool upCount = false;

        foreach (Image images in hotBarDisplayHolders)
        {
            barCount += 1;
            if (itemForHotBar.hotBarSlot == 0)
            {
                if (images.sprite == null)
                {
                    var tempColor = images.color;
                    tempColor.a = 1f;
                    images.color = tempColor;
                    itemForHotBar.hotBarSlot = barCount;
                    images.sprite = itemForHotBar.hbSprite;
                    upCount = true;
                    break;
                }
            }
            else if (itemForHotBar.invEntry.itemDefinition.isStackable)
            {
                upCount = true;
            }

        }

        if (upCount)
        {
            hotBarDisplayHolders[itemForHotBar.hotBarSlot - 1].GetComponentInChildren<Text>().text = itemForHotBar.stackSize.ToString();
        }

    }

    public void ClearInventory()
    {
        foreach (Image hotPlace in hotBarDisplayHolders)
        {
            hotPlace.sprite = null;
            var tempColor = hotPlace.color;
            tempColor.a = 0f;
            hotPlace.color = tempColor;
            hotPlace.GetComponentInChildren<Text>().text = "";
        }
    }

    public void TriggerItemUse(int itemToUseID)
    {
        bool trigger = false;
        itemToUseID -= 100;
        foreach (KeyValuePair<int, InventoryEntry> x in itemsInInventory)
        {
            if (x.Value.hotBarSlot == itemToUseID)
            {
                trigger = true;
            }

            if (trigger)
            {
                if (x.Value.stackSize == 1)
                {
                    if (x.Value.invEntry.itemDefinition.isStackable)
                    {
                        if (x.Value.hotBarSlot != 0)
                        {
                            hotBarDisplayHolders[x.Value.hotBarSlot - 1].sprite = null;
                            var tempColor = hotBarDisplayHolders[x.Value.hotBarSlot - 1].color;
                            tempColor.a = 0f;
                            hotBarDisplayHolders[x.Value.hotBarSlot - 1].color = tempColor;
                            hotBarDisplayHolders[x.Value.hotBarSlot - 1].GetComponentInChildren<Text>().text = "";


                        }

                        x.Value.invEntry.UseItem();
                        itemsInInventory.Remove(x.Key);
                        break;
                    }
                    else
                    {
                        x.Value.invEntry.UseItem();
                        if (!x.Value.invEntry.itemDefinition.isIndestructable)
                        {
                            itemsInInventory.Remove(x.Key);
                            break;
                        }
                    }
                }
                else
                {
                    x.Value.invEntry.UseItem();
                    x.Value.stackSize -= 1;
                    hotBarDisplayHolders[x.Value.hotBarSlot - 1].GetComponentInChildren<Text>().text = x.Value.stackSize.ToString();
                    break;
                }
            }
        }
    }
}