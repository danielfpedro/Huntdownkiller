using UnityEngine;

public class PatrolController : MonoBehaviour
{
    public float patrolDistance = 5f; // Distance to patrol before turning
    private Vector3 startPosition;
    private int direction = 1; // 1 for right, -1 for left
    private float targetX;
    private PlayerMovement playerMovement;

    void Start()
    {
        startPosition = transform.position;
        targetX = startPosition.x + direction * patrolDistance;
        playerMovement = transform.parent.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerMovement != null)
        {
            // Use PlayerMovement to move
            playerMovement.Move(new Vector2(direction, 0));

            // Check if reached target
            if ((direction == 1 && transform.position.x >= targetX) ||
                (direction == -1 && transform.position.x <= targetX))
            {
                // Flip direction
                direction *= -1;
                targetX = startPosition.x + direction * patrolDistance;
            }
        }
    }
}
