using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Der Spieler
    public Vector3 offset;   // Abstand (z.B. Z = -10)

    void LateUpdate() // LateUpdate ist besser für Kameras als Update
    {
        if (target != null)
        {
            // Wir folgen nur der X-Achse (Rechts), Höhe (Y) bleibt fix, damit es nicht wackelt
            transform.position = new Vector3(target.position.x + offset.x, transform.position.y, offset.z);
        }
    }
}