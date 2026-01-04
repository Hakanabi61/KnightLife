using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    [Header("Inventory")]
    public List<ShopItem> potions = new List<ShopItem>();
    public List<ShopItem> weapons = new List<ShopItem>();
    public List<ShopItem> armor = new List<ShopItem>();

    [Header("Equipped Items")]
    public ShopItem equippedWeapon;
    public ShopItem equippedArmor;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ============================================
    // ADD ITEMS (PUBLIC - für Dungeon Rewards)
    // ============================================

    public void AddPotion(ShopItem item)
    {
        if (item == null) return;

        potions.Add(item);
        Debug.Log($"✓ Added {item.itemName} to inventory");

        // Update UI
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdatePotionUI();
        }
    }

    public void AddWeapon(ShopItem item)
    {
        if (item == null) return;

        weapons.Add(item);
        Debug.Log($"✓ Added {item.itemName} to inventory");
    }

    public void AddArmor(ShopItem item)
    {
        if (item == null) return;

        armor.Add(item);
        Debug.Log($"✓ Added {item.itemName} to inventory");
    }

    // ============================================
    // KAUFEN (für Shop/Marketplace)
    // ============================================

    public bool BuyItem(ShopItem item, int cost)
    {
        if (item == null)
        {
            Debug.LogWarning("⚠️ Item is null!");
            return false;
        }

        // Prüfe ob genug Gold
        if (GameManager.instance == null || GameManager.instance.playerStats == null)
        {
            Debug.LogWarning("⚠️ GameManager or PlayerStats not found!");
            return false;
        }

        if (GameManager.instance.playerStats.gold < cost)
        {
            Debug.Log("⚠️ Nicht genug Gold!");
            return false;
        }

        // Gold abziehen
        GameManager.instance.playerStats.gold -= cost;

        // Item zu passendem Inventory hinzufügen
        switch (item.type)
        {
            case ShopItem.ItemType.Potion:
                AddPotion(item);
                break;

            case ShopItem.ItemType.Weapon:
                AddWeapon(item);
                break;

            case ShopItem.ItemType.Armor:
                AddArmor(item);
                break;

            case ShopItem.ItemType.Accessory:
                Debug.Log($"✓ Bought accessory: {item.itemName}");
                break;
        }

        // UI Update
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }

        Debug.Log($"✓ Gekauft: {item.itemName} für {cost} Gold");
        return true;
    }

    // Überladung:  Nutzt item.price wenn kein cost angegeben
    public bool BuyItem(ShopItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("⚠️ Item is null!");
            return false;
        }

        // Nutze den Preis vom Item selbst
        return BuyItem(item, item.price);
    }

    // ============================================
    // USE ITEMS
    // ============================================

    public void UsePotion(int index)
    {
        if (index < 0 || index >= potions.Count)
        {
            Debug.LogWarning("⚠️ Invalid potion index!");
            return;
        }

        ShopItem potion = potions[index];

        // Heal Player
        if (GameManager.instance != null && GameManager.instance.playerStats != null)
        {
            GameManager.instance.playerStats.Heal(potion.healAmount);
            Debug.Log($"✓ Used {potion.itemName} - Healed {potion.healAmount} HP");
        }

        // Remove from inventory
        potions.RemoveAt(index);

        // Update UI
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdatePotionUI();
            GameManager.instance.UpdateUI();
        }
    }

    // ============================================
    // EQUIP ITEMS
    // ============================================

    public void EquipWeapon(ShopItem weapon)
    {
        if (weapon == null) return;

        // Unequip old weapon
        if (equippedWeapon != null)
        {
            weapons.Add(equippedWeapon);
        }

        // Equip new weapon
        equippedWeapon = weapon;
        weapons.Remove(weapon);

        // Apply stats
        if (GameManager.instance != null && GameManager.instance.playerStats != null)
        {
            GameManager.instance.playerStats.attack += weapon.attackBonus;
            GameManager.instance.UpdateUI();
        }

        Debug.Log($"✓ Equipped {weapon.itemName}");
    }

    public void EquipArmor(ShopItem armorItem)
    {
        if (armorItem == null) return;

        // Unequip old armor
        if (equippedArmor != null)
        {
            armor.Add(equippedArmor);
        }

        // Equip new armor
        equippedArmor = armorItem;
        armor.Remove(armorItem);

        // Apply stats
        if (GameManager.instance != null && GameManager.instance.playerStats != null)
        {
            GameManager.instance.playerStats.defense += armorItem.defenseBonus;
            GameManager.instance.UpdateUI();
        }

        Debug.Log($"✓ Equipped {armorItem.itemName}");
    }

    // ============================================
    // UTILITY
    // ============================================

    public int GetPotionCount()
    {
        return potions.Count;
    }

    public bool HasPotions()
    {
        return potions.Count > 0;
    }

    public void ClearInventory()
    {
        potions.Clear();
        weapons.Clear();
        armor.Clear();
        equippedWeapon = null;
        equippedArmor = null;
        Debug.Log("✓ Inventory cleared");
    }
}