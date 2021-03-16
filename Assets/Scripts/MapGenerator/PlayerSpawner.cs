using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject player;
    public Image[] hotBarDisplayHolders = new Image[4];
    public CharacterStats_SO characterStatsDuringGame;

    void Start()
    {
        characterStatsDuringGame = null;
    }
    public void CreatePlayer(GameObject platform)
    {
        var playerPrefab = Instantiate(player, platform.transform.position, player.transform.rotation);
        playerPrefab.GetComponent<CharacterStats>().charInv.hotBarDisplayHolders = hotBarDisplayHolders;
        playerPrefab.GetComponent<CharacterStats>().charInv.ClearInventory();
        if (characterStatsDuringGame != null)
        {
            playerPrefab.GetComponent<CharacterStats>().characterDefinition = UpdatePlayerStats(characterStatsDuringGame);
        }
        playerPrefab.GetComponent<CharacterBehaviour>().fireballHolder = GetComponent<MapGenerator>().centerHolder;
        var pointer = GameObject.FindGameObjectWithTag("Pointer");
        if (pointer != null)
            pointer.GetComponent<Pointer>().targetPosition = platform.transform.position;
        playerPrefab.GetComponent<CharacterBehaviour>().EscapeMenu = GetComponent<MapGenerator>().escapeMenu;
        EventManager.TriggerEvent(EventManager.PlayerInstantiated);
    }

    CharacterStats_SO UpdatePlayerStats(CharacterStats_SO PlayerStatsOld)
    {
        var PlayerStatsNew = new CharacterStats_SO();
        PlayerStatsNew.setManually = true;
        PlayerStatsNew.maxHealth = PlayerStatsOld.maxHealth;
        PlayerStatsNew.currentHealth = PlayerStatsOld.currentHealth;
        PlayerStatsNew.maxMana = PlayerStatsOld.maxMana;
        PlayerStatsNew.currentMana = PlayerStatsOld.currentMana;
        PlayerStatsNew.currentDamage = PlayerStatsOld.currentDamage;
        PlayerStatsNew.maxEncumbrance = PlayerStatsOld.maxEncumbrance;
        PlayerStatsNew.currentEncumbrance = PlayerStatsOld.currentEncumbrance;
        PlayerStatsNew.charExperience = PlayerStatsOld.charExperience;
        PlayerStatsNew.charLevel = PlayerStatsOld.charLevel;
        PlayerStatsNew.charLevelUps = PlayerStatsOld.charLevelUps;
        PlayerStatsNew.currentMagicDamage = PlayerStatsOld.currentMagicDamage;
        PlayerStatsNew.charRenevalPoints = PlayerStatsOld.charRenevalPoints;
        return PlayerStatsNew;
    }


}
