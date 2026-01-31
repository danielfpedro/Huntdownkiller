using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float crouchSpeed = 2f;
    public float jumpForce = 10f;
    public float gravity = 20f;
    
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Ground Detection")]
    public float hoverHeight = 1.0f;     // The desired height above ground
    public float rayLength = 1.5f;       // How far down to check for ground
    public LayerMask groundLayer;
    
    [Header("State")]
    public bool isCrouching = false;
    public bool isDashing = false;

    private Vector3 velocity;
    private bool isGrounded;
    
    // Dash State
    private float dashTimer;
    private float lastDashTime = -100f;
    private float facingDirection = 1f;
    private bool hasAirDashed = false;

    void Start()
    {
        // Initialize facing direction based on initial scale
        facingDirection = Mathf.Sign(transform.localScale.x);
    }

    void Update()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            ApplyGravity();
            HandleGroundCheck();
            ApplyMovement();
        }
    }

    public void Move(Vector2 input)
    {
        // Don't change movement intent while dashing
        if (isDashing) return;

        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        velocity.x = input.x * currentSpeed;

        // Face direction
        if (input.x != 0)
        {
            facingDirection = Mathf.Sign(input.x);
            transform.localScale = new Vector3(facingDirection, 1, 1);
        }
    }

    public void Jump()
    {
        if (isDashing) return;

        if (isGrounded && !isCrouching)
        {
            velocity.y = jumpForce;
            isGrounded = false; // Detach from ground immediately
        }
    }

    public void AttemptDash()
    {
        // Cooldown check
        if (Time.time < lastDashTime + dashCooldown) return;
        
        // Air dash check
        if (!isGrounded && hasAirDashed) return;

        StartDash();
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;

        // If in air, mark that we used our air dash
        if (!isGrounded)
        {
            hasAirDashed = true;
        }

        // Set dash velocity (horizontal only)
        velocity.x = facingDirection * dashSpeed;
        velocity.y = 0; // Maintain height (classic air dash) or allow gravity? Classic usually suspends gravity.
    }

    void HandleDash()
    {
        // Move
        transform.Translate(velocity * Time.deltaTime);

        // Count down
        dashTimer -= Time.deltaTime;
        
        if (dashTimer <= 0)
        {
            isDashing = false;
            velocity.x = 0; // Stop momentum or keep it? Let's stop it for precise control
        }
    }

    public void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
        // Visual feedback could be added here (e.g. shrinking sprite)
    }

    void ApplyGravity()
    {
        // Apply gravity if not grounded
        // Note: Even if grounded, we might apply a small gravity to keep checking
        // but since we snap to position, we handle gravity manually in ground check
        
        velocity.y -= gravity * Time.deltaTime;
    }

    void HandleGroundCheck()
    {
        // Simple raycast down
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

        // Check if we hit ground and are falling or staying still (not jumping up)
        if (hit.collider != null && velocity.y <= 0)
        {
            float distanceToGround = hit.point.y; // World Y of ground
            float desiredY = distanceToGround + hoverHeight;

            // If we are close enough to the ground level (or below it due to movement)
            // Snap to the correct height
            if (transform.position.y <= desiredY + 0.1f) // Tolerance
            {
                Vector3 pos = transform.position;
                pos.y = desiredY;
                transform.position = pos;

                velocity.y = 0;
                isGrounded = true;
                hasAirDashed = false; // Reset air dash
                return;
            }
        }
        
        isGrounded = false;
    }

    void ApplyMovement()
    {
        transform.Translate(velocity * Time.deltaTime);
    }
}
