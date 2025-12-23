using UnityEngine;
using System.Collections;

/// <summary>
/// Helper script for camera shake effects
/// Attach to the main camera for screen shake during critical hits
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPosition;
    private bool isShaking = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate GameObject to maintain singleton
        }
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Trigger a camera shake effect
    /// </summary>
    /// <param name="duration">How long the shake lasts</param>
    /// <param name="magnitude">Intensity of the shake</param>
    public void Shake(float duration = 0.3f, float magnitude = 0.2f)
    {
        if (!isShaking)
        {
            StartCoroutine(DoShake(duration, magnitude));
        }
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Quick shake for critical hits
    /// </summary>
    public void CriticalHitShake()
    {
        Shake(0.25f, 0.15f);
    }

    /// <summary>
    /// Stronger shake for player damage
    /// </summary>
    public void PlayerDamageShake()
    {
        Shake(0.3f, 0.25f);
    }
}
