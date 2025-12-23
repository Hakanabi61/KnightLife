using UnityEngine;

public class EnemyStats : CharacterStats
{
    // Diese Funktion wird automatisch beim Start aufgerufen
    void Start()
    {
        // FIX: Falls der Gegner manuell in die Szene gelegt wurde (und nicht vom Spawner kommt),
        // hat er noch keine XP-Werte. Das holen wir hier nach!
        if (xpReward == 0)
        {
            SetupEnemy(level); // Sich selbst einstellen basierend auf dem Level im Inspector
        }

        // Leben auffüllen (aus dem Eltern-Script CharacterStats)
        currentHP = maxHP;
    }

    public void SetupEnemy(int targetLevel)
    {
        level = targetLevel;
        characterName = "Goblin Lvl " + level;

        // Werte skalieren
        maxHP = 30 + (level * 15);
        currentHP = maxHP;

        attack = 5 + (level * 2);
        defense = 1 + (level * 1);

        // XP WERT BERECHNEN
        xpReward = 20 + (level * 15);
    }

    // (Die Die-Methode brauchen wir hier nicht extra, er nimmt automatisch die vom CharacterStats)
}