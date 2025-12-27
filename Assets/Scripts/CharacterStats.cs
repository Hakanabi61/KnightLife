using UnityEngine;
using System.Collections;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName = "Character";
    public int level = 1;

    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int attack = 10;
    public int defense = 5;

    [Header("Player Stats")]
    public int gold = 0;
    public int currentXP = 0;
    public int maxXP = 100;

    [Header("Enemy Stats")]
    public int xpReward = 50;

    void Start()
    {
        currentHP = maxHP;

        if (CompareTag("Player") && GameManager.instance != null)
        {
            GameManager.instance.UpdatePlayerHPText(currentHP, maxHP);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        Animator animator = GetComponent<Animator>();
        if (animator != null && currentHP > 0)
        {
            animator.SetTrigger("Hit");
        }

        if (currentHP <= 0)
        {
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            Die();
        }

        if (CompareTag("Player") && GameManager.instance != null)
        {
            GameManager.instance.UpdatePlayerHPText(currentHP, maxHP);
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        if (CompareTag("Player") && GameManager.instance != null)
        {
            GameManager.instance.UpdatePlayerHPText(currentHP, maxHP);
        }
    }

    public void GainXP(int amount)
    {
        if (!CompareTag("Player")) return;

        currentXP += amount;

        while (currentXP >= maxXP)
        {
            LevelUp();
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateLevelUI(level, currentXP, maxXP);
        }
    }

    void LevelUp()
    {
        level++;
        currentXP -= maxXP;
        maxXP = (int)(maxXP * 1.5f);

        maxHP += 20;
        currentHP = maxHP;
        attack += 3;
        defense += 2;

        Debug.Log("🎉 LEVEL UP! Level " + level);

        if (GameManager.instance != null)
        {
            GameManager.instance.ShowLevelUpEffect();
            GameManager.instance.UpdatePlayerHPText(currentHP, maxHP);
            GameManager.instance.UpdateLevelUI(level, currentXP, maxXP);
        }
    }

    void Die()
    {
        Debug.Log(characterName + " ist gestorben!");

        if (!CompareTag("Player"))
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        // Warte 2 Sekunden (Death Animation)
        yield return new WaitForSeconds(2.0f);

        // Fade out über 1 Sekunde
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            float elapsed = 0f;
            float fadeDuration = 1.0f;
            Color originalColor = sprite.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}