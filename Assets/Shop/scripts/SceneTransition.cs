using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    private static bool returningToGame = false; // Flag zum Merken

    public void GoToMarketplace()
    {
        Debug.Log("🏪 Gehe zum Marktplatz.. .");
        
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
                PlayerPrefs.SetInt("MaxXP", stats.maxXP);
                PlayerPrefs.Save();
                
                Debug.Log("💾 Stats gespeichert:  Gold=" + stats.gold);
            }

            // Physik zurücksetzen
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb. linearVelocity = Vector2.zero;
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
            CharacterStats stats = player. GetComponent<CharacterStats>();
            if (stats != null)
            {
                PlayerPrefs. SetInt("Gold", stats.gold);
                PlayerPrefs.SetInt("Attack", stats.attack);
                PlayerPrefs.SetInt("Defense", stats.defense);
                PlayerPrefs.SetInt("MaxHP", stats.maxHP);
                PlayerPrefs.SetInt("CurrentHP", stats.currentHP);
                PlayerPrefs.Save();
                
                Debug.Log("💾 Stats gespeichert vor Rückkehr");
            }

            // Physik zurücksetzen
            Rigidbody2D rb = player. GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        
        returningToGame = true; // Merken, dass wir zurückkehren
        SceneManager.sceneLoaded += OnSceneLoaded; // Event registrieren
        SceneManager.LoadScene("SampleScene"); // ⚠️ DEIN SZENENNAME!
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (returningToGame && scene.name == "SampleScene") // ⚠️ DEIN SZENENNAME! 
        {
            Debug.Log("🎮 Szene geladen - fixe Player Position");
            
            // Warte einen Frame, dann fixe den Player
            StartCoroutine(FixPlayerAfterLoad());
            
            returningToGame = false;
            SceneManager.sceneLoaded -= OnSceneLoaded; // Event entfernen
        }
    }

    System.Collections.IEnumerator FixPlayerAfterLoad()
    {
        yield return null; // Warte 1 Frame

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
                rb.gravityScale = 1f; // Normale Schwerkraft
            }

            // Controller aktivieren
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc. enabled = true;
                pc.isRunning = true;
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
}