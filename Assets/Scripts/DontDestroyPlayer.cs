using UnityEngine;

public class DontDestroyPlayer : MonoBehaviour
{
    void Awake()
    {
        // Verhindert, dass der Player beim Szenenwechsel zerstört wird
        DontDestroyOnLoad(gameObject);
        
        // Verhindert Duplikate
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1)
        {
            Destroy(gameObject); // Ich bin ein Duplikat, weg damit!
        }
    }
}