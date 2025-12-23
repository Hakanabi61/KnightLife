using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Referenzen")]
    public GameObject enemyPrefab;
    public GameObject goldPrefab;
    public Transform player;

    [Header("Spawn-Einstellungen")]
    public float spawnDistance = 10f;
    public float spawnAheadDistance = 15f;
    public float despawnBehindDistance = 30f;

    [Header("Höhen-Einstellungen (World Space)")]
    // Diese Werte sind absolut. Egal wo der Spawner liegt, 
    // das Objekt spawnt auf dieser Höhe im Raum.
    public float enemyY = 0.83f;
    public float goldY = 1.5f; // Habe ich mal auf 1.5 gesetzt (statt 15), damit man es erreicht!

    [Header("Schwierigkeit & Balancing")]
    public float baseGoldChance = 0.15f; // Reduziert von 0.3
    public float levelScalingDistance = 50f;
    public int maxLevel = 20;

    [Header("Abwechslung")]
    public float goldClusterChance = 0.05f; // Reduziert von 0.15
    public int goldClusterSize = 3;
    public float goldClusterSpacing = 2f;

    [Header("Pooling Einstellungen")]
    public int initialPoolSize = 20; // Wie viele Objekte sollen vorbereitet werden?

    // Interne Variablen
    private float lastSpawnPosition;
    private CharacterStats playerStats;

    // Die Pools (unsere "Lagerhallen" für Objekte)
    private List<GameObject> enemyPool = new List<GameObject>();
    private List<GameObject> goldPool = new List<GameObject>();

    void Start()
    {
        if (player != null)
        {
            lastSpawnPosition = player.position.x;
            playerStats = player.GetComponent<CharacterStats>();
        }
        else
        {
            Debug.LogError("LevelGenerator: Spieler-Referenz fehlt!");
        }

        // Pools initialisieren (Lager füllen)
        InitializePool(enemyPrefab, enemyPool, initialPoolSize);
        InitializePool(goldPrefab, goldPool, initialPoolSize);
    }

    void Update()
    {
        if (player == null) return;

        // Neuen Kram spawnen
        if (player.position.x > lastSpawnPosition + spawnDistance)
        {
            SpawnSomething();
        }

        // Alten Kram deaktivieren (statt löschen)
        DeactivateOldObjects();
    }

    // --- POOLING LOGIK ---

    void InitializePool(GameObject prefab, List<GameObject> pool, int amount)
    {
        if (prefab == null) return;

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false); // Erstmal ausschalten
            obj.transform.parent = this.transform; // Aufräumen: Unter den Generator hängen
            pool.Add(obj);
        }
    }

    GameObject GetObjectFromPool(List<GameObject> pool, GameObject prefab)
    {
        // 1. Suche nach einem inaktiven Objekt im Pool
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // 2. Falls alle benutzt sind, erstelle ein neues (Notfall-Lösung)
        if (prefab != null)
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            newObj.transform.parent = this.transform;
            pool.Add(newObj);
            return newObj;
        }

        return null;
    }

    void DeactivateOldObjects()
    {
        // Prüfe Gegner
        foreach (GameObject enemy in enemyPool)
        {
            if (enemy.activeInHierarchy && enemy.transform.position.x < player.position.x - despawnBehindDistance)
            {
                enemy.SetActive(false); // Zurück in den Schrank
            }
        }

        // Prüfe Gold
        foreach (GameObject gold in goldPool)
        {
            if (gold.activeInHierarchy && gold.transform.position.x < player.position.x - despawnBehindDistance)
            {
                gold.SetActive(false); // Zurück in den Schrank
            }
        }
    }

    // --- SPAWN LOGIK ---

    void SpawnSomething()
    {
        float goldChance = CalculateGoldChance();

        if (Random.value < goldClusterChance)
        {
            SpawnGoldCluster();
        }
        else if (Random.value < goldChance)
        {
            SpawnGold();
        }
        else
        {
            SpawnEnemy();
        }
    }

    float CalculateGoldChance()
    {
        float chance = baseGoldChance;

        if (playerStats != null)
        {
            float healthPercent = (float)playerStats.currentHP / playerStats.maxHP;

            // Pity-System: Wenn fast tot, gib etwas mehr Hoffnung (aber weniger als vorher)
            if (healthPercent < 0.3f) chance += 0.1f;
            else if (healthPercent < 0.5f) chance += 0.05f;
        }

        return Mathf.Clamp01(chance);
    }

    void SpawnEnemy()
    {
        GameObject enemy = GetObjectFromPool(enemyPool, enemyPrefab);
        if (enemy == null) return;

        // Position setzen
        Vector3 spawnPos = new Vector3(player.position.x + spawnAheadDistance, enemyY, 0);
        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true); // Einschalten!

        // Stats resetten
        int currentLevel = CalculateCurrentLevel();
        EnemyStats stats = enemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.SetupEnemy(currentLevel);
        }

        lastSpawnPosition = player.position.x;
    }

    void SpawnGold()
    {
        GameObject gold = GetObjectFromPool(goldPool, goldPrefab);
        if (gold == null) return;

        Vector3 spawnPos = new Vector3(player.position.x + spawnAheadDistance, goldY, 0);
        gold.transform.position = spawnPos;
        gold.transform.rotation = Quaternion.identity;
        gold.SetActive(true);

        lastSpawnPosition = player.position.x;
    }

    void SpawnGoldCluster()
    {
        if (goldPrefab == null) return;

        for (int i = 0; i < goldClusterSize; i++)
        {
            GameObject gold = GetObjectFromPool(goldPool, goldPrefab);
            if (gold != null)
            {
                float xOffset = i * goldClusterSpacing;
                float yVariation = Random.Range(-0.5f, 0.5f);

                Vector3 spawnPos = new Vector3(
                    player.position.x + spawnAheadDistance + xOffset,
                    goldY + yVariation,
                    0
                );

                gold.transform.position = spawnPos;
                gold.transform.rotation = Quaternion.identity;
                gold.SetActive(true);
            }
        }

        lastSpawnPosition = player.position.x + (goldClusterSize * goldClusterSpacing);
    }

    int CalculateCurrentLevel()
    {
        int level = 1 + (int)(player.position.x / levelScalingDistance);
        return Mathf.Min(level, maxLevel);
    }
}