using UnityEngine;

public enum EncounterType
{
    Start,      // Startpunkt
    Enemy,      // Normaler Gegner
    Elite,      // Starker Gegner
    Chest,      // Schatztruhe
    Shop,       // Händler
    Campfire,   // Heilen
    Mystery,    // Zufälliges Event
    Choice,     // Kreuzung (Player wählt Path)
    Boss        // Endgegner
}

/// <summary>
/// Represents a single encounter point in the dungeon
/// </summary>
public class EncounterPoint : MonoBehaviour
{
    [Header("Encounter Settings")]
    public EncounterType encounterType = EncounterType.Enemy;
    public int encounterLevel = 1;

    [Header("Visual")]
    public SpriteRenderer encounterSprite;
    public GameObject visualIndicator; // Optionales Visual (z.B. "!" Symbol)

    [Header("Choice Point Settings")]
    [Tooltip("Nur für EncounterType.Choice - Welche Paths sind möglich?")]
    public EncounterPoint[] choiceOptions; // Für Branching Paths

    [Header("Next Point")]
    [Tooltip("Für lineare Paths - nächster Punkt")]
    public EncounterPoint nextPoint;

    [Header("State")]
    public bool isCompleted = false;
    public bool isActive = false;

    void Start()
    {
        if (encounterSprite == null)
        {
            encounterSprite = GetComponent<SpriteRenderer>();
        }

        // Visual Indicator verstecken bis aktiv
        if (visualIndicator != null)
        {
            visualIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Aktiviert diesen Encounter Point
    /// </summary>
    public void Activate()
    {
        isActive = true;

        if (visualIndicator != null)
        {
            visualIndicator.SetActive(true);
        }

        Debug.Log($"📍 Encounter activated:  {gameObject.name} ({encounterType})");
    }

    /// <summary>
    /// Markiert Encounter als abgeschlossen
    /// </summary>
    public void Complete()
    {
        isCompleted = true;
        isActive = false;

        if (visualIndicator != null)
        {
            visualIndicator.SetActive(false);
        }

        // Visual Feedback (Grau machen)
        if (encounterSprite != null)
        {
            encounterSprite.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }

        Debug.Log($"✅ Encounter completed: {gameObject.name}");
    }

    /// <summary>
    /// Editor Gizmos - Zeigt Verbindungen
    /// </summary>
    void OnDrawGizmos()
    {
        // Zeige Verbindung zum nächsten Point
        if (nextPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, nextPoint.transform.position);
        }

        // Zeige Choice Options
        if (choiceOptions != null && choiceOptions.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (EncounterPoint choice in choiceOptions)
            {
                if (choice != null)
                {
                    Gizmos.DrawLine(transform.position, choice.transform.position);
                }
            }
        }

        // Zeige Encounter Type als Text (nur im Editor sichtbar)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, encounterType.ToString());
#endif
    }
}