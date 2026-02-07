using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onIdleStart;
    public UnityEvent onCrouchStart;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float crouchSpeed = 2f;
    public float jumpForce = 10f;
    public float gravity = 20f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 20f; // Stamina per second
    public float dashStaminaCost = 25f;
    public float staminaRegenDelay = 0.5f; // Delay before regen starts after spending

    [Header("Ground Detection")]
    public float hoverHeight = 1.0f;     // The desired height above ground
    public float rayLength = 1.5f;       // How far down to check for ground
    public LayerMask groundLayer;

    [Header("State")]
    public bool isCrouching = false;
    public bool isDashing = false;

    [Header("Colliders")]
    public Collider2D upperCollider;
    public Collider2D baseCollider;
    public SpriteRenderer SpriteRendererUpper;
    public SpriteRenderer SpriteRendererLower;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Weapon Settings")]
    [Tooltip("Container object for holding weapons, moved during crouch")]
    public GameObject weaponsContainer;
    [Tooltip("Local position offset applied to weapons when crouching")]
    public Vector2 weaponCrouchOffset = new Vector2(0f, -0.2f);
    [Tooltip("Smoothing time for weapon position changes")]
    public float weaponSmoothing = 0.1f;
    private Vector3 initialWeaponLocalPos;
    private Vector3 targetWeaponLocalPos;
    private Vector3 weaponVelocity;

    // Dash State
    private float dashTimer;
    private float lastDashTime = -100f;
    public float facingDirection = 1f;
    private bool hasAirDashed = false;

    // Stamina State
    private float currentStamina;
    private float lastStaminaSpendTime = -100f;
    HealthController healthController;
    bool isDead = false;

    void Start()
    {
        // Initialize facing direction.
        // If we are rotated 180 on Y, we are facing left (-1)
        if (Mathf.Abs(transform.rotation.eulerAngles.y - 180f) < 1f)
            facingDirection = -1f;
        else
            facingDirection = 1f;

        // Initialize weapon container position
        if (weaponsContainer != null)
        {
            initialWeaponLocalPos = weaponsContainer.transform.localPosition;
        }

        // Initialize stamina
        currentStamina = maxStamina;

        healthController = GetComponent<HealthController>();
        healthController.onDeath.AddListener(() =>
        {
            isDead = true;
            upperCollider.enabled = false;
            baseCollider.enabled = false;

            SpriteRendererUpper.enabled = false;
            SpriteRendererLower.enabled = false;

            Destroy(GetComponent<HealthController>());
            Destroy(GetComponent<Animator>());

            // Destroy all children except those containing "Blood"
            foreach (Transform child in transform)
            {
                if (!child.name.Contains("Blood"))
                {
                    Destroy(child.gameObject);
                }
            }

            Destroy(gameObject, 3f);
        });
    }

    void Update()
    {
        if (isDead) return;
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

        HandleStaminaRegen();
        HandleColliders();
    }

    void LateUpdate()
    {
        HandleWeaponPosition();
    }

    private void HandleWeaponPosition()
    {
        if (weaponsContainer == null) return;

        targetWeaponLocalPos = isCrouching ? initialWeaponLocalPos + (Vector3)weaponCrouchOffset : initialWeaponLocalPos;

        weaponsContainer.transform.localPosition = Vector3.SmoothDamp(
            weaponsContainer.transform.localPosition,
            targetWeaponLocalPos,
            ref weaponVelocity,
            weaponSmoothing
        );
    }

    private void HandleColliders()
    {
        upperCollider.enabled = true;
        baseCollider.enabled = true;
        if (isDashing)
        {
            upperCollider.enabled = false;
            baseCollider.enabled = false;
        }
        else if (isCrouching)
        {
            upperCollider.enabled = false;
        }
    }

    private void HandleStaminaRegen()
    {
        // Only regenerate if enough time has passed since last spend
        if (Time.time >= lastStaminaSpendTime + staminaRegenDelay)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
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

            // Rotate the character to face direction instead of flipping scale
            // This prevents negative scale issues with child objects (like sight cones)
            if (facingDirection > 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else
                transform.rotation = Quaternion.Euler(0, 180, 0);
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
        // Must be moving to dash
        if (Mathf.Abs(velocity.x) < 0.01f) return;

        // Cooldown check
        if (Time.time < lastDashTime + dashCooldown) return;

        // Stamina check
        if (currentStamina < dashStaminaCost) return;

        // Air dash check
        if (!isGrounded && hasAirDashed) return;

        StartDash();
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;

        // Consume stamina
        currentStamina -= dashStaminaCost;
        lastStaminaSpendTime = Time.time;

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
        // Move in World space to match valid movement logic
        transform.Translate(velocity * Time.deltaTime, Space.World);

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
        if (isCrouching == crouch) return;

        isCrouching = crouch;

        if (isCrouching)
        {
            onCrouchStart?.Invoke();
        }
        else
        {
            onIdleStart?.Invoke();
        }
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 0.2f, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(transform.position + Vector3.up * 0.2f, Vector2.down * rayLength, Color.red);

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
        // Move in World space so that rotation doesn't invert controls
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    // Public stamina access methods
    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }

    public bool CanDash()
    {
        return currentStamina >= dashStaminaCost &&
               Time.time >= lastDashTime + dashCooldown &&
               (!(!isGrounded && hasAirDashed));
    }
}
