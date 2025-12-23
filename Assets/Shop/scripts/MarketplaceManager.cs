using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MarketplaceManager : MonoBehaviour
{
    public static MarketplaceManager instance;

    [Header("Shop Items")]
    public List<ShopItem> availableItems;

    [Header("UI Referenzen")]
    public Transform shopItemContainer;
    public GameObject shopItemButtonPrefab;

    [Header("Detail Panel")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemPriceText;
    public Image itemIcon;
    public Button buyButton;

    [Header("Player Info")]
    public TextMeshProUGUI playerGoldText;
    public TextMeshProUGUI playerStatsText;

    private ShopItem selectedItem;
    private CharacterStats playerStats;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Debug.Log("🏪 MarketplaceManager startet.. .");

        // Spieler finden
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            Debug.Log("✅ Player gefunden: " + player.name);
            playerStats = player.GetComponent<CharacterStats>();
            
            if (playerStats != null)
            {
                Debug.Log("✅ CharacterStats gefunden!  Gold: " + playerStats.gold);
            }
            else
            {
                Debug. LogError("❌ Player hat kein CharacterStats Script!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Kein Player gefunden - erstelle Dummy");
            CreateDummyPlayer();
        }

        PopulateShop();
        UpdatePlayerUI();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonPressed);
        }

        Debug.Log("🏪 MarketplaceManager fertig!");
    }

    void CreateDummyPlayer()
    {
        GameObject temp = new GameObject("TempPlayer");
        temp.tag = "Player";
        playerStats = temp.AddComponent<CharacterStats>();
        temp.AddComponent<PlayerInventory>();
        
        // Lade gespeicherte Werte
        playerStats.gold = PlayerPrefs.GetInt("Gold", 1000); // TEST: 1000 Gold
        playerStats.level = PlayerPrefs.GetInt("Level", 1);
        playerStats.attack = 10 + (playerStats.level * 3);
        playerStats.defense = 5 + (playerStats.level * 1);
        playerStats.currentHP = 100;
        playerStats.maxHP = 100 + ((playerStats.level - 1) * 20);

        Debug.Log("💰 Dummy Player erstellt mit " + playerStats.gold + " Gold");
    }

    void PopulateShop()
    {
        if (shopItemContainer == null)
        {
            Debug.LogError("❌ Shop Item Container fehlt!");
            return;
        }

        if (shopItemButtonPrefab == null)
        {
            Debug.LogError("❌ Shop Item Button Prefab fehlt!");
            return;
        }

        // Lösche alte Buttons
        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }

        // Erstelle Button für jedes Item
        foreach (ShopItem item in availableItems)
        {
            GameObject btnObj = Instantiate(shopItemButtonPrefab, shopItemContainer);

            // Finde die Komponenten
            Image icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI nameText = btnObj.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = btnObj.transform. Find("Price").GetComponent<TextMeshProUGUI>();

            // Setze Werte
            if (item. icon != null) 
                icon.sprite = item.icon;
            else
                icon.color = new Color(1, 1, 1, 0.3f);

            nameText.text = item.itemName;
            priceText. text = item.price + " G";

            // Click Event
            Button btn = btnObj.GetComponent<Button>();
            ShopItem itemCopy = item;
            btn.onClick.AddListener(() => SelectItem(itemCopy));
        }

        Debug.Log("✅ Shop mit " + availableItems.Count + " Items gefüllt!");
    }

    void SelectItem(ShopItem item)
    {
        selectedItem = item;

        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemPriceText.text = "Preis: " + item. price + " Gold";

        if (item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.color = Color.white;
        }
        else
        {
            itemIcon.color = new Color(1, 1, 1, 0.3f);
        }

        // Button aktivieren/deaktivieren
        if (playerStats != null)
        {
            buyButton.interactable = (playerStats.gold >= item.price);
        }

        Debug.Log("📦 Item ausgewählt: " + item.itemName);
    }

    void OnBuyButtonPressed()
    {
        if (selectedItem == null)
        {
            Debug.Log("❌ Kein Item ausgewählt!");
            return;
        }

        if (PlayerInventory.instance == null)
        {
            Debug.LogError("❌ PlayerInventory nicht gefunden!");
            return;
        }

        bool success = PlayerInventory.instance. BuyItem(selectedItem);

        if (success)
        {
            Debug.Log("✅ Gekauft: " + selectedItem. itemName);
            UpdatePlayerUI();

            // Button-Status aktualisieren
            if (playerStats != null)
            {
                buyButton.interactable = (playerStats.gold >= selectedItem.price);
            }
        }
        else
        {
            Debug.Log("❌ Kauf fehlgeschlagen!");
        }
    }

    void UpdatePlayerUI()
    {
        if (playerStats != null)
        {
            if (playerGoldText != null)
                playerGoldText.text = "Gold: " + playerStats.gold;

            if (playerStatsText != null)
                playerStatsText.text = $"ATK: {playerStats.attack} | DEF: {playerStats. defense} | HP: {playerStats.currentHP}/{playerStats.maxHP}";
        }
    }
}