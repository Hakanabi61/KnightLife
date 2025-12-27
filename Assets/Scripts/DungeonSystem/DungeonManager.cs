using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the dungeon flow, player movement, and encounters
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;

    [Header("Player")]
    public Transform player;
    public float moveSpeed = 3f;
    public Animator playerAnimator;

    [Header("Dungeon")]
    public EncounterPoint startPoint;
    public EncounterPoint currentPoint;
    private EncounterPoint targetPoint;
    private bool isMoving = false;

    [Header("UI")]
    public GameObject encounterPanel;
    public TextMeshProUGUI encounterTitleText;
    public TextMeshProUGUI encounterDescriptionText;
    public Transform buttonContainer;

    [Header("Choice UI")]
    public GameObject choicePanel;
    public GameObject choiceButtonPrefab;
    public Transform choiceButtonContainer;

    [Header("Camera Follow")]
    public Camera mainCamera;
    public bool enableCameraFollow = true;
    public float cameraFollowSpeed = 3f;
    public float cameraYOffset = 2f;
    public Vector2 cameraBounds = new Vector2(-6f, 10f);

    void Awake()
    {
        Debug.Log("========== AWAKE CALLED ==========");
        Debug.Log($"🔍 This GameObject:  {gameObject.name}");
        Debug.Log($"🔍 Instance before check: {(instance != null ? "EXISTS (another DungeonManager! )" : "NULL (first one)")}");

        // SINGLETON CHECK
        if (instance == null)
        {
            Debug.Log("✅ No existing instance - Setting this as instance");
            instance = this;
        }
        else
        {
            Debug.LogError($"❌❌❌ DUPLICATE FOUND! Instance exists:  {instance.gameObject.name}");
            Debug.LogError($"❌❌❌ Destroying THIS:  {gameObject.name}");
            Debug.LogError($"❌❌❌ Check your scene for multiple DungeonManagers!");
            Destroy(gameObject);
            return;
        }

        Debug.Log("✅ Awake complete - Instance set");
        Debug.Log("==================================");
    }

    void Start()
    {
        Debug.Log("==============================================");
        Debug.Log("🏰 DungeonManager START");
        Debug.Log("==============================================");

        // Main Camera automatisch finden falls nicht zugewiesen
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.Log($"📷 Main Camera auto-found: {(mainCamera != null ? "YES" : "NO")}");
        }

        // UI verstecken
        if (encounterPanel != null) encounterPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);

        // ========== DEBUG:  SETUP CHECK ==========
        Debug.Log("---------- SETUP CHECK ----------");
        Debug.Log($"✓ Player assigned: {(player != null ? player.name : "❌ NULL")}");
        Debug.Log($"✓ Start Point assigned: {(startPoint != null ? startPoint.name : "❌ NULL")}");
        Debug.Log($"✓ Move Speed: {moveSpeed}");
        Debug.Log($"✓ Main Camera: {(mainCamera != null ? mainCamera.name : "❌ NULL")}");
        Debug.Log($"✓ Camera Follow Enabled: {enableCameraFollow}");

        if (player != null)
        {
            Debug.Log($"✓ Player Start Position: {player.position}");
        }

        if (startPoint != null)
        {
            Debug.Log($"✓ Start Point Position: {startPoint.transform.position}");
            Debug.Log($"✓ Start Point Next:  {(startPoint.nextPoint != null ? startPoint.nextPoint.name : "❌ NULL")}");
        }
        Debug.Log("----------------------------------");

        // Start Point finden falls nicht zugewiesen
        if (startPoint == null)
        {
            Debug.LogWarning("⚠️ Start Point not assigned!  Searching for Start type...");
            EncounterPoint[] allPoints = FindObjectsByType<EncounterPoint>(FindObjectsSortMode.None);
            foreach (EncounterPoint point in allPoints)
            {
                if (point.encounterType == EncounterType.Start)
                {
                    startPoint = point;
                    Debug.Log($"✓ Found Start Point: {startPoint.name}");
                    break;
                }
            }
        }

        if (startPoint != null)
        {
            currentPoint = startPoint;
            Debug.Log($"🎯 Current Point set to: {currentPoint.name}");

            // Player zum Start bewegen
            if (player != null)
            {
                player.position = startPoint.transform.position;
                Debug.Log($"🚶 Player moved to Start Point: {player.position}");
            }
            else
            {
                Debug.LogError("❌ PLAYER IS NULL!  Cannot position player!");
            }

            // Camera zum Start positionieren
            if (mainCamera != null && enableCameraFollow)
            {
                Vector3 startCameraPos = new Vector3(
                    Mathf.Clamp(startPoint.transform.position.x, cameraBounds.x, cameraBounds.y),
                    cameraYOffset,
                    mainCamera.transform.position.z
                );
                mainCamera.transform.position = startCameraPos;
                Debug.Log($"📷 Camera positioned at: {mainCamera.transform.position}");
            }

            // Start Point komplettieren und zum nächsten gehen
            startPoint.Complete();
            Debug.Log("✅ Start Point completed");

            Debug.Log("🚀 Calling MoveToNextPoint().. .");
            MoveToNextPoint();
        }
        else
        {
            Debug.LogError("❌❌❌ NO START POINT FOUND! Create an EncounterPoint with type 'Start'");
        }

        Debug.Log("==============================================");
        Debug.Log("🏰 DungeonManager START COMPLETE");
        Debug.Log("==============================================");
    }

    void Update()
    {
        // ============================================
        // PLAYER MOVEMENT DEBUG
        // ============================================

        if (isMoving)
        {
            if (targetPoint == null)
            {
                Debug.LogError("❌ isMoving=true but targetPoint is NULL!");
                isMoving = false;
                return;
            }

            if (player == null)
            {
                Debug.LogError("❌ isMoving=true but player is NULL!");
                isMoving = false;
                return;
            }

            float distance = Vector3.Distance(player.position, targetPoint.transform.position);
            Debug.Log($"⏩ MOVING:  {player.position: F2} → {targetPoint.transform.position:F2} | Dist: {distance:F2} | Speed: {moveSpeed}");
        }

        // ============================================
        // PLAYER MOVEMENT
        // ============================================

        if (isMoving && targetPoint != null && player != null)
        {
            // Richtung berechnen
            Vector3 direction = (targetPoint.transform.position - player.position).normalized;
            Vector3 oldPosition = player.position;

            // Player bewegen
            player.position = Vector3.MoveTowards(
                player.position,
                targetPoint.transform.position,
                moveSpeed * Time.deltaTime
            );

            Vector3 newPosition = player.position;
            Vector3 actualMovement = newPosition - oldPosition;

            if (actualMovement.magnitude > 0.001f)
            {
                Debug.Log($"   → Moved {actualMovement.magnitude:F3} units | New Pos: {newPosition:F2}");
            }
            else
            {
                Debug.LogWarning("⚠️ Player position NOT changing!  Movement blocked? ");
            }

            // Animation (falls vorhanden)
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isRunning", true);

                // Sprite Flip
                if (direction.x > 0)
                {
                    player.localScale = new Vector3(1, 1, 1);
                }
                else if (direction.x < 0)
                {
                    player.localScale = new Vector3(-1, 1, 1);
                }
            }

            // Angekommen? 
            float distanceToTarget = Vector3.Distance(player.position, targetPoint.transform.position);
            if (distanceToTarget < 0.1f)
            {
                Debug.Log($"🎯 ARRIVED at {targetPoint.name}!  Distance: {distanceToTarget:F3}");
                isMoving = false;

                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isRunning", false);
                }

                OnPointReached(targetPoint);
            }
        }

        // ============================================
        // CAMERA FOLLOW
        // ============================================

        UpdateCameraFollow();
    }

    // ============================================
    // CAMERA SYSTEM
    // ============================================

    void UpdateCameraFollow()
    {
        if (!enableCameraFollow || mainCamera == null || player == null) return;

        // Ziel Position berechnen
        float targetX = player.position.x;

        // Bounds anwenden
        targetX = Mathf.Clamp(targetX, cameraBounds.x, cameraBounds.y);

        Vector3 targetPosition = new Vector3(
            targetX,
            cameraYOffset,
            mainCamera.transform.position.z
        );

        // Smooth Follow
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );
    }

    public void SetCameraBounds(float minX, float maxX)
    {
        cameraBounds = new Vector2(minX, maxX);
        Debug.Log($"📷 Camera bounds set:  {minX} to {maxX}");
    }

    // ============================================
    // MOVEMENT FUNCTIONS
    // ============================================

    public void MoveToNextPoint()
    {
        Debug.Log("---------- MoveToNextPoint() called ----------");

        if (currentPoint == null)
        {
            Debug.LogError("❌ Current point is NULL!");
            return;
        }

        Debug.Log($"Current Point: {currentPoint.name} (Type: {currentPoint.encounterType})");

        // Check ob Choice Point
        if (currentPoint.encounterType == EncounterType.Choice)
        {
            Debug.Log("🚪 Current point is CHOICE - showing UI");
            ShowChoiceUI();
            return;
        }

        // Normaler nächster Punkt
        if (currentPoint.nextPoint != null)
        {
            Debug.Log($"✓ Next point found: {currentPoint.nextPoint.name}");
            MoveToPoint(currentPoint.nextPoint);
        }
        else
        {
            Debug.Log("🏁 No next point - End of dungeon reached!");
        }

        Debug.Log("----------------------------------------------");
    }

    public void MoveToPoint(EncounterPoint point)
    {
        Debug.Log("========== MoveToPoint() ==========");

        if (point == null)
        {
            Debug.LogError("❌ Cannot move to NULL point!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ Player is NULL!  Cannot move!");
            return;
        }

        Debug.Log($"🎯 Target:  {point.gameObject.name} ({point.encounterType})");
        Debug.Log($"   Position: {point.transform.position}");
        Debug.Log($"   Player current position: {player.position}");
        Debug.Log($"   Distance:  {Vector3.Distance(player.position, point.transform.position):F2}");

        targetPoint = point;
        targetPoint.Activate();
        isMoving = true;

        Debug.Log($"✓ isMoving set to TRUE");
        Debug.Log($"✓ Move Speed: {moveSpeed}");
        Debug.Log("===================================");
    }

    void OnPointReached(EncounterPoint point)
    {
        Debug.Log("========== OnPointReached() ==========");
        Debug.Log($"📍 Reached:  {point.gameObject.name} ({point.encounterType})");

        currentPoint = point;

        // Trigger Event basierend auf Typ
        switch (point.encounterType)
        {
            case EncounterType.Enemy:
                TriggerEnemyEncounter();
                break;
            case EncounterType.Elite:
                TriggerEliteEncounter();
                break;
            case EncounterType.Chest:
                TriggerChest();
                break;
            case EncounterType.Shop:
                TriggerShop();
                break;
            case EncounterType.Campfire:
                TriggerCampfire();
                break;
            case EncounterType.Mystery:
                TriggerMystery();
                break;
            case EncounterType.Choice:
                ShowChoiceUI();
                break;
            case EncounterType.Boss:
                TriggerBoss();
                break;
        }

        Debug.Log("======================================");
    }

    // ============================================
    // ENCOUNTER TRIGGERS
    // ============================================

    void TriggerEnemyEncounter()
    {
        Debug.Log("⚔️ ENEMY ENCOUNTER!");
        ShowEncounter(
            "⚔️ GEGNER! ",
            "Ein wilder Gegner erscheint!",
            new string[] { "Kämpfen", "Fliehen" }
        );
    }

    void TriggerEliteEncounter()
    {
        Debug.Log("👹 ELITE ENEMY!");
        ShowEncounter(
            "👹 ELITE GEGNER!",
            "Ein mächtiger Gegner versperrt den Weg!",
            new string[] { "Kämpfen" }
        );
    }

    void TriggerChest()
    {
        Debug.Log("📦 CHEST!");
        ShowEncounter(
            "📦 SCHATZTRUHE!",
            "Eine glänzende Truhe steht vor dir!",
            new string[] { "Öffnen" }
        );
    }

    void TriggerShop()
    {
        Debug.Log("🏪 SHOP!");
        ShowEncounter(
            "🏪 HÄNDLER!",
            "Ein freundlicher Händler bietet seine Waren an.",
            new string[] { "Handeln", "Weiter gehen" }
        );
    }

    void TriggerCampfire()
    {
        Debug.Log("💤 CAMPFIRE!");
        ShowEncounter(
            "🔥 LAGERFEUER!",
            "Ein warmes Lagerfeuer lädt zum Ausruhen ein.",
            new string[] { "Rasten (+50 HP)", "Weiter gehen" }
        );
    }

    void TriggerMystery()
    {
        Debug.Log("❓ MYSTERY!");

        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                TriggerChest();
                break;
            case 1:
                TriggerEnemyEncounter();
                break;
            case 2:
                ShowEncounter(
                    "❓ NICHTS.. .",
                    "Es ist niemand hier...  Glück gehabt!",
                    new string[] { "Weiter" }
                );
                break;
        }
    }

    void TriggerBoss()
    {
        Debug.Log("👑 BOSS FIGHT!");
        ShowEncounter(
            "👑 BOSS!",
            "Der mächtige Endgegner erwartet dich! ",
            new string[] { "KÄMPFEN!" }
        );
    }

    // ============================================
    // UI FUNCTIONS
    // ============================================

    /// <summary>
    /// Zeigt Encounter UI mit Optionen
    /// </summary>
    void ShowEncounter(string title, string description, string[] buttonLabels)
    {
        Debug.Log($"📋 ShowEncounter: {title}");

        if (encounterPanel == null)
        {
            Debug.LogWarning("⚠️ Encounter Panel is NULL! Auto-completing in 2 seconds.. .");
            Invoke("CompleteEncounter", 2f);
            return;
        }

        // Panel anzeigen
        encounterPanel.SetActive(true);

        // Texte setzen
        if (encounterTitleText != null)
        {
            encounterTitleText.text = title;
        }

        if (encounterDescriptionText != null)
        {
            encounterDescriptionText.text = description;
        }

        // Alte Buttons löschen
        if (buttonContainer != null)
        {
            foreach (Transform child in buttonContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Neue Buttons erstellen
        if (buttonContainer != null && buttonLabels != null)
        {
            foreach (string label in buttonLabels)
            {
                CreateEncounterButton(label);
            }
        }

        Debug.Log($"✅ Encounter shown with {buttonLabels.Length} buttons");
    }

    /// <summary>
    /// Erstellt einen Button für Encounter
    /// </summary>
    void CreateEncounterButton(string label)
    {
        // Button GameObject erstellen
        GameObject buttonObj = new GameObject($"Button_{label}");
        buttonObj.transform.SetParent(buttonContainer, false);

        // Button Component
        Button button = buttonObj.AddComponent<Button>();

        // Image (Background) - SCHÖNER!
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.5f, 0.8f, 1f); // Schönes Blau! 

        // Color Block für Hover/Press - KONTRASTREICHER!
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.8f, 1f); // Blau
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f, 1f); // Helleres Blau
        colors.pressedColor = new Color(0.1f, 0.7f, 1f, 1f); // Cyan
        colors.selectedColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        // Layout Element - GRÖSSERE BUTTONS!
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.minWidth = 200;
        layout.minHeight = 80; // Höher! 
        layout.flexibleWidth = 1;
        layout.preferredHeight = 80;

        // Text - GRÖßER & SCHÖNER!
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 32; // VIEL GRÖßER!
        text.fontStyle = FontStyles.Bold; // BOLD!
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        // Outline für bessere Lesbarkeit
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.5f);

        // RectTransform für Text (Fill Parent)
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 10); // Padding
        textRect.offsetMax = new Vector2(-10, -10);

        // Button Click Event
        button.onClick.AddListener(() => OnEncounterButtonClicked(label));

        Debug.Log($"   ✅ Button created: {label}");
    }

    /// <summary>
    /// Wird aufgerufen wenn Encounter Button geklickt wird
    /// </summary>
    void OnEncounterButtonClicked(string buttonLabel)
    {
        Debug.Log($"========== BUTTON CLICKED: {buttonLabel} ==========");

        // Je nach Button verschiedene Aktionen
        switch (buttonLabel)
        {
            case "Kämpfen":
            case "KÄMPFEN! ":
                Debug.Log("⚔️ Starting battle.. .");
                // TODO: Später Battle System starten! 
                CompleteEncounter();
                break;

            case "Fliehen":
                Debug.Log("🏃 Fleeing.. .");
                CompleteEncounter();
                break;

            case "Öffnen":
                Debug.Log("📦 Opening chest...");
                // TODO: Give items/gold
                CompleteEncounter();
                break;

            case "Handeln":
                Debug.Log("🏪 Opening shop...");
                // TODO:  Open shop UI
                CompleteEncounter();
                break;

            case "Weiter gehen":
            case "Weiter":
                Debug.Log("🚶 Moving on...");
                CompleteEncounter();
                break;

            case "Rasten (+50 HP)":
                Debug.Log("💤 Resting.. .");
                // TODO: Heal player
                CompleteEncounter();
                break;

            default:
                Debug.Log($"Unknown button: {buttonLabel}");
                CompleteEncounter();
                break;
        }

        Debug.Log("===============================================");
    }

    /// <summary>
    /// Zeigt Choice UI (Branching Paths)
    /// </summary>
    void ShowChoiceUI()
    {
        Debug.Log("========== ShowChoiceUI() ==========");

        if (choicePanel == null)
        {
            Debug.LogWarning("⚠️ Choice Panel is NULL!");
            MoveToNextPoint();
            return;
        }

        if (currentPoint == null || currentPoint.choiceOptions == null || currentPoint.choiceOptions.Length == 0)
        {
            Debug.LogWarning("⚠️ No choice options!");
            MoveToNextPoint();
            return;
        }

        Debug.Log($"🚪 CHOICE POINT!  {currentPoint.choiceOptions.Length} options");

        // Panel anzeigen
        choicePanel.SetActive(true);

        // Alte Buttons löschen
        if (choiceButtonContainer != null)
        {
            foreach (Transform child in choiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Choice Buttons erstellen
        for (int i = 0; i < currentPoint.choiceOptions.Length; i++)
        {
            EncounterPoint option = currentPoint.choiceOptions[i];
            if (option != null)
            {
                CreateChoiceButton(option, i);
            }
        }

        Debug.Log("====================================");
    }

    /// <summary>
    /// Erstellt einen Choice Button
    /// </summary>
    void CreateChoiceButton(EncounterPoint targetPoint, int index)
    {
        if (choiceButtonPrefab == null || choiceButtonContainer == null)
        {
            Debug.LogError("❌ Choice Button Prefab or Container is NULL!");
            return;
        }

        // Instantiate Prefab
        GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);

        // Finde Text Components
        TextMeshProUGUI titleText = buttonObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = buttonObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();

        // Setze Texte basierend auf Encounter Type
        string icon = GetEncounterIcon(targetPoint.encounterType);
        string title = $"{icon} {targetPoint.encounterType.ToString().ToUpper()}";
        string description = GetEncounterDescription(targetPoint.encounterType);

        if (titleText != null) titleText.text = title;
        if (descText != null) descText.text = description;

        // Button Click Event
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnChoiceButtonClicked(targetPoint));
        }

        Debug.Log($"   ✅ Choice button created: {title}");
    }

    /// <summary>
    /// Icon für Encounter Type
    /// </summary>
    string GetEncounterIcon(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "⚔️";
            case EncounterType.Elite: return "👹";
            case EncounterType.Chest: return "📦";
            case EncounterType.Shop: return "🏪";
            case EncounterType.Campfire: return "🔥";
            case EncounterType.Mystery: return "❓";
            case EncounterType.Boss: return "👑";
            default: return "🚪";
        }
    }

    /// <summary>
    /// Beschreibung für Encounter Type
    /// </summary>
    string GetEncounterDescription(EncounterType type)
    {
        switch (type)
        {
            case EncounterType.Enemy: return "Kampf | Normale Belohnung";
            case EncounterType.Elite: return "Schwerer Kampf | Große Belohnung ⭐⭐⭐";
            case EncounterType.Chest: return "Garantierte Belohnung | Sicher";
            case EncounterType.Shop: return "Items kaufen | Gold ausgeben";
            case EncounterType.Campfire: return "Heilen | HP wiederherstellen";
            case EncounterType.Mystery: return "Zufälliges Event | Risiko/Belohnung";
            case EncounterType.Boss: return "Endgegner | Größte Herausforderung! ";
            default: return "Unbekannt";
        }
    }

    /// <summary>
    /// Choice Button Click Handler
    /// </summary>
    void OnChoiceButtonClicked(EncounterPoint chosenPoint)
    {
        Debug.Log($"========== CHOICE SELECTED: {chosenPoint.name} ==========");

        ChoosePath(chosenPoint);

        Debug.Log("==================================================");
    }

    /// <summary>
    /// Schließt Encounter und geht weiter
    /// </summary>
    public void CompleteEncounter()
    {
        Debug.Log("========== CompleteEncounter() ==========");

        if (currentPoint != null)
        {
            currentPoint.Complete();
            Debug.Log($"✓ {currentPoint.name} marked as completed");
        }

        if (encounterPanel != null)
        {
            encounterPanel.SetActive(false);
        }

        Debug.Log("🚀 Calling MoveToNextPoint()...");
        MoveToNextPoint();

        Debug.Log("=========================================");
    }

    /// <summary>
    /// Player wählt einen Path (bei Choice Point)
    /// </summary>
    public void ChoosePath(EncounterPoint chosenPoint)
    {
        Debug.Log($"========== ChoosePath:  {chosenPoint.name} ==========");

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        if (currentPoint != null)
        {
            currentPoint.Complete();
        }

        MoveToPoint(chosenPoint);

        Debug.Log("================================================");
    }
}