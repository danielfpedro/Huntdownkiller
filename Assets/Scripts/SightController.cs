using UnityEngine;
using UnityEngine.Events;

public class SightController : MonoBehaviour
{
    [Header("Sight Settings")]
    public float viewDistance = 5f; // The distance of the sight cone
    public float viewAngle = 90f; // The angle of the sight cone in degrees
    public int resolution = 10; // Number of rays in the cone
    public LayerMask targetLayer;
    public Transform sightOrigin; // The point from which the sight rays originate
    public PlayerMovement playerMovement; // Reference to the player's movement to get facing direction

    [Header("Events")]
    public UnityEvent onTargetDetected;

    void Update()
    {
        if (sightOrigin != null)
        {
            DetectTargets();
        }
    }

    void DetectTargets()
    {
        bool targetDetected = false;

        // Calculate the starting angle (leftmost) of the cone
        float halfAngle = viewAngle / 2f;
        float step = viewAngle / (resolution - 1);
        
        // Because we use Rotation in PlayerMovement now, we can simply rely on 
        // sightOrigin.right to point in the correct forward direction.
        Vector3 forwardDir = sightOrigin.right;

        for (int i = 0; i < resolution; i++)
        {
            float angle = -halfAngle + (step * i);

            // Rotate the forward vector by the angle around the Z axis
            Vector3 rayDir = Quaternion.Euler(0, 0, angle) * forwardDir;

            // Cast the ray for detection (only target layer)
            RaycastHit2D hit = Physics2D.Raycast(sightOrigin.position, rayDir, viewDistance, targetLayer);

            // Cast another ray for debug drawing (any collision)
            RaycastHit2D debugHit = Physics2D.Raycast(sightOrigin.position, rayDir, viewDistance);

            // Determine draw distance and color
            float drawDistance = debugHit.collider != null ? debugHit.distance : viewDistance;
            Color rayColor = hit.collider != null ? Color.red : (debugHit.collider != null ? Color.yellow : Color.green);

            // Debug draw the ray
            Debug.DrawRay(sightOrigin.position, rayDir * drawDistance, rayColor);

            if (hit.collider != null)
            {
                targetDetected = true;
            }
        }

        if (targetDetected)
        {
            onTargetDetected.Invoke();
        }
    }
}
