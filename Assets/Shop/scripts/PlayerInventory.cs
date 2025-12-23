using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    [Header("Ausgerüstete Items")]
    public ShopItem equippedWeapon;
    public ShopItem equippedArmor;

    [Header("Tränke")]
    public List<PotionStack> potions = new List<PotionStack>();

    [Header("Standard Heiltrank")]
    public ShopItem defaultHealingPotion;

    private CharacterStats playerStats;

    [System.Serializable]
    public class PotionStack
    {
        public ShopItem potion;
        public int count;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance.equippedWeapon != null)
            {
                equippedWeapon = instance.equippedWeapon;
            }
            if (instance.equippedArmor != null)
            {
                equippedArmor = instance.equippedArmor;
            }

            instance = this;
        }
    }

    void Start()
    {
        playerStats = GetComponent<CharacterStats>();

        if (playerStats == null)
        {
            Debug.LogError("PlayerInventory braucht CharacterStats auf dem gleichen Objekt!");
        }

        LoadSavedPotions();

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdatePotionUI();
        }
    }

    public bool BuyItem(ShopItem item)
    {
        if (playerStats == null) return false;

        if (playerStats.gold < item.price)
        {
            Debug.Log("❌ Nicht genug Gold!");
            return false;
        }

        playerStats.gold -= item.price;
        PlayerPrefs.SetInt("Gold", playerStats.gold);

        switch (item.type)
        {
            case ShopItem.ItemType.Potion:
                AddPotion(item);
                Debug.Log("✅ Trank gekauft: " + item.itemName);
                break;

            case ShopItem.ItemType.Weapon:
                EquipWeapon(item);
                Debug.Log("⚔️ Waffe ausgerüstet: " + item.itemName);
                break;

            case ShopItem.ItemType.Armor:
                EquipArmor(item);
                Debug.Log("🛡️ Rüstung ausgerüstet: " + item.itemName);
                break;
        }

        SaveInventory();
        return true;
    }

    void AddPotion(ShopItem potion)
    {
        PotionStack existing = potions.Find(p => p.potion == potion);

        if (existing != null)
        {
            existing.count++;
        }
        else
        {
            potions.Add(new PotionStack { potion = potion, count = 1 });
        }
    }

    public void UsePotion(int index)
    {
        if (index < 0 || index >= potions.Count)
        {
            Debug.Log("❌ Ungültiger Trank-Index!");
            return;
        }

        PotionStack stack = potions[index];

        if (playerStats != null)
        {
            playerStats.Heal(stack.potion.healAmount);
            Debug.Log("💊 Trank benutzt!  +" + stack.potion.healAmount + " HP");
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterStats stats = player.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.Heal(stack.potion.healAmount);
                    Debug.Log("💊 Trank benutzt! +" + stack.potion.healAmount + " HP");
                }
            }
        }

        stack.count--;

        if (stack.count <= 0)
        {
            potions.RemoveAt(index);
        }

        SaveInventory();
    }

    void EquipWeapon(ShopItem weapon)
    {
        if (equippedWeapon != null)
        {
            playerStats.attack -= equippedWeapon.attackBonus;
        }

        equippedWeapon = weapon;
        playerStats.attack += weapon.attackBonus;

        Debug.Log("⚔️ Angriff jetzt: " + playerStats.attack);
    }

    void EquipArmor(ShopItem armor)
    {
        if (equippedArmor != null)
        {
            playerStats.defense -= equippedArmor.defenseBonus;
            playerStats.maxHP -= equippedArmor.maxHPBonus;
        }

        equippedArmor = armor;
        playerStats.defense += armor.defenseBonus;
        playerStats.maxHP += armor.maxHPBonus;
        playerStats.currentHP += armor.maxHPBonus;

        Debug.Log("🛡️ Verteidigung jetzt: " + playerStats.defense);
    }

    void SaveInventory()
    {
        PlayerPrefs.SetString("EquippedWeapon", equippedWeapon != null ? equippedWeapon.itemName : "");
        PlayerPrefs.SetString("EquippedArmor", equippedArmor != null ? equippedArmor.itemName : "");

        int totalPotions = 0;
        foreach (var stack in potions)
        {
            totalPotions += stack.count;
        }
        PlayerPrefs.SetInt("TotalPotions", totalPotions);

        PlayerPrefs.Save();
        Debug.Log("💾 Inventar gespeichert!  Tränke: " + totalPotions);
    }

    void LoadSavedPotions()
    {
        int savedPotions = PlayerPrefs.GetInt("TotalPotions", 0);

        if (savedPotions > 0)
        {
            ShopItem healPotion = defaultHealingPotion;

            if (healPotion == null)
            {
                MarketplaceManager marketplace = FindAnyObjectByType<MarketplaceManager>();
                if (marketplace != null && marketplace.availableItems.Count > 0)
                {
                    foreach (var item in marketplace.availableItems)
                    {
                        if (item.type == ShopItem.ItemType.Potion)
                        {
                            healPotion = item;
                            break;
                        }
                    }
                }
            }

            if (healPotion != null)
            {
                potions.Clear();
                potions.Add(new PotionStack { potion = healPotion, count = savedPotions });
                Debug.Log("📥 Tränke geladen: " + savedPotions);
            }
            else
            {
                Debug.LogWarning("⚠️ Kein Heiltrank gefunden zum Laden!");
            }
        }
    }
}