using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    [Header("Status Werte")]
    public string characterName = "Held";
    public int level = 1;
    public int maxHP = 100;
    public int currentHP;

    public int attack = 10;
    public int defense = 5;
    public int luck = 5;

    [Header("Inventar")]
    public int gold = 0;

    [Header("Erfahrung (XP)")]
    public int currentXP = 0;
    public int maxXP = 100;
    public int xpReward = 0;

    [Header("UI")]
    public Slider hpSlider;

    void Start()
    {
        // Lade gespeicherte Werte (falls vorhanden)
        if (gameObject.CompareTag("Player"))
        {
            int savedGold = PlayerPrefs. GetInt("Gold", -1);
            if (savedGold >= 0) // Wurde schon mal gespeichert
            {
                gold = savedGold;
                level = PlayerPrefs.GetInt("Level", level);
                attack = PlayerPrefs.GetInt("Attack", attack);
                defense = PlayerPrefs. GetInt("Defense", defense);
                maxHP = PlayerPrefs.GetInt("MaxHP", maxHP);
                currentXP = PlayerPrefs.GetInt("CurrentXP", currentXP);
                maxXP = PlayerPrefs.GetInt("MaxXP", maxXP);
                
                // HP laden - aber nicht 0! 
                int savedHP = PlayerPrefs.GetInt("CurrentHP", -1);
                if (savedHP > 0)
                {
                    currentHP = savedHP;
                }
                else
                {
                    currentHP = maxHP; // Falls nichts gespeichert, voll heilen
                }
                
                Debug.Log("📥 Stats geladen: Gold=" + gold + ", HP=" + currentHP + "/" + maxHP + ", ATK=" + attack);
            }
            else
            {
                // Erste Mal spielen - normale Werte
                currentHP = maxHP;
            }
        }
        else
        {
            // Gegner - normale Werte
            currentHP = maxHP;
        }
        
        UpdateHealthUI();
    }

    public void TakeDamage(int dmg)
    {
        int finalDamage = Mathf.Max(1, dmg - defense);
        currentHP -= finalDamage;
        if (currentHP < 0) currentHP = 0;

        UpdateHealthUI();

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHealthUI();
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log(characterName + " erhält " + amount + " XP!");

        if (currentXP >= maxXP)
        {
            LevelUp();
        }

        if (gameObject.CompareTag("Player"))
        {
            GameManager.instance.UpdateLevelUI(level, currentXP, maxXP);
        }
    }

    void LevelUp()
    {
        currentXP -= maxXP;
        level++;

        maxXP = (int)(maxXP * 1.2f);

        maxHP += 20;
        currentHP = maxHP;
        attack += 3;
        defense += 1;

        Debug.Log("LEVEL UP! Jetzt Level " + level);

        UpdateHealthUI();
        if (gameObject.CompareTag("Player"))
        {
            GameManager.instance.UpdateLevelUI(level, currentXP, maxXP);
            GameManager.instance.ShowLevelUpEffect();
        }
    }

    void UpdateHealthUI()
    {
        if (hpSlider != null) 
        { 
            hpSlider. maxValue = maxHP; 
            hpSlider.value = currentHP; 
        }
        
        if (gameObject.CompareTag("Player") && GameManager.instance != null) 
        {
            GameManager.instance.UpdatePlayerHPText(currentHP, maxHP);
        }
    }

    public virtual void Die()
    {
        if (! gameObject.CompareTag("Player")) 
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        // Beim Beenden:  Gold speichern
        if (gameObject.CompareTag("Player"))
        {
            PlayerPrefs.SetInt("Gold", gold);
            PlayerPrefs.SetInt("Level", level);
            PlayerPrefs.SetInt("Attack", attack);
            PlayerPrefs.SetInt("Defense", defense);
            PlayerPrefs.SetInt("MaxHP", maxHP);
            PlayerPrefs.SetInt("CurrentHP", currentHP);
            PlayerPrefs.SetInt("CurrentXP", currentXP);
            PlayerPrefs.SetInt("MaxXP", maxXP);
            PlayerPrefs.Save();
            Debug.Log("💾 Beim Beenden gespeichert: " + gold + " Gold");
        }
    }
}