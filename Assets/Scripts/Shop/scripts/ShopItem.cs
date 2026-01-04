using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    [Header("Basis Info")]
    public string itemName;
    public Sprite icon;
    public int price;
    public ItemType type;

    [Header("Effekte")]
    public int attackBonus = 0;
    public int defenseBonus = 0;
    public int healAmount = 0;
    public int maxHPBonus = 0;

    [Header("Beschreibung")]
    [TextArea(3, 5)]
    public string description;

    public enum ItemType
    {
        Potion,
        Weapon,
        Armor,
        Accessory
    }
}