using UnityEngine;

public enum NodeType
{
    Start,
    Enemy,
    Chest,
    Mystery,
    Boss,
    Shop,
    Rest
}

/// <summary>
/// Represents a single node on the map
/// </summary>
public class MapNode : MonoBehaviour
{
    [Header("Node Settings")]
    public NodeType nodeType = NodeType.Enemy;
    public int nodeLevel = 1; // Schwierigkeit des Nodes

    [Header("State")]
    public bool isCompleted = false;
    public bool isAccessible = false;
    public bool isCurrentPosition = false;

    [Header("Connections")]
    public MapNode[] nextNodes; // Nodes die danach erreichbar sind

    [Header("Visual")]
    public SpriteRenderer nodeSprite;
    public Sprite enemySprite;
    public Sprite chestSprite;
    public Sprite mysterySprite;
    public Sprite bossSprite;
    public Sprite shopSprite;
    public Sprite restSprite;

    [Header("Colors")]
    public Color accessibleColor = Color.white;
    public Color completedColor = Color.gray;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color currentColor = Color.yellow;

    void Start()
    {
        if (nodeSprite == null)
        {
            nodeSprite = GetComponent<SpriteRenderer>();
        }

        SetSpriteByType();
        UpdateVisuals();
    }

    void SetSpriteByType()
    {
        if (nodeSprite == null) return;

        switch (nodeType)
        {
            case NodeType.Enemy:
                if (enemySprite != null) nodeSprite.sprite = enemySprite;
                break;
            case NodeType.Chest:
                if (chestSprite != null) nodeSprite.sprite = chestSprite;
                break;
            case NodeType.Mystery:
                if (mysterySprite != null) nodeSprite.sprite = mysterySprite;
                break;
            case NodeType.Boss:
                if (bossSprite != null) nodeSprite.sprite = bossSprite;
                break;
            case NodeType.Shop:
                if (shopSprite != null) nodeSprite.sprite = shopSprite;
                break;
            case NodeType.Rest:
                if (restSprite != null) nodeSprite.sprite = restSprite;
                break;
        }
    }

    public void SetAccessible(bool accessible)
    {
        Debug.Log($"?? SetAccessible({accessible}) called on {gameObject.name}");
        isAccessible = accessible;
        UpdateVisuals();
        Debug.Log($"   ? isAccessible is now: {isAccessible}");
    }

    public void SetCurrent(bool current)
    {
        isCurrentPosition = current;
        UpdateVisuals();
    }

    public void Complete()
    {
        isCompleted = true;
        isCurrentPosition = false;
        UpdateVisuals();

        Debug.Log($"? Node {gameObject.name} ({nodeType}) completed!");
        Debug.Log($"   Unlocking {nextNodes.Length} next nodes...");

        // Unlock next nodes
        foreach (MapNode nextNode in nextNodes)
        {
            if (nextNode != null)
            {
                Debug.Log($"   ? Unlocking:  {nextNode.gameObject.name}");
                nextNode.SetAccessible(true);
            }
            else
            {
                Debug.LogWarning($"   ? Next node is NULL!");
            }
        }
    }

    void UpdateVisuals()
    {
        if (nodeSprite == null) return;

        if (isCurrentPosition)
        {
            nodeSprite.color = currentColor;
        }
        else if (isCompleted)
        {
            nodeSprite.color = completedColor;
        }
        else if (isAccessible)
        {
            nodeSprite.color = accessibleColor;
        }
        else
        {
            nodeSprite.color = lockedColor;
        }
    }

    /// <summary>
    /// Called when player clicks on this node
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log("========================================");
        Debug.Log("????????? ONMOUSEDOWN TRIGGERED! !!  ?????????");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log("========================================");

        Debug.Log($"??? NODE CLICKED:  {gameObject.name} ({nodeType})");
        Debug.Log($"   - isAccessible: {isAccessible}");
        Debug.Log($"   - isCompleted: {isCompleted}");

        if (!isAccessible || isCompleted)
        {
            Debug.LogWarning($"? Node {gameObject.name} is not accessible or already completed!");
            return;
        }

        Debug.Log($"? Node {gameObject.name} is accessible!  Calling MapManager...");

        if (MapManager.instance != null)
        {
            Debug.Log("? MapManager instance found, calling OnNodeClicked()");
            MapManager.instance.OnNodeClicked(this);
        }
        else
        {
            Debug.LogError("? MapManager. instance is NULL!");
        }
    } 

    /// <summary>
    /// Draw connections in editor
    /// </summary>
    void OnDrawGizmos()
    {
        if (nextNodes == null) return;

        Gizmos.color = Color.cyan;
        foreach (MapNode nextNode in nextNodes)
        {
            if (nextNode != null)
            {
                Gizmos.DrawLine(transform.position, nextNode.transform.position);
            }
        }
    }
}