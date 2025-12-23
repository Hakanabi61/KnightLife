using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public GameObject cam;
    public float parallaxEffect; // 0 = bewegt sich mit Spieler, 1 = steht still

    [Header("Manuelle Einstellung")]
    public float length; // <--- HIER gibst du ein, wie breit deine 3 Bilder zusammen sind
    private float startpos;

    void Start()
    {
        startpos = transform.position.x;

        // Falls du vergessen hast, die Länge einzutragen, versuchen wir es automatisch
        if (length == 0)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) length = sprite.bounds.size.x;
        }
    }

    void FixedUpdate()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}