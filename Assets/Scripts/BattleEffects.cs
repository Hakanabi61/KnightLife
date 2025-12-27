using UnityEngine;
using System.Collections;

/// <summary>
/// Manages visual effects during battle (particles, screen flash, etc.)
/// </summary>
public class BattleEffects : MonoBehaviour
{
    public static BattleEffects instance;

    [Header("Particle Effects")]
    public GameObject criticalHitParticle;
    public GameObject normalHitParticle;
    public GameObject weakHitParticle;
    public float particleLifetime = 2f;

    [Header("Screen Flash")]
    public GameObject screenFlashPanel;
    public float flashDuration = 0.2f;
    public Color criticalFlashColor = new Color(1f, 0.84f, 0f, 0.5f); // Gold
    public Color normalFlashColor = new Color(1f, 1f, 1f, 0.3f); // White
    public Color weakFlashColor = new Color(0.5f, 0.5f, 0.5f, 0.2f); // Gray

    [Header("Enemy Effects")]
    public Color enemyDamageColor = Color.red;
    public float enemyDamageDuration = 0.15f;

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
    /// Spawn particle effect at position
    /// </summary>
    public void SpawnParticle(GameObject particlePrefab, Vector3 position)
    {
        if (particlePrefab != null)
        {
            GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity);
            Destroy(particle, particleLifetime);
        }
    }

    /// <summary>
    /// Critical hit effect (particles + flash)
    /// </summary>
    public void PlayCriticalHitEffect(Vector3 position)
    {
        SpawnParticle(criticalHitParticle, position);
        StartCoroutine(ScreenFlash(criticalFlashColor, flashDuration));
    }

    /// <summary>
    /// Normal hit effect
    /// </summary>
    public void PlayNormalHitEffect(Vector3 position)
    {
        SpawnParticle(normalHitParticle, position);
        StartCoroutine(ScreenFlash(normalFlashColor, flashDuration * 0.5f));
    }

    /// <summary>
    /// Weak hit effect
    /// </summary>
    public void PlayWeakHitEffect(Vector3 position)
    {
        SpawnParticle(weakHitParticle, position);
        // No screen flash for weak hits
    }

    /// <summary>
    /// Screen flash effect
    /// </summary>
    IEnumerator ScreenFlash(Color flashColor, float duration)
    {
        if (screenFlashPanel != null)
        {
            UnityEngine.UI.Image flashImage = screenFlashPanel.GetComponent<UnityEngine.UI.Image>();
            if (flashImage != null)
            {
                screenFlashPanel.SetActive(true);
                flashImage.color = flashColor;

                float elapsed = 0f;
                Color startColor = flashColor;
                Color endColor = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    flashImage.color = Color.Lerp(startColor, endColor, t);
                    yield return null;
                }

                screenFlashPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Enemy damage flash effect
    /// </summary>
    public void PlayEnemyDamageEffect(GameObject enemy)
    {
        if (enemy != null)
        {
            StartCoroutine(EnemyFlash(enemy));
        }
    }

    IEnumerator EnemyFlash(GameObject enemy)
    {
        SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = enemyDamageColor;

            yield return new WaitForSeconds(enemyDamageDuration);

            spriteRenderer.color = originalColor;
        }
    }
}