using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    [Header("Player")]
    public Transform player;
    public float moveSpeed = 3f;
    public Animator playerAnimator;
    public CharacterStats playerStats;

    [Header("Encounter Points")]
    public List<EncounterPoint> encounterPoints = new List<EncounterPoint>();
    private int currentPointIndex = 0;
    private EncounterPoint currentChoicePoint;

    [Header("Battle System")]
    public GameObject battleEnemy;
    public GameObject battlePanel;

    [Header("UI")]
    public GameObject encounterPanel;
    public TextMeshProUGUI encounterTitleText;
    public TextMeshProUGUI encounterDescriptionText;
    public Transform buttonContainer;

    [Header("Buttons (Already in Container)")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    [Header("Camera")]
    public Camera mainCamera;
    public float cameraFollowSpeed = 3f;
    public float cameraYOffset = 2f;

    // Private
    private bool isMoving = false;
    private Vector3 targetPosition;
    private List<Button> allButtons = new List<Button>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (enableDebugLogs) Debug.Log("========== DUNGEON START ==========");

        // WICHTIG:  IMMER Player neu finden nach Scene Load!
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // Falls nicht gefunden, suche nach Namen
        if (playerObj == null)
        {
            Debug.LogWarning("⚠️ Player not found by tag, searching by name.. .");
            playerObj = GameObject.Find("Player");
        }

        if (playerObj != null)
        {
            // WICHTIG: Stelle sicher dass Tag gesetzt ist! 
            if (playerObj.tag != "Player")
            {
                Debug.LogWarning($"⚠️ Player GameObject has wrong tag '{playerObj.tag}', fixing to 'Player'");
                playerObj.tag = "Player";
            }

            player = playerObj.transform;
            playerStats = playerObj.GetComponent<CharacterStats>();
            playerAnimator = playerObj.GetComponent<Animator>();

            if (enableDebugLogs) Debug.Log($"✅ Player found:  {player.name} at {player.position}");
        }
        else
        {
            Debug.LogError("❌ Player NOT FOUND!");
        }

        // Battle Enemy verstecken
        if (battleEnemy != null)
        {
            battleEnemy.SetActive(false);
        }

        // Battle Panel verstecken
        if (battlePanel != null)
        {
            battlePanel.SetActive(false);
        }

        // Buttons sammeln
        allButtons.Clear();
        if (button1 != null) allButtons.Add(button1);
        if (button2 != null) allButtons.Add(button2);
        if (button3 != null) allButtons.Add(button3);
        if (button4 != null) allButtons.Add(button4);

        if (enableDebugLogs) Debug.Log($"Buttons found: {allButtons.Count}");

        // Alle Buttons verstecken
        HideAllButtons();

        // Encounter Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        // PRÜFE OB WIR VOM SHOP ZURÜCKKEHREN
        string dungeonState = PlayerPrefs.GetString("DungeonState", "");

        if (dungeonState == "AfterShop")
        {
            if (enableDebugLogs) Debug.Log("Returning from Shop - going to Boss!");

            // Lösche State
            PlayerPrefs.SetString("DungeonState", "");
            PlayerPrefs.Save();

            // Lade gespeicherte Shop Position
            if (PlayerPrefs.HasKey("ShopPositionX") && player != null)
            {
                float x = PlayerPrefs.GetFloat("ShopPositionX");
                float y = PlayerPrefs.GetFloat("ShopPositionY");
                float z = PlayerPrefs.GetFloat("ShopPositionZ");

                player.position = new Vector3(x, y, z);

                if (enableDebugLogs) Debug.Log($"✅ Player positioned at saved Shop position:  {player.position}");

                // Lösche gespeicherte Position
                PlayerPrefs.DeleteKey("ShopPositionX");
                PlayerPrefs.DeleteKey("ShopPositionY");
                PlayerPrefs.DeleteKey("ShopPositionZ");
            }
            else
            {
                // Fallback wenn keine Position gespeichert
                if (enableDebugLogs) Debug.LogWarning("⚠️ No saved Shop position, using fallback");

                // Finde Choice Point
                EncounterPoint choicePoint = null;
                foreach (EncounterPoint point in encounterPoints)
                {
                    if (point.encounterType == EncounterType.Choice)
                    {
                        choicePoint = point;
                        break;
                    }
                }

                if (choicePoint != null && player != null)
                {
                    player.position = choicePoint.transform.position;
                    if (enableDebugLogs) Debug.Log($"Player at Choice Point: {choicePoint.transform.position}");
                }
            }

            if (player != null && enableDebugLogs)
            {
                Debug.Log($"Player position BEFORE GoToBoss: {player.position}");
            }

            // Gehe direkt zum Boss
            GoToBoss();
        }
        else
        {
            // Normale Dungeon Start
            // Player zum Start Point bewegen
            if (encounterPoints.Count > 0 && player != null)
            {
                player.position = encounterPoints[0].transform.position;
                if (enableDebugLogs) Debug.Log($"Player at Start:  {encounterPoints[0].name}");
            }

            // Starte Dungeon (überspringe Point 0 = Start)
            currentPointIndex = 1;
            MoveToNextPoint();
        }

        if (enableDebugLogs) Debug.Log("===================================");
    }

    void Update()
    {
        // WICHTIG:  Prüfe ob Player existiert, falls nicht -> finde neu
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            // Falls nicht mit Tag gefunden, suche nach Namen
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");

                if (playerObj != null)
                {
                    Debug.LogWarning($"⚠️ Found Player by name, fixing tag");
                    playerObj.tag = "Player";
                }
            }

            if (playerObj != null)
            {
                player = playerObj.transform;
                playerStats = playerObj.GetComponent<CharacterStats>();
                playerAnimator = playerObj.GetComponent<Animator>();

                Debug.Log($"✅ Player refound in Update: {player.name} at {player.position}");
            }
            else
            {
                // Kein Player gefunden
                if (isMoving)
                {
                    Debug.LogError("❌ isMoving=TRUE but player is NULL and can't be found!");
                    isMoving = false;
                }
                return;
            }
        }

        // Player Movement
        if (isMoving && player != null)
        {
            // DEBUG - Zeige Movement Info
            if (enableDebugLogs && Time.frameCount % 30 == 0)
            {
                Debug.Log($"🏃 Moving: {player.position} → {targetPosition}, Distance: {Vector3.Distance(player.position, targetPosition):F2}");
            }

            player.position = Vector3.MoveTowards(
                player.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Angekommen? 
            if (Vector3.Distance(player.position, targetPosition) < 0.1f)
            {
                player.position = targetPosition;
                isMoving = false;

                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isRunning", false);
                }

                if (enableDebugLogs) Debug.Log("✅ Reached target!");

                OnReachedPoint();
            }
            else
            {
                // Animation während Movement
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isRunning", true);
                }
            }
        }

        // Camera Follow
        if (mainCamera != null && player != null)
        {
            Vector3 targetCameraPos = new Vector3(
                player.position.x,
                cameraYOffset,
                mainCamera.transform.position.z
            );

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetCameraPos,
                cameraFollowSpeed * Time.deltaTime
            );
        }
    }

    // ============================================
    // MOVEMENT
    // ============================================

    void MoveToNextPoint()
    {
        if (currentPointIndex >= encounterPoints.Count)
        {
            if (enableDebugLogs) Debug.Log("DUNGEON COMPLETE!");
            return;
        }

        if (enableDebugLogs) Debug.Log($"Moving to Point [{currentPointIndex}]:  {encounterPoints[currentPointIndex].name}");

        targetPosition = encounterPoints[currentPointIndex].transform.position;
        isMoving = true;
    }

    void MoveToPoint(EncounterPoint targetPoint)
    {
        if (enableDebugLogs) Debug.Log($"========== MoveToPoint ==========");
        if (enableDebugLogs) Debug.Log($"Target: {targetPoint.name} ({targetPoint.encounterType})");

        targetPosition = targetPoint.transform.position;
        isMoving = true;

        // Finde Index von targetPoint in der Liste
        bool found = false;
        for (int i = 0; i < encounterPoints.Count; i++)
        {
            if (encounterPoints[i] == targetPoint)
            {
                currentPointIndex = i;
                found = true;
                if (enableDebugLogs) Debug.Log($"Found in list at index {i}");
                break;
            }
        }

        if (!found)
        {
            // Point ist NICHT in der Liste (z.B. Chest/Shop bei Choice)
            if (enableDebugLogs) Debug.Log($"{targetPoint.name} NOT in encounter list (choice path)");
            // Setze Index auf -1 damit CompleteEncounter() nicht automatisch weitergeht
            currentPointIndex = -1;
        }

        if (enableDebugLogs) Debug.Log("=================================");
    }

    void OnReachedPoint()
    {
        if (currentPointIndex < 0)
        {
            // Wir sind bei einem Choice Point (Chest/Shop)
            // Finde den Point manuell in der Scene
            EncounterPoint[] allPoints = FindObjectsOfType<EncounterPoint>();
            foreach (EncounterPoint p in allPoints)
            {
                if (Vector3.Distance(player.position, p.transform.position) < 0.2f)
                {
                    if (enableDebugLogs) Debug.Log($"Reached (not in list): {p.name} ({p.encounterType})");
                    ShowEncounter(p);
                    return;
                }
            }

            Debug.LogError("Reached unknown point!");
            return;
        }

        if (currentPointIndex >= encounterPoints.Count) return;

        EncounterPoint point = encounterPoints[currentPointIndex];

        if (enableDebugLogs) Debug.Log($"Reached [{currentPointIndex}]: {point.name} ({point.encounterType})");

        // Zeige Encounter
        ShowEncounter(point);
    }

    // ============================================
    // ENCOUNTER UI
    // ============================================

    void ShowEncounter(EncounterPoint point)
    {
        if (encounterPanel == null) return;

        if (enableDebugLogs) Debug.Log($"========== SHOW ENCOUNTER ==========");
        if (enableDebugLogs) Debug.Log($"Type: {point.encounterType}");

        // Panel aktivieren
        encounterPanel.SetActive(true);

        // Title & Description setzen
        if (encounterTitleText != null)
        {
            encounterTitleText.text = GetTitleForType(point.encounterType);
        }

        if (encounterDescriptionText != null)
        {
            encounterDescriptionText.text = GetDescriptionForType(point.encounterType);
        }

        // Buttons basierend auf Typ
        HideAllButtons();

        switch (point.encounterType)
        {
            case EncounterType.Enemy:
            case EncounterType.Elite:
            case EncounterType.Boss:
                ShowButton(0, "KAEMPFEN", () => OnBattleStart(point));
                ShowButton(1, "FLIEHEN", () => OnFlee());
                break;

            case EncounterType.Chest:
                ShowButton(0, "OEFFNEN", () => OnChestOpen(point));
                ShowButton(1, "WEITER", () => GoToBoss());
                break;

            case EncounterType.Shop:
                ShowButton(0, "HANDELN", () => OnShopEnter());
                ShowButton(1, "WEITER", () => GoToBoss());
                break;

            case EncounterType.Campfire:
                ShowButton(0, "HEILEN (+50 HP)", () => OnCampfireHeal());
                ShowButton(1, "KRAFT (+2 ATK)", () => OnCampfireAttack());
                ShowButton(2, "SCHUTZ (+2 DEF)", () => OnCampfireDefense());
                break;

            case EncounterType.Choice:
                // Speichere Choice Point
                currentChoicePoint = point;

                // Zeige Buttons basierend auf choiceOptions
                if (point.choiceOption1 != null && point.choiceOption2 != null)
                {
                    string label1 = GetLabelForPoint(point.choiceOption1);
                    string label2 = GetLabelForPoint(point.choiceOption2);

                    ShowButton(0, label1, () => OnChoiceMade(point.choiceOption1));
                    ShowButton(1, label2, () => OnChoiceMade(point.choiceOption2));
                }
                else
                {
                    Debug.LogWarning("Choice options not assigned!");
                    ShowButton(0, "CHEST", () => OnChoiceChest());
                    ShowButton(1, "SHOP", () => OnChoiceShop());
                }
                break;

            case EncounterType.Mystery:
                ShowButton(0, "OEFFNEN", () => OnMystery());
                break;
        }

        if (enableDebugLogs) Debug.Log("====================================");
    }

    void ShowButton(int index, string label, UnityEngine.Events.UnityAction onClick)
    {
        if (index >= allButtons.Count)
        {
            Debug.LogWarning($"Button index {index} out of range!");
            return;
        }

        Button btn = allButtons[index];
        if (btn == null)
        {
            Debug.LogWarning($"Button {index} is NULL!");
            return;
        }

        // Button aktivieren
        btn.gameObject.SetActive(true);

        // Text setzen
        TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = label;
        }

        // Click Listener
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);

        if (enableDebugLogs) Debug.Log($"Button {index}:  {label}");
    }

    void HideAllButtons()
    {
        foreach (Button btn in allButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    // ============================================
    // BUTTON CALLBACKS
    // ============================================

    void OnBattleStart(EncounterPoint point)
    {
        if (enableDebugLogs) Debug.Log("========== BATTLE START ==========");

        // Encounter Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Hole Enemy Data
        if (point.enemyData == null)
        {
            Debug.LogError("No enemy data on this point!");
            CompleteEncounter();
            return;
        }

        // Battle Enemy aktivieren & positionieren
        if (battleEnemy != null)
        {
            // Enemy Stats setzen
            CharacterStats enemyStats = battleEnemy.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.characterName = point.enemyData.enemyName;
                enemyStats.level = point.enemyData.level;
                enemyStats.maxHP = point.enemyData.maxHealth;
                enemyStats.currentHP = point.enemyData.maxHealth;
                enemyStats.attack = point.enemyData.attack;
                enemyStats.defense = point.enemyData.defense;
                enemyStats.xpReward = point.enemyData.xpReward;
                enemyStats.gold = point.enemyData.goldReward;

                if (enableDebugLogs) Debug.Log($"Enemy Stats: {enemyStats.characterName} HP:{enemyStats.maxHP} ATK:{enemyStats.attack}");
            }

            // Enemy Sprite setzen
            SpriteRenderer enemySprite = battleEnemy.GetComponent<SpriteRenderer>();
            if (enemySprite != null && point.enemyData.enemySprite != null)
            {
                enemySprite.sprite = point.enemyData.enemySprite;
            }

            // Enemy Position (rechts vom Player im Battle)
            if (player != null)
            {
                Vector3 battlePos = player.position + new Vector3(3f, 0, 0);
                battleEnemy.transform.position = battlePos;
                if (enableDebugLogs) Debug.Log($"Enemy positioned at: {battlePos}");
            }

            // Enemy aktivieren
            battleEnemy.SetActive(true);

            // GameManager Battle starten (WICHTIG!)
            if (GameManager.instance != null && enemyStats != null)
            {
                if (enableDebugLogs) Debug.Log("Calling GameManager.TriggerEncounter.. .");
                GameManager.instance.TriggerEncounter(enemyStats);
            }
            else
            {
                Debug.LogError("GameManager.instance is NULL!");
            }
        }
        else
        {
            Debug.LogError("BattleEnemy is NULL!");
            CompleteEncounter();
        }

        if (enableDebugLogs) Debug.Log("==================================");
    }

    public void OnBattleComplete(bool playerWon)
    {
        if (enableDebugLogs) Debug.Log($"========== BATTLE COMPLETE ==========");
        if (enableDebugLogs) Debug.Log($"Player Won: {playerWon}");

        // Battle Enemy verstecken
        if (battleEnemy != null)
        {
            battleEnemy.SetActive(false);
        }

        // Battle Panel verstecken
        if (battlePanel != null)
        {
            battlePanel.SetActive(false);
        }

        if (playerWon)
        {
            CompleteEncounter();
        }
        else
        {
            // Game Over
            if (enableDebugLogs) Debug.Log("GAME OVER");
        }

        if (enableDebugLogs) Debug.Log("=====================================");
    }

    void OnFlee()
    {
        if (enableDebugLogs) Debug.Log("Fled from battle!");
        CompleteEncounter();
    }

    void OnChestOpen(EncounterPoint point)
    {
        if (enableDebugLogs) Debug.Log("========== CHEST OPENED ==========");

        // Zufälliges Gold (20-80)
        int goldAmount = Random.Range(20, 81);

        if (GameManager.instance != null)
        {
            GameManager.instance.AddGold(goldAmount);
            GameManager.instance.PlayCoinSound();
        }

        if (enableDebugLogs) Debug.Log($"Found {goldAmount} Gold!");

        // Chance auf Health Potion (20%)
        bool foundPotion = Random.Range(0f, 1f) < 0.2f;

        // Baue Reward Message
        string rewardMessage = $"Du hast gefunden:\n\n{goldAmount} Gold";

        if (foundPotion)
        {
            // Lade Heiltrank aus Resources
            ShopItem potion = Resources.Load<ShopItem>("Heiltrank_Klein");

            if (potion != null && PlayerInventory.instance != null)
            {
                PlayerInventory.instance.AddPotion(potion);
                if (enableDebugLogs) Debug.Log($"Found {potion.itemName}!");
                rewardMessage += $"\n{potion.itemName}";
            }
            else
            {
                if (enableDebugLogs) Debug.LogWarning("Heiltrank_Klein not found in Resources or PlayerInventory missing!");
            }
        }

        // ZEIGE IM ENCOUNTER PANEL
        if (encounterTitleText != null)
        {
            encounterTitleText.text = "GEOEFFNET! ";
        }

        if (encounterDescriptionText != null)
        {
            encounterDescriptionText.text = rewardMessage;
        }

        // Verstecke ÖFFNEN Button, zeige nur WEITER Button
        HideAllButtons();
        ShowButton(0, "WEITER", () => HideChestMessageAndContinue());

        if (enableDebugLogs) Debug.Log($"Reward Message: {rewardMessage}");
        if (enableDebugLogs) Debug.Log("==================================");
    }

    void HideChestMessageAndContinue()
    {
        if (enableDebugLogs) Debug.Log("Continuing to Boss.. .");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Gehe direkt zum Boss
        GoToBoss();
    }

    void OnShopEnter()
    {
        if (enableDebugLogs) Debug.Log("========== SHOP ENTERED ==========");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Speichere aktuelle Player Position (Shop Position)
        if (player != null)
        {
            PlayerPrefs.SetFloat("ShopPositionX", player.position.x);
            PlayerPrefs.SetFloat("ShopPositionY", player.position.y);
            PlayerPrefs.SetFloat("ShopPositionZ", player.position.z);

            if (enableDebugLogs) Debug.Log($"Saved Shop Position: {player.position}");
        }

        // Speichere Dungeon State
        PlayerPrefs.SetString("DungeonState", "AfterShop");
        PlayerPrefs.Save();

        // Gehe zu Marketplace Scene
        SceneTransition sceneTransition = FindObjectOfType<SceneTransition>();
        if (sceneTransition != null)
        {
            sceneTransition.GoToMarketplaceFromDungeon();
        }
        else
        {
            // Fallback:  Lade Scene direkt
            if (enableDebugLogs) Debug.LogWarning("SceneTransition not found, loading Marketplace directly");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Marketplace");
        }

        if (enableDebugLogs) Debug.Log("==================================");
    }

    void OnCampfireHeal()
    {
        if (enableDebugLogs) Debug.Log("========== CAMPFIRE:  HEAL ==========");

        int healAmount = 50;

        if (playerStats != null)
        {
            int actualHeal = playerStats.Heal(healAmount);
            if (enableDebugLogs) Debug.Log($"Healed {actualHeal} HP");
        }

        // Update UI
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }

        // Zeige Ergebnis
        ShowCampfireResult($"Du ruhst dich aus.\n\nGeheilt: +{healAmount} HP");

        if (enableDebugLogs) Debug.Log("====================================");
    }

    void OnCampfireAttack()
    {
        if (enableDebugLogs) Debug.Log("========== CAMPFIRE:  ATTACK ==========");

        int attackBonus = 2;

        if (playerStats != null)
        {
            playerStats.attack += attackBonus;
            if (enableDebugLogs) Debug.Log($"Attack increased to {playerStats.attack}");
        }

        // Update UI
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }

        // Zeige Ergebnis
        ShowCampfireResult($"Du trainierst deine Kraft.\n\nAngriff:  +{attackBonus}");

        if (enableDebugLogs) Debug.Log("======================================");
    }

    void OnCampfireDefense()
    {
        if (enableDebugLogs) Debug.Log("========== CAMPFIRE:  DEFENSE ==========");

        int defenseBonus = 2;

        if (playerStats != null)
        {
            playerStats.defense += defenseBonus;
            if (enableDebugLogs) Debug.Log($"Defense increased to {playerStats.defense}");
        }

        // Update UI
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }

        // Zeige Ergebnis
        ShowCampfireResult($"Du staerkst deine Verteidigung.\n\nVerteidigung: +{defenseBonus}");

        if (enableDebugLogs) Debug.Log("=======================================");
    }

    void ShowCampfireResult(string message)
    {
        if (enableDebugLogs) Debug.Log($"Campfire Result: {message}");

        // Ändere Titel & Beschreibung
        if (encounterTitleText != null)
        {
            encounterTitleText.text = "AUSGERUHT!";
        }

        if (encounterDescriptionText != null)
        {
            encounterDescriptionText.text = message;
        }

        // Verstecke alle Buttons, zeige nur WEITER
        HideAllButtons();
        ShowButton(0, "WEITER", () => CompleteEncounter());
    }

    void OnChoiceMade(EncounterPoint chosenPoint)
    {
        if (enableDebugLogs) Debug.Log($"========== CHOICE MADE ==========");
        if (enableDebugLogs) Debug.Log($"Chosen:  {chosenPoint.name} ({chosenPoint.encounterType})");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Bewege zum gewählten Point
        MoveToPoint(chosenPoint);

        if (enableDebugLogs) Debug.Log("=================================");
    }

    void OnChoiceChest()
    {
        if (enableDebugLogs) Debug.Log("Chose Chest (fallback)");
        OnChestOpen(null);
    }

    void OnChoiceShop()
    {
        if (enableDebugLogs) Debug.Log("Chose Shop (fallback)");
        OnShopEnter();
    }

    void OnMystery()
    {
        if (enableDebugLogs) Debug.Log("Mystery encounter!");

        int random = Random.Range(0, 2);
        if (random == 0)
        {
            OnChestOpen(null);
        }
        else
        {
            CompleteEncounter();
        }
    }

    void GoToBoss()
    {
        if (enableDebugLogs) Debug.Log("========== GO TO BOSS ==========");

        // Finde Boss Point in der Liste
        bool found = false;
        for (int i = 0; i < encounterPoints.Count; i++)
        {
            if (encounterPoints[i].encounterType == EncounterType.Boss)
            {
                currentPointIndex = i;
                found = true;
                if (enableDebugLogs) Debug.Log($"Boss found at index {i}:  {encounterPoints[i].name}");

                // Bewege zu Boss
                targetPosition = encounterPoints[i].transform.position;
                isMoving = true;

                if (enableDebugLogs) Debug.Log($"Target: {targetPosition}, isMoving: {isMoving}");

                break;
            }
        }

        if (!found)
        {
            Debug.LogError("Boss point not found in list!");
        }

        if (enableDebugLogs) Debug.Log("================================");
    }

    void CompleteEncounter()
    {
        if (enableDebugLogs) Debug.Log("========== COMPLETE ENCOUNTER ==========");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Falls currentPointIndex = -1 (Choice path außerhalb Liste), nicht weitergehen
        if (currentPointIndex < 0)
        {
            if (enableDebugLogs) Debug.Log("Index is -1 (choice path) - waiting for manual navigation");
            if (enableDebugLogs) Debug.Log("========================================");
            return;
        }

        // Nächster Point
        currentPointIndex++;

        if (enableDebugLogs) Debug.Log($"Next index: {currentPointIndex}");

        MoveToNextPoint();

        if (enableDebugLogs) Debug.Log("========================================");
    }

    // ============================================
    // HELPER
    // ============================================

    string GetTitleForType(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "GEGNER! ";
            case EncounterType.Elite: return "ELITE!";
            case EncounterType.Boss: return "BOSS!";
            case EncounterType.Chest: return "SCHATZTRUHE!";
            case EncounterType.Shop: return "HAENDLER!";
            case EncounterType.Campfire: return "LAGERFEUER!";
            case EncounterType.Choice: return "WAHL!";
            case EncounterType.Mystery: return "GEHEIMNIS!";
            default: return "??? ";
        }
    }

    string GetDescriptionForType(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "Ein wilder Gegner erscheint! ";
            case EncounterType.Elite: return "Ein maechtiger Gegner versperrt den Weg!";
            case EncounterType.Boss: return "Der Boss erwartet dich!";
            case EncounterType.Chest: return "Eine glaenzende Truhe! ";
            case EncounterType.Shop: return "Ein Haendler bietet Waren an. ";
            case EncounterType.Campfire: return "Ein warmes Feuer zum Ausruhen. ";
            case EncounterType.Choice: return "Waehle deinen Weg!";
            case EncounterType.Mystery: return "Was verbirgt sich hier?";
            default: return "... ";
        }
    }

    string GetLabelForPoint(EncounterPoint point)
    {
        switch (point.encounterType)
        {
            case EncounterType.Chest: return "CHEST";
            case EncounterType.Shop: return "SHOP";
            case EncounterType.Campfire: return "RASTEN";
            default: return "WEITER";
        }
    }
}