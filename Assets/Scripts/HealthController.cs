using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    [Header("Health Bar")]
    [Tooltip("Prefab for the health bar UI")]
    public GameObject healthBarPrefab;
    [Tooltip("Time in seconds to display the health bar after taking damage")]
    public float healthBarDisplayTime = 3f;
    [Tooltip("Offset for positioning the health bar above the character")]
    public Vector3 worldOffset = Vector3.up * 2f;

    private GameObject healthBarInstance;
    private RectTransform healthBarRect;
    private Coroutine healthBarCoroutine;
    private bool healthBarIsVisible = false;

    #region Events
    [Header("Events")]
    [Tooltip("Event triggered when taking damage (passes damage amount)")]
    public UnityEvent<int> onHurt;
    [Tooltip("Event triggered when health is healed (passes heal amount)")]
    public UnityEvent<int> onHeal;
    [Tooltip("Event triggered on death")]
    public UnityEvent onDeath;
    [Tooltip("Event triggered when health changes (passes new health value)")]
    public UnityEvent<int> onHealthChanged;
    #endregion

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

        // Instantiate health bar
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null && healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, canvasGO.transform);
            healthBarRect = healthBarInstance.GetComponent<RectTransform>();
            CharacterHealthBar ui = healthBarInstance.GetComponent<CharacterHealthBar>();
            ui.SetTarget(transform);
            // Start hidden
            healthBarInstance.SetActive(false);
            healthBarIsVisible = false;
        }
    }

    private void OnDestroy()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
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

        // Position the health bar above the character
        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarIsVisible && healthBarRect != null && Camera.main != null)
        {
            Vector3 targetPosition = transform.position + worldOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);
            healthBarRect.position = screenPos;
        }
    }

    public void TakeDamage(int damage)
    {
        HitIndicatorManager.Instance.ShowIndicator(transform.position, damage.ToString());
        TakeDamage(damage, Vector2.zero);
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (damage <= 0) return;

        // Show health bar
        if (healthBarInstance != null)
        {
            if (healthBarCoroutine != null)
            {
                StopCoroutine(healthBarCoroutine);
            }
            healthBarInstance.SetActive(true);
            healthBarIsVisible = true;
            healthBarCoroutine = StartCoroutine(HideHealthBarAfter(healthBarDisplayTime));
        }

        Debug.Log("About to call show indicator: " + damage);
        HitIndicatorManager.Instance.ShowIndicator(transform.position * new Vector2(1, 2), damage.ToString());

        // Play blood particles
        if (bloodParticleSystem != null)
        {
            if (hitDirection != Vector2.zero)
            {
                // Rotate 90 degrees if hit from right (blood sprays up), -90 if from left (down)
                float rotationAngle = (hitDirection.x < 0) ? -90 : 90f;
                bloodParticleSystem.transform.rotation = Quaternion.Euler(0, 0, rotationAngle); // Offset X position based on hit direction
                float offset = (hitDirection.x < 0) ? bloodOffsetX : -bloodOffsetX;
                bloodParticleSystem.transform.localPosition = new Vector3(offset - 0.7f, 1.4f, 0);
            }
            var emission = bloodParticleSystem.emission;
            emission.rateOverTime = bloodEmissionRate;
            bloodParticleSystem.Emit(6);
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
        EmojiManager.Instance.ShowEmoji(transform, "<sprite=2>", 1f);
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

    private System.Collections.IEnumerator HideHealthBarAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (healthBarInstance != null)
        {
            healthBarIsVisible = false;
            healthBarInstance.SetActive(false);
        }
    }
}

