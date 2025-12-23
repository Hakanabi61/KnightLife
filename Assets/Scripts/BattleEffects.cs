using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Helper script for visual battle effects
/// Manages particle effects and visual feedback during combat
/// </summary>
public class BattleEffects : MonoBehaviour
{
    public static BattleEffects instance;

    [Header("Particle Effects")]
    public GameObject criticalHitParticles;
    public GameObject hitParticles;
    public GameObject blockParticles;

    [Header("Flash Effects")]
    public Image flashImage;
    public Color criticalFlashColor = new Color(1f, 0.5f, 0f, 0.5f); // Orange
    public Color normalHitColor = new Color(1f, 1f, 1f, 0.3f); // White
    public Color damageFlashColor = new Color(1f, 0f, 0f, 0.4f); // Red

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

    /// <summary>
    /// Spawn particle effect at a position
    /// </summary>
    /// <param name="particles">The particle prefab to spawn</param>
    /// <param name="position">World position to spawn at</param>
    public void SpawnParticles(GameObject particles, Vector3 position)
    {
        if (particles != null)
        {
            GameObject effect = Instantiate(particles, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    /// <summary>
    /// Flash the screen with a color
    /// </summary>
    /// <param name="color">Color to flash</param>
    /// <param name="duration">How long the flash lasts</param>
    public void FlashScreen(Color color, float duration = 0.2f)
    {
        if (flashImage != null)
        {
            StartCoroutine(DoFlash(color, duration));
        }
    }

    private IEnumerator DoFlash(Color color, float duration)
    {
        if (flashImage == null) yield break;

        flashImage.color = color;
        flashImage.enabled = true;

        float elapsed = 0f;
        Color startColor = color;
        Color endColor = new Color(color.r, color.g, color.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            flashImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        flashImage.enabled = false;
    }

    /// <summary>
    /// Play critical hit effects
    /// </summary>
    /// <param name="position">Position to spawn particles</param>
    public void CriticalHitEffect(Vector3 position)
    {
        FlashScreen(criticalFlashColor, 0.25f);
        SpawnParticles(criticalHitParticles, position);

        // Trigger camera shake if available
        if (CameraShake.instance != null)
        {
            CameraShake.instance.CriticalHitShake();
        }

        Debug.Log("✨ Critical hit visual effect played!");
    }

    /// <summary>
    /// Play normal hit effects
    /// </summary>
    /// <param name="position">Position to spawn particles</param>
    public void NormalHitEffect(Vector3 position)
    {
        FlashScreen(normalHitColor, 0.15f);
        SpawnParticles(hitParticles, position);
    }

    /// <summary>
    /// Play damage taken effects
    /// </summary>
    public void DamageTakenEffect()
    {
        FlashScreen(damageFlashColor, 0.2f);

        // Trigger camera shake if available
        if (CameraShake.instance != null)
        {
            CameraShake.instance.PlayerDamageShake();
        }

        Debug.Log("💔 Damage taken visual effect played!");
    }
}
