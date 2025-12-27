using UnityEngine;
using UnityEngine.UI;

public class HitBarVisualizer : MonoBehaviour
{
    public Slider attackBar;
    public Image fillImage;

    [Header("Zonen")]
    public Color weakColor = new Color(0.8f, 0.2f, 0.2f); // Rot
    public Color criticalColor = new Color(1f, 0.84f, 0f); // Gold
    public Color normalColor = new Color(0.2f, 0.8f, 0.2f); // Grün

    void Update()
    {
        if (attackBar != null && fillImage != null && attackBar.gameObject.activeSelf)
        {
            float value = attackBar.value;

            // Farbe basierend auf Wert
            if (value >= 40f && value <= 60f)
            {
                // Kritische Zone:  GOLD
                fillImage.color = criticalColor;
            }
            else if (value >= 25f && value <= 75f)
            {
                // Normale Zone: GRÜN
                fillImage.color = normalColor;
            }
            else
            {
                // Schwache Zone: ROT
                fillImage.color = weakColor;
            }
        }
    }
}