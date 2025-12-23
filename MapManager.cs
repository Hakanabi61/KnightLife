using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the map, player movement, and node events
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Player")]
    public Transform playerIcon;
    public float moveSpeed = 3f;

    [Header("Current State")]
    public MapNode currentNode;
    private MapNode targetNode;
    private bool isMoving = false;

    [Header("UI")]
    public GameObject eventPanel; // Panel für Events (Kampf, Chest, etc.)
    public TMPro.TextMeshProUGUI eventText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("?? MapManager.Start() CALLED!");

        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
            Debug.Log("? Event panel hidden");
        }
        else
        {
            Debug.LogWarning("?? Event panel is NULL!");
        }

        if (playerIcon == null)
        {
            Debug.LogError("? PLAYER ICON IS NULL!  Drag PlayerIcon into MapManager!");
            return;
        }
        else
        {
            Debug.Log($"? Player Icon found: {playerIcon.name}");
        }

        // Find start node
        MapNode[] allNodes = FindObjectsOfType<MapNode>();
        Debug.Log($"?? Found {allNodes.Length} nodes in scene");

        foreach (MapNode node in allNodes)
        {
            Debug.Log($"?? Node found: {node.gameObject.name} - Type: {node.nodeType}");

            if (node.nodeType == NodeType.Start)
            {
                currentNode = node;
                currentNode.SetAccessible(true);
                currentNode.SetCurrent(true);
                currentNode.Complete();

                playerIcon.position = currentNode.transform.position;

                Debug.Log($"?? START NODE SET:  {currentNode.gameObject.name} at position {currentNode.transform.position}");
                Debug.Log($"?? Player icon moved to: {playerIcon.position}");
                break;
            }
        }

        if (currentNode == null)
        {
            Debug.LogError("? NO START NODE FOUND!  Create a node with NodeType.Start!");
        }
    }

    void Update()
    {
        if (isMoving && targetNode != null && playerIcon != null)
        {
            playerIcon.position = Vector3.MoveTowards(
                playerIcon.position,
                targetNode.transform.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(playerIcon.position, targetNode.transform.position) < 0.1f)
            {
                isMoving = false;
                OnNodeReached(targetNode);
            }
        }
    }

    public void OnNodeClicked(MapNode node)
    {
        if (isMoving) return;

        Debug.Log($"?? Moving to node:  {node.gameObject.name} ({node.nodeType})");

        if (currentNode != null)
        {
            currentNode.SetCurrent(false);
        }

        targetNode = node;
        targetNode.SetCurrent(true);
        isMoving = true;
    }

    void OnNodeReached(MapNode node)
    {
        currentNode = node;

        Debug.Log($"?? Reached node: {node.gameObject.name} ({node.nodeType})");

        // Trigger event based on node type
        switch (node.nodeType)
        {
            case NodeType.Enemy:
                StartEnemyEncounter();
                break;
            case NodeType.Chest:
                OpenChest();
                break;
            case NodeType.Mystery:
                TriggerMystery();
                break;
            case NodeType.Boss:
                StartBossFight();
                break;
            case NodeType.Shop:
                OpenShop();
                break;
            case NodeType.Rest:
                RestAtCampfire();
                break;
        }
    }

    void StartEnemyEncounter()
    {
        Debug.Log("?? ENEMY ENCOUNTER!");
        ShowEvent("?? GEGNER ERSCHEINT!\n\nBereit für den Kampf? ");

        // TODO: Start battle (später mit deinem Battle-System verbinden)
        // Für jetzt:  Auto-complete nach 2 Sekunden
        Invoke("CompleteCurrentNode", 2f);
    }

    void OpenChest()
    {
        int goldReward = Random.Range(50, 150);

        Debug.Log($"?? CHEST OPENED! +{goldReward} Gold");
        ShowEvent($"?? TRUHE GEÖFFNET!\n\n+{goldReward} Gold erhalten!");

        if (GameData.instance != null)
        {
            GameData.instance.playerGold += goldReward;
        }

        Invoke("CompleteCurrentNode", 2f);
    }

    void TriggerMystery()
    {
        Debug.Log("? MYSTERY NODE!");

        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                ShowEvent("? MYSTERIUM!\n\nEine versteckte Truhe!");
                Invoke("OpenChest", 1f);
                break;
            case 1:
                ShowEvent("? MYSTERIUM!\n\nEin Gegner lauert!");
                Invoke("StartEnemyEncounter", 1f);
                break;
            case 2:
                ShowEvent("? MYSTERIUM!\n\nNichts passiert...  Glück gehabt!");
                Invoke("CompleteCurrentNode", 2f);
                break;
        }
    }

    void StartBossFight()
    {
        Debug.Log("?? BOSS BATTLE!");
        ShowEvent("?? BOSS ERSCHEINT!\n\nEin mächtiger Gegner!");

        Invoke("CompleteCurrentNode", 3f);
    }

    void OpenShop()
    {
        Debug.Log("?? SHOP!");
        ShowEvent("?? HÄNDLER GEFUNDEN!\n\nWillkommen!");

        Invoke("CompleteCurrentNode", 2f);
    }

    void RestAtCampfire()
    {
        Debug.Log("?? RESTING.. .");
        ShowEvent("?? LAGERFEUER!\n\nHP vollständig wiederhergestellt!");

        if (GameData.instance != null)
        {
            GameData.instance.playerHP = GameData.instance.playerMaxHP;
        }

        Invoke("CompleteCurrentNode", 2f);
    }

    void CompleteCurrentNode()
    {
        if (currentNode != null)
        {
            currentNode.Complete();

            if (GameData.instance != null)
            {
                GameData.instance.nodesCompleted++;
            }
        }

        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }

    void ShowEvent(string message)
    {
        if (eventPanel != null && eventText != null)
        {
            eventText.text = message;
            eventPanel.SetActive(true);
        }
    }
}