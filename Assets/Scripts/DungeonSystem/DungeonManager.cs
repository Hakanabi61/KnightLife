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

        // Player Stats holen
        if (player != null && playerStats == null)
        {
            playerStats = player.GetComponent<CharacterStats>();
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

        // Player zum Start Point bewegen
        if (encounterPoints.Count > 0 && player != null)
        {
            player.position = encounterPoints[0].transform.position;
            if (enableDebugLogs) Debug.Log($"Player at Start:  {encounterPoints[0].name}");
        }

        // Starte Dungeon (überspringe Point 0 = Start)
        currentPointIndex = 1;
        MoveToNextPoint();

        if (enableDebugLogs) Debug.Log("===================================");
    }

    void Update()
    {
        // Player Movement
        if (isMoving && player != null)
        {
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
            // Point ist NICHT in der Liste (z.B.  Chest/Shop bei Choice)
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
                ShowButton(0, "RASTEN", () => OnRest(point));
                ShowButton(1, "WEITER", () => CompleteEncounter());
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

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

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
            }

            // Enemy Sprite setzen
            SpriteRenderer enemySprite = battleEnemy.GetComponent<SpriteRenderer>();
            if (enemySprite != null && point.enemyData.enemySprite != null)
            {
                enemySprite.sprite = point.enemyData.enemySprite;
            }

            // Enemy Position (rechts neben Player)
            if (player != null)
            {
                battleEnemy.transform.position = player.position + new Vector3(3f, 0, 0);
            }

            // Enemy aktivieren
            battleEnemy.SetActive(true);

            // Battle Panel öffnen
            if (battlePanel != null)
            {
                battlePanel.SetActive(true);
            }

            // GameManager Battle starten
            if (GameManager.instance != null && enemyStats != null)
            {
                GameManager.instance.TriggerEncounter(enemyStats);
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

        // Gebe Gold
        if (GameManager.instance != null)
        {
            GameManager.instance.AddGold(50);
            GameManager.instance.PlayCoinSound();
        }

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Gehe direkt zum Boss
        GoToBoss();

        if (enableDebugLogs) Debug.Log("==================================");
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

        // Gehe direkt zum Boss
        GoToBoss();

        if (enableDebugLogs) Debug.Log("==================================");
    }

    void OnRest(EncounterPoint point)
    {
        if (enableDebugLogs) Debug.Log("Resting at campfire!");

        // Heile Player
        if (playerStats != null)
        {
            playerStats.Heal(30);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }

        CompleteEncounter();
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
            case EncounterType.Mystery: return "GEHEIMNIS! ";
            default: return "??? ";
        }
    }

    string GetDescriptionForType(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "Ein wilder Gegner erscheint!";
            case EncounterType.Elite: return "Ein maechtiger Gegner versperrt den Weg!";
            case EncounterType.Boss: return "Der Boss erwartet dich!";
            case EncounterType.Chest: return "Eine glaenzende Truhe! ";
            case EncounterType.Shop: return "Ein Haendler bietet Waren an. ";
            case EncounterType.Campfire: return "Ein warmes Feuer zum Ausruhen.";
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