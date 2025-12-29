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

    [Header("Encounter Points")]
    public List<EncounterPoint> encounterPoints = new List<EncounterPoint>();
    private int currentPointIndex = 0;
    private EncounterPoint currentChoicePoint;

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
        if (enableDebugLogs) Debug.Log("🏰 DungeonManager START");

        // Buttons sammeln
        if (button1 != null) allButtons.Add(button1);
        if (button2 != null) allButtons.Add(button2);
        if (button3 != null) allButtons.Add(button3);
        if (button4 != null) allButtons.Add(button4);

        if (enableDebugLogs) Debug.Log($"✓ {allButtons.Count} buttons found");

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
            if (enableDebugLogs) Debug.Log($"✓ Player at Start Point: {encounterPoints[0].name}");
        }

        // Starte Dungeon (überspringe Point 0 = Start)
        currentPointIndex = 1;
        MoveToNextPoint();
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
            if (enableDebugLogs) Debug.Log("🏁 Dungeon Complete!");
            return;
        }

        if (enableDebugLogs) Debug.Log($"➡️ Moving to Point {currentPointIndex}:  {encounterPoints[currentPointIndex].name}");

        targetPosition = encounterPoints[currentPointIndex].transform.position;
        isMoving = true;
    }

    void MoveToPoint(EncounterPoint targetPoint)
    {
        if (enableDebugLogs) Debug.Log($"➡️ Moving to: {targetPoint.name}");

        targetPosition = targetPoint.transform.position;
        isMoving = true;

        // Finde Index von targetPoint in der Liste
        for (int i = 0; i < encounterPoints.Count; i++)
        {
            if (encounterPoints[i] == targetPoint)
            {
                currentPointIndex = i;
                if (enableDebugLogs) Debug.Log($"✓ Updated index to {i}");
                break;
            }
        }
    }

    void OnReachedPoint()
    {
        if (currentPointIndex >= encounterPoints.Count) return;

        EncounterPoint point = encounterPoints[currentPointIndex];

        if (enableDebugLogs) Debug.Log($"📍 Reached:  {point.name} ({point.encounterType})");

        // Zeige Encounter
        ShowEncounter(point);
    }

    // ============================================
    // ENCOUNTER UI
    // ============================================

    void ShowEncounter(EncounterPoint point)
    {
        if (encounterPanel == null) return;

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
                ShowButton(0, "⚔️ KÄMPFEN", () => OnBattleStart(point));
                ShowButton(1, "🏃 FLIEHEN", () => OnFlee());
                break;

            case EncounterType.Chest:
                ShowButton(0, "📦 ÖFFNEN", () => OnChestOpen(point));
                ShowButton(1, "➡️ WEITER", () => CompleteEncounter());
                break;

            case EncounterType.Shop:
                ShowButton(0, "🛒 HANDELN", () => OnShopEnter());
                ShowButton(1, "➡️ WEITER", () => CompleteEncounter());
                break;

            case EncounterType.Campfire:
                ShowButton(0, "🔥 RASTEN", () => OnRest(point));
                ShowButton(1, "➡️ WEITER", () => CompleteEncounter());
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
                    // Fallback (falls nicht zugewiesen)
                    Debug.LogWarning("⚠️ Choice options not assigned!  Using fallback.");
                    ShowButton(0, "🎁 CHEST", () => OnChoiceChest());
                    ShowButton(1, "🛒 SHOP", () => OnChoiceShop());
                }
                break;

            case EncounterType.Mystery:
                ShowButton(0, "❓ ÖFFNEN", () => OnMystery());
                break;
        }
    }

    void ShowButton(int index, string label, UnityEngine.Events.UnityAction onClick)
    {
        if (index >= allButtons.Count)
        {
            Debug.LogWarning($"⚠️ Button index {index} out of range!  Only {allButtons.Count} buttons available.");
            return;
        }

        Button btn = allButtons[index];
        if (btn == null)
        {
            Debug.LogWarning($"⚠️ Button {index} is NULL!");
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
        else
        {
            Debug.LogWarning($"⚠️ Button {index} has no TextMeshProUGUI child!");
        }

        // Click Listener
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);

        if (enableDebugLogs) Debug.Log($"✓ Button {index}:  {label}");
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
        if (enableDebugLogs) Debug.Log("⚔️ Battle Start!");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        // TODO: Trigger GameManager Battle
        // Für jetzt: Auto-win nach 2 Sekunden
        Invoke("CompleteEncounter", 2f);
    }

    void OnFlee()
    {
        if (enableDebugLogs) Debug.Log("🏃 Fled!");
        CompleteEncounter();
    }

    void OnChestOpen(EncounterPoint point)
    {
        if (enableDebugLogs) Debug.Log("📦 Chest opened!");

        // Gebe Gold
        if (GameManager.instance != null)
        {
            GameManager.instance.AddGold(50);
            GameManager.instance.PlayCoinSound();
        }

        CompleteEncounter();
    }

    void OnShopEnter()
    {
        if (enableDebugLogs) Debug.Log("🛒 Shop entered!");
        CompleteEncounter();
    }

    void OnRest(EncounterPoint point)
    {
        if (enableDebugLogs) Debug.Log("🔥 Resting!");

        // Heile Player
        if (GameManager.instance != null && GameManager.instance.playerStats != null)
        {
            GameManager.instance.playerStats.Heal(30);
            GameManager.instance.UpdateUI();
        }

        CompleteEncounter();
    }

    void OnChoiceMade(EncounterPoint chosenPoint)
    {
        if (enableDebugLogs) Debug.Log($"🚪 Chose: {chosenPoint.name} ({chosenPoint.encounterType})");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Bewege zum gewählten Point
        MoveToPoint(chosenPoint);
    }

    void OnChoiceChest()
    {
        if (enableDebugLogs) Debug.Log("🎁 Chose Chest (fallback)!");
        OnChestOpen(null);
    }

    void OnChoiceShop()
    {
        if (enableDebugLogs) Debug.Log("🛒 Chose Shop (fallback)!");
        OnShopEnter();
    }

    void OnMystery()
    {
        if (enableDebugLogs) Debug.Log("❓ Mystery!");

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

    void CompleteEncounter()
    {
        if (enableDebugLogs) Debug.Log("✅ Encounter Complete");

        // Panel verstecken
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        HideAllButtons();

        // Nächster Point
        currentPointIndex++;
        MoveToNextPoint();
    }

    // ============================================
    // HELPER
    // ============================================

    string GetTitleForType(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "⚔️ GEGNER! ";
            case EncounterType.Elite: return "👹 ELITE!";
            case EncounterType.Boss: return "👑 BOSS!";
            case EncounterType.Chest: return "📦 SCHATZTRUHE!";
            case EncounterType.Shop: return "🛒 HÄNDLER!";
            case EncounterType.Campfire: return "🔥 LAGERFEUER!";
            case EncounterType.Choice: return "🚪 WAHL!";
            case EncounterType.Mystery: return "❓ GEHEIMNIS!";
            default: return "??? ";
        }
    }

    string GetDescriptionForType(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "Ein wilder Gegner erscheint! ";
            case EncounterType.Elite: return "Ein mächtiger Gegner versperrt den Weg!";
            case EncounterType.Boss: return "Der Boss erwartet dich!";
            case EncounterType.Chest: return "Eine glänzende Truhe! ";
            case EncounterType.Shop: return "Ein Händler bietet Waren an. ";
            case EncounterType.Campfire: return "Ein warmes Feuer zum Ausruhen. ";
            case EncounterType.Choice: return "Wähle deinen Weg! ";
            case EncounterType.Mystery: return "Was verbirgt sich hier?";
            default: return "... ";
        }
    }

    string GetLabelForPoint(EncounterPoint point)
    {
        switch (point.encounterType)
        {
            case EncounterType.Chest: return "🎁 CHEST";
            case EncounterType.Shop: return "🛒 SHOP";
            case EncounterType.Campfire: return "🔥 RASTEN";
            default: return "➡️ WEITER";
        }
    }
}