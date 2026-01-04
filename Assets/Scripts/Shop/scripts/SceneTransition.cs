using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    private static bool returningToGame = false;

    // ============================================
    // MARKETPLACE VON SAMPLESCENE
    // ============================================

    public void GoToMarketplace()
    {
        Debug.Log("🏪 Gehe zum Marktplatz...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                PlayerPrefs.SetInt("Gold", stats.gold);
                PlayerPrefs.SetInt("Level", stats.level);
                PlayerPrefs.SetInt("CurrentHP", stats.currentHP);
                PlayerPrefs.SetInt("MaxHP", stats.maxHP);
                PlayerPrefs.SetInt("Attack", stats.attack);
                PlayerPrefs.SetInt("Defense", stats.defense);
                PlayerPrefs.SetInt("CurrentXP", stats.currentXP);
                PlayerPrefs.SetInt("MaxXP", stats.xpToNextLevel);

                // Merke dass wir von SampleScene kommen
                PlayerPrefs.SetString("ReturnScene", "SampleScene");

                PlayerPrefs.Save();

                Debug.Log("💾 Stats gespeichert:  Gold=" + stats.gold);
            }

            // Physik zurücksetzen
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        SceneManager.LoadScene("Marketplace");
    }

    public void ReturnToGame()
    {
        Debug.Log("🎮 Zurück zum Spiel...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                PlayerPrefs.SetInt("Gold", stats.gold);
                PlayerPrefs.SetInt("Attack", stats.attack);
                PlayerPrefs.SetInt("Defense", stats.defense);
                PlayerPrefs.SetInt("MaxHP", stats.maxHP);
                PlayerPrefs.SetInt("CurrentHP", stats.currentHP);
                PlayerPrefs.Save();

                Debug.Log("💾 Stats gespeichert vor Rückkehr");
            }

            // Physik zurücksetzen
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        returningToGame = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("SampleScene");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (returningToGame && scene.name == "SampleScene")
        {
            Debug.Log("🎮 Szene geladen - fixe Player Position");

            StartCoroutine(FixPlayerAfterLoad());

            returningToGame = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    IEnumerator FixPlayerAfterLoad()
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("🔧 Fixe Player Position & Physik");

            // Position zurücksetzen
            player.transform.position = new Vector3(-6f, -2.6f, 0f);

            // Physik komplett zurücksetzen
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = 1f;
            }

            // Controller aktivieren
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = true;
            }

            // Animator aktivieren
            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = true;
            }

            Debug.Log("✅ Player wieder aktiv an Position:  " + player.transform.position);
        }
    }

    // ============================================
    // MARKETPLACE VON DUNGEON
    // ============================================

    public void GoToMarketplaceFromDungeon()
    {
        Debug.Log("🏪 Gehe zum Marktplatz (von Dungeon)...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Speichere Stats
                PlayerPrefs.SetInt("Gold", stats.gold);
                PlayerPrefs.SetInt("Level", stats.level);
                PlayerPrefs.SetInt("CurrentHP", stats.currentHP);
                PlayerPrefs.SetInt("MaxHP", stats.maxHP);
                PlayerPrefs.SetInt("Attack", stats.attack);
                PlayerPrefs.SetInt("Defense", stats.defense);
                PlayerPrefs.SetInt("CurrentXP", stats.currentXP);
                PlayerPrefs.SetInt("MaxXP", stats.xpToNextLevel);

                // WICHTIG: Merke dass wir aus Dungeon kommen! 
                PlayerPrefs.SetString("ReturnScene", "DungeonScene");

                PlayerPrefs.Save();

                Debug.Log($"💾 Stats gespeichert: Gold={stats.gold}, HP={stats.currentHP}");
            }

            // Physik zurücksetzen
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        SceneManager.LoadScene("Marketplace");
    }

    public void ReturnToDungeon()
    {
        Debug.Log("🎮 Zurück zum Dungeon...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Speichere Stats vor Rückkehr
                PlayerPrefs.SetInt("Gold", stats.gold);
                PlayerPrefs.SetInt("Attack", stats.attack);
                PlayerPrefs.SetInt("Defense", stats.defense);
                PlayerPrefs.SetInt("MaxHP", stats.maxHP);
                PlayerPrefs.SetInt("CurrentHP", stats.currentHP);
                PlayerPrefs.Save();

                Debug.Log("💾 Stats gespeichert vor Rückkehr zu Dungeon");
            }

            // Physik zurücksetzen
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        // Lade DungeonScene
        SceneManager.LoadScene("DungeonScene");
    }
}