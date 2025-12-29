using UnityEngine;

public enum EncounterType
{
    Start,
    Enemy,
    Elite,
    Chest,
    Shop,
    Campfire,
    Mystery,
    Choice,
    Boss
}

public class EncounterPoint : MonoBehaviour
{
    [Header("Encounter Settings")]
    public EncounterType encounterType = EncounterType.Enemy;

    [Header("Choice Options (nur für Choice Type)")]
    [Tooltip("Welche Points sind bei Choice verfügbar?")]
    public EncounterPoint choiceOption1; // z.B.  Chest
    public EncounterPoint choiceOption2; // z.B. Shop

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    void OnValidate()
    {
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            switch (encounterType)
            {
                case EncounterType.Start:
                    spriteRenderer.color = Color.green;
                    break;
                case EncounterType.Enemy:
                case EncounterType.Elite:
                    spriteRenderer.color = Color.red;
                    break;
                case EncounterType.Chest:
                    spriteRenderer.color = Color.yellow;
                    break;
                case EncounterType.Shop:
                    spriteRenderer.color = Color.cyan;
                    break;
                case EncounterType.Campfire:
                    spriteRenderer.color = new Color(1f, 0.5f, 0f);
                    break;
                case EncounterType.Choice:
                case EncounterType.Mystery:
                    spriteRenderer.color = Color.magenta;
                    break;
                case EncounterType.Boss:
                    spriteRenderer.color = new Color(0.5f, 0f, 0.5f);
                    break;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Zeige Choice Verbindungen im Editor
        if (encounterType == EncounterType.Choice)
        {
            Gizmos.color = Color.yellow;
            if (choiceOption1 != null)
            {
                Gizmos.DrawLine(transform.position, choiceOption1.transform.position);
            }
            if (choiceOption2 != null)
            {
                Gizmos.DrawLine(transform.position, choiceOption2.transform.position);
            }
        }
    }
}