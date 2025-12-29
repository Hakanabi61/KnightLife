using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Stats")]
    public string characterName = "Character";
    public int level = 1;
    public int maxHP = 100;
    public int currentHP = 100;
    public int attack = 10;
    public int defense = 5;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Rewards")]
    public int xpReward = 25;
    public int gold = 10;

    void Start()
    {
        // Sicherstellen dass HP nicht über Max ist
        currentHP = Mathf.Min(currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"{characterName} takes {damage} damage! HP:  {currentHP}/{maxHP}");

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }

        // Tod? 
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        Debug.Log($"{characterName} heals {amount} HP! HP:  {currentHP}/{maxHP}");

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }
    }

    public void RestoreHP(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }
    }

    public void GainXP(int amount)
    {
        currentXP += amount;

        Debug.Log($"{characterName} gains {amount} XP!");

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }
    }

    public void LevelUp()
    {
        level++;
        maxHP += 10;
        currentHP = maxHP;
        attack += 2;
        defense += 1;

        Debug.Log($"🎉 {characterName} LEVEL UP!  Now Level {level}");
        Debug.Log($"   HP: {maxHP} | ATK: {attack} | DEF: {defense}");

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateUI();
        }
    }

    void Die()
    {
        Debug.Log($"💀 {characterName} died!");

        // Wenn es der Player ist
        if (this == GameManager.instance.playerStats)
        {
            // Game Over Logic
        }
    }
}