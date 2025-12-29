using UnityEngine;

[System.Serializable]
public class EncounterData
{
    [Header("Encounter Type")]
    public EncounterType type;

    [Header("Story")]
    public string title = "Encounter";
    [TextArea(3, 6)]
    public string description = "Du findest etwas...  ";

    [Header("Enemy (für BATTLE)")]
    public EnemyData enemyData;

    [Header("Chest Rewards (für CHEST)")]
    public int goldReward = 50;
    public bool hasPotion = false;
    public int potionAmount = 1;

    [Header("Shop Items (für SHOP)")]
    public EncounterShopItem[] shopItems; // ← GEÄNDERT

    [Header("Campfire (für REST)")]
    public int healAmount = 30;
    public bool fullHeal = false;
}


[System.Serializable]
public class EncounterShopItem // ← GEÄNDERT
{
    public string itemName = "Potion";
    public int price = 20;
    public EncounterItemType type; // ← GEÄNDERT
}

public enum EncounterItemType // ← GEÄNDERT
{
    POTION,
    WEAPON,
    ARMOR,
    ACCESSORY
}