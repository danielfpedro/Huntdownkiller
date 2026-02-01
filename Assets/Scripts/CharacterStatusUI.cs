using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusUI : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("The player gameobject to get health and stamina from")]
    public GameObject playerTarget;

    [Header("Health Bar")]
    [Tooltip("The Image component for the health bar fill")]
    public Image healthBarFill;
    [Tooltip("Color of the health bar")]
    public Color healthBarColor = Color.red;

    [Header("Stamina Bar")]
    [Tooltip("The Image component for the stamina bar fill")]
    public Image staminaBarFill;
    [Tooltip("Color of the stamina bar")]
    public Color staminaBarColor = Color.blue;

    // Cached references
    private PlayerMovement playerMovement;
    private HealthController healthController;

    void Start()
    {
        // Get the PlayerMovement component from the target
        if (playerTarget != null)
        {
            playerMovement = playerTarget.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("CharacterStatusUI: No PlayerMovement component found on playerTarget!");
            }

            healthController = playerTarget.GetComponent<HealthController>();
            if (healthController == null)
            {
                Debug.LogError("CharacterStatusUI: No HealthController component found on playerTarget!");
            }
        }
        else
        {
            Debug.LogError("CharacterStatusUI: playerTarget is not assigned!");
        }

        // Set bar colors
        if (healthBarFill != null)
        {
            healthBarFill.color = healthBarColor;
        }

        if (staminaBarFill != null)
        {
            staminaBarFill.color = staminaBarColor;
        }
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update stamina bar from PlayerMovement
        if (staminaBarFill != null && playerMovement != null)
        {
            float staminaPercentage = playerMovement.GetStaminaPercentage();
            staminaBarFill.fillAmount = staminaPercentage;
        }

        // Update health bar from HealthController
        if (healthBarFill != null && healthController != null)
        {
            float healthPercentage = (float)healthController.currentHealth / healthController.maxHealth;
            healthBarFill.fillAmount = healthPercentage;
        }
    }
}
