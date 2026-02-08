using UnityEngine;
using UnityEngine.UI;

public class CharacterHealthBar : MonoBehaviour
{
    [HideInInspector]
    public Transform target;
    public float fillSpeed = 5f;

    public Image healthImage;
    private float targetFill = 1f;
    HealthController healthController;
    // Update is called once per frame
    void Update()
    {
        if (target != null && healthController != null)
        {
            targetFill = (float)healthController.currentHealth / healthController.maxHealth;
            healthImage.fillAmount = Mathf.Lerp(healthImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        healthController = target.GetComponent<HealthController>();
    }
}
