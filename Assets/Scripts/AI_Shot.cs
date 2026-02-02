using UnityEngine;

public class AI_Shot : MonoBehaviour
{
    [Header("AI Shooting Settings")]
    public float shotRate = 1f; // The rate at which the AI attempts to shoot
    public float startDelay = 0f; // Delay before the first shot

    private float nextShotTime = 0f;
    private WeaponsManager weaponsManager;

    void Start()
    {
        weaponsManager = GetComponentInParent<WeaponsManager>();
        nextShotTime = Time.time + startDelay;
    }

    void Update()
    {
        // Simple logic: Attempt to shoot periodically
        // You can enable/disable this script to start/stop shooting
        if (Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + shotRate;
        }
    }

    private void Shoot()
    {
        if (weaponsManager != null && weaponsManager.CurrentWeapon != null)
        {
            weaponsManager.CurrentWeapon.AttemptShoot();
        }
    }
}
