using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public int maxHealth = 100;
    [Tooltip("Current health points")]
    public int currentHealth;
    [Tooltip("Whether to flash the sprite when taking damage")]
    public bool flashOnHurt = true;
    [Tooltip("Flash duration when taking damage")]
    public float flashDuration = 0.1f;
    [Tooltip("Color to flash to when taking damage")]
    public Color flashColor = Color.clear;
    [Tooltip("Particle system for blood effect when hurt")]
    public ParticleSystem bloodParticleSystem;
    [Tooltip("Time to play blood particle system when hurt")]
    public float bloodPlayTime = 0.5f;
    [Tooltip("Emission rate for blood particles when playing")]
    public float bloodEmissionRate = 100f;
    [Tooltip("X offset for blood particles based on hit direction")]
    public float bloodOffsetX = 0.5f;

    [Header("Events")]
    [Tooltip("Event triggered when taking damage (passes damage amount)")]
    public UnityEvent<int> onHurt;
    [Tooltip("Event triggered when health is healed (passes heal amount)")]
    public UnityEvent<int> onHeal;
    [Tooltip("Event triggered on death")]
    public UnityEvent onDeath;
    [Tooltip("Event triggered when health changes (passes new health value)")]
    public UnityEvent<int> onHealthChanged;

    private float flashTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        onHealthChanged?.Invoke(currentHealth);

        if (bloodParticleSystem != null)
        {
            bloodParticleSystem.Stop();
        }
    }

    void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashOnHurt && spriteRenderer != null)
            {
                spriteRenderer.color = flashColor;
            }
            if (flashTimer <= 0)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("About to call show indicator: " + damage);
        HitIndicatorManager.Instance.ShowIndicator(transform.position, damage.ToString());
        TakeDamage(damage, Vector2.zero);
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (damage <= 0) return;

        Debug.Log("About to call show indicator: " + damage);
        HitIndicatorManager.Instance.ShowIndicator(transform.position, damage.ToString());

        // Play blood particles
        if (bloodParticleSystem != null)
        {
            if (hitDirection != Vector2.zero)
            {
                // Rotate 90 degrees if hit from right (blood sprays up), -90 if from left (down)
                float rotationAngle = (hitDirection.x < 0) ? -90 : 90f;
                bloodParticleSystem.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);                // Offset X position based on hit direction
                float offset = (hitDirection.x < 0) ? bloodOffsetX : -bloodOffsetX;
                bloodParticleSystem.transform.localPosition = new Vector3(offset, 0, 0);
            }
            var emission = bloodParticleSystem.emission;
            emission.rateOverTime = bloodEmissionRate;
            bloodParticleSystem.Emit(10);
            Invoke("StopBloodParticles", bloodPlayTime);
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        onHurt?.Invoke(damage);
        onHealthChanged?.Invoke(currentHealth);

        // Start or prolong flash
        if (flashOnHurt)
        {
            if (flashTimer > 0)
            {
                flashTimer += flashDuration; // Prolong the flash
            }
            else
            {
                flashTimer = flashDuration;
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        onHeal?.Invoke(amount);
        onHealthChanged?.Invoke(currentHealth);
    }

    public void Die()
    {
        onDeath?.Invoke();
    }

    public void Revive()
    {
        currentHealth = maxHealth;
        flashTimer = 0f;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        gameObject.SetActive(true);
        onHealthChanged?.Invoke(currentHealth);
    }

    private void StopBloodParticles()
    {
        if (bloodParticleSystem != null)
        {
            bloodParticleSystem.Stop();
        }
    }
}

