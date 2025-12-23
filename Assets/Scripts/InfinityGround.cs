using UnityEngine;

public class InfiniteGround : MonoBehaviour
{
    private float length;       // Wie lang ist das Bild?
    public GameObject cam;      // Die Kamera, die wir verfolgen
    public float parallaxEffect = 1f; // 1 = Bewegt sich normal mit (optional für Hintergrund-Effekte)

    void Start()
    {
        // Wir holen uns die Kamera automatisch
        cam = Camera.main.gameObject;

        // Wir messen automatisch, wie breit das Bild ist
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Distanz, die die Kamera zurückgelegt hat
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        // Der "Teleport"-Moment:
        // Wenn die Kamera weit genug am Boden vorbei ist...
        if (cam.transform.position.x > transform.position.x + length)
        {
            // ...verschieben wir diesen Boden um 2x Länge nach vorne!
            transform.position = new Vector3(transform.position.x + length * 2, transform.position.y, transform.position.z);
        }
        else if (cam.transform.position.x < transform.position.x - length)
        {
            // (Falls man rückwärts laufen könnte, was wir nicht tun, aber sicher ist sicher)
            transform.position = new Vector3(transform.position.x - length * 2, transform.position.y, transform.position.z);
        }
    }
}