using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Einstellungen")]
    public float speed = 3.5f;
    public bool isRunning = true;

    [Header("Audio (Schritte)")]
    public AudioClip stepSound;
    public float stepInterval = 0.4f;

    private Rigidbody2D rb;
    private Animator animator;
    private float stepTimer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (animator == null)
        {
            Debug.LogError("❌ Kein Animator am Player gefunden!");
        }
        else
        {
            Debug.Log("✅ Animator gefunden:  " + animator.runtimeAnimatorController.name);
        }

        StartRunning();
    }

    void Update()
    {
        if (isRunning)
        {
            // Bewegung nach rechts
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

            // Animator:  Lauf-Animation aktivieren
            if (animator != null)
            {
                animator.SetBool("isRunning", true);
            }

            // Schritte-Sound
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                if (audioSource != null && stepSound != null)
                {
                    audioSource.PlayOneShot(stepSound);
                }
                stepTimer = 0f;
            }
        }
        else
        {
            // Spieler steht
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // Animator: Idle-Animation
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
            }
        }
    }

    public void StartRunning()
    {
        isRunning = true;
        Debug.Log("🏃 Player läuft!");
    }

    public void StopRunning()
    {
        isRunning = false;
        Debug.Log("🛑 Player stoppt!");
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            Debug.Log("⚔️ Attack-Animation!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            StopRunning();

            CharacterStats enemyStats = other.GetComponent<CharacterStats>();
            if (enemyStats != null && GameManager.instance != null)
            {
                GameManager.instance.TriggerEncounter(enemyStats);
            }
        }

        if (other.CompareTag("Coin"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.AddGold(10);
                GameManager.instance.PlayCoinSound();
            }
            Destroy(other.gameObject);
        }
    }
}