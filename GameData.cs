using UnityEngine;

/// <summary>
/// Persistent data that survives scene changes
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData instance;

    [Header("Player Stats")]
    public int playerLevel = 1;
    public int playerHP = 100;
    public int playerMaxHP = 100;
    public int playerGold = 0;
    public int playerXP = 0;
    public int playerAttack = 10;
    public int playerDefense = 5;

    [Header("Current Run")]
    public int currentMapLevel = 1; // Welches Level der Spieler gerade spielt
    public int nodesCompleted = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Überlebt Scene-Wechsel! 
            Debug.Log("✅ GameData initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Save data from CharacterStats (for transitioning from SampleScene)
    /// </summary>
    public void SaveFromCharacterStats(CharacterStats stats)
    {
        if (stats == null) return;

        playerLevel = stats.level;
        playerHP = stats.currentHP;
        playerMaxHP = stats.maxHP;
        playerGold = stats.gold;
        playerXP = stats.currentXP;
        playerAttack = stats.attack;
        playerDefense = stats.defense;

        Debug.Log($"💾 Saved player data:  Lvl {playerLevel}, HP {playerHP}/{playerMaxHP}, Gold {playerGold}");
    }

    /// <summary>
    /// Load data to CharacterStats (for transitioning back to SampleScene)
    /// </summary>
    public void LoadToCharacterStats(CharacterStats stats)
    {
        if (stats == null) return;

        stats.level = playerLevel;
        stats.currentHP = playerHP;
        stats.maxHP = playerMaxHP;
        stats.gold = playerGold;
        stats.currentXP = playerXP;
        stats.attack = playerAttack;
        stats.defense = playerDefense;

        Debug.Log($"📂 Loaded player data to CharacterStats");
    }

    /// <summary>
    /// Reset for new run
    /// </summary>
    public void ResetRun()
    {
        currentMapLevel = 1;
        nodesCompleted = 0;
    }
}