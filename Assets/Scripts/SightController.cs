using UnityEngine;
using UnityEngine.Events;

public class SightController : MonoBehaviour
{
    [Header("Sight Settings")]
    public float viewDistance = 5f; // The distance of the sight cone
    public float viewAngle = 90f; // The angle of the sight cone in degrees
    public int resolution = 10; // Number of rays in the cone
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    public Transform sightOrigin; // The point from which the sight rays originate
    public PlayerMovement playerMovement; // Reference to the player's movement to get facing direction

    [Header("Events")]
    public UnityEvent onTargetEnter;
    public UnityEvent onTargetStay;
    public UnityEvent onTargetExit;

    private bool wasTargetDetected = false;

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

            // Cast the ray for detection (target layer and obstacle layer)
            RaycastHit2D hit = Physics2D.Raycast(sightOrigin.position, rayDir, viewDistance, targetLayer | obstacleLayer);

            // Cast another ray for debug drawing (any collision)
            RaycastHit2D debugHit = Physics2D.Raycast(sightOrigin.position, rayDir, viewDistance);

            bool isTarget = false;
            
            if (hit.collider != null)
            {
                int hitLayerMask = 1 << hit.collider.gameObject.layer;
                bool isObstacle = (hitLayerMask & obstacleLayer) != 0;
                bool isTargetLayer = (hitLayerMask & targetLayer) != 0;

                // If it's strictly an obstacle, it's not a target. 
                // If it's in both layers, treat as obstacle (blocking).
                if (isObstacle)
                {
                    isTarget = false;
                }
                else if (isTargetLayer)
                {
                    isTarget = true;
                }
                
                // Specific check for Player reference if assigned
                if (playerMovement != null && hit.transform == playerMovement.transform)
                {
                    isTarget = true;
                }
            }

            // Determine draw distance and color
            float drawDistance = hit.collider != null ? hit.distance : (debugHit.collider != null ? debugHit.distance : viewDistance);
            Color rayColor = isTarget ? Color.red : (debugHit.collider != null ? Color.yellow : Color.green);

            // Debug draw the ray
            Debug.DrawRay(sightOrigin.position, rayDir * drawDistance, rayColor);

            if (isTarget)
            {
                targetDetected = true;
            }
        }

        if (targetDetected)
        {
            if (!wasTargetDetected)
            {
                onTargetEnter.Invoke();
            }
            else
            {
                onTargetStay.Invoke();
            }
        }
        else
        {
            if (wasTargetDetected)
            {
                onTargetExit.Invoke();
            }
        }

        wasTargetDetected = targetDetected;
    }
}
