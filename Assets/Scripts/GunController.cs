using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

// [RequireComponent(typeof(Animator))] // Animator replaced by custom Animation handling in GunAnimatorController
[RequireComponent(typeof(SpriteRenderer))]
public class GunController : MonoBehaviour
{
    public enum FireMode
    {
        Manual,
        Automatic
    }

    [Header("Gun Settings")]
    [Tooltip("The position where the bullet is spawned")]
    public Transform firePoint;
    [Tooltip("The Bullet Prefab to shoot")]
    public GameObject bulletPrefab;
    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.2f;
    [Tooltip("Minimum damage dealt per bullet")]
    public int minDamage = 1;
    [Tooltip("Maximum damage dealt per bullet")]
    public int maxDamage = 3;
    [Tooltip("Y-axis offset for shot randomness")]
    public float yOffset = 0f;
    [Tooltip("Particle system for shell ejection")]
    public ParticleSystem shellParticleSystem;
    [Tooltip("Particle system for old magazine ejection")]
    public ParticleSystem oldMagazineVFX;
    [Tooltip("Delay before emitting the old magazine during reload")]
    public float oldMagazineDelay = 0.5f;
    [Tooltip("Fire mode: Manual for single shots, Automatic for continuous fire")]
    public FireMode fireMode = FireMode.Automatic;
    [Tooltip("Size of the magazine")]
    public int magazineSize = 10;
    [Tooltip("Total ammo available")]
    public int totalAmmo = 100;
    [Tooltip("Time it takes to reload")]
    public float reloadDuration = 1.0f;
    [Tooltip("Automatically reload when trying to shoot with an empty magazine")]
    public bool autoReload = true;

    [Header("Muzzle Flash")]
    [Tooltip("The game object for muzzle flash light")]
    public GameObject muzzleFlashObject;
    [Tooltip("Duration the muzzle flash stays enabled")]
    public float muzzleFlashDuration = 0.1f;

    [Header("Camera Effects")]
    [Tooltip("Force of the camera impulse")]
    public float impulseForce = 1f;

    [Header("Events")]
    public UnityEvent onShot;
    public UnityEvent onReloadStart;

    [Header("Animation")]
    // Removed direct reference to GunAnimatorController

    [Header("Pooling System")]
    [Tooltip("Initial number of bullets to pool")]
    public int defaultCapacity = 20;
    public int maxPoolSize = 100;

    // Unity ObjectPool
    private ObjectPool<GameObject> pool;
    private float nextFireTime = 0f;
    private bool isFiring = false;
    private bool isReloading = false;
    private int currentAmmo;

    void Awake()
    {
        // If no firePoint assigned, use the gun's own transform
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // Initialize the pool
        pool = new ObjectPool<GameObject>(
            createFunc: CreateBullet,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnToPool,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        // Disable muzzle flash object on start
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }

        // Initialize ammo
        currentAmmo = magazineSize;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"Ammo: {currentAmmo}/{magazineSize} Total: {totalAmmo} | Mode: {fireMode} | Firing: {isFiring} | Reloading: {isReloading}");
    }

    void Update()
    {
        if (isFiring && fireMode == FireMode.Automatic)
        {
            AttemptShoot();
        }
    }

    public void StartFiring()
    {
        Debug.Log("StartFiring called");
        isFiring = true;
        AttemptShoot();
    }

    public void StopFiring()
    {
        Debug.Log("StopFiring called");
        isFiring = false;
    }

    public void Reload()
    {
        if (isReloading || totalAmmo <= 0 || currentAmmo >= magazineSize)
        {
            return;
        }

        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        onReloadStart?.Invoke();
        
        // Start old mag effect concurrently if needed
        StartCoroutine(EmitMagazineAfterDelay(oldMagazineDelay));

        // Wait for reload duration
        yield return new WaitForSeconds(reloadDuration);

        // Perform ammo refill logic
        int needed = magazineSize - currentAmmo;
        int reloadAmount = Mathf.Min(needed, totalAmmo);
        currentAmmo += reloadAmount;
        totalAmmo -= reloadAmount;

        isReloading = false;
    }

    IEnumerator EmitMagazineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EmitOldMagazine();
    }

    public int CurrentAmmo => currentAmmo;
    public int TotalAmmo => totalAmmo;

    public void AttemptShoot()
    {
        if (isReloading)
        {
            return;
        }

        // Auto-reload if empty and enabled
        if (currentAmmo <= 0 && autoReload && totalAmmo > 0)
        {
            Reload();
            return; // Return so we don't shoot while reloading
        }

        // Check if we have ammo
        if (currentAmmo <= 0)
        {
            return;
        }

        // Checks if enough time has passed since the last shot
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    #region Pooling Methods

    GameObject CreateBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned in GunController!");
            return null;
        }

        GameObject bullet = Instantiate(bulletPrefab);

        // Pass the pool reference to the projectile so it can return itself
        Projectile projectileScript = bullet.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetPool(pool);
        }

        return bullet;
    }

    void OnTakeFromPool(GameObject bullet)
    {
        // Do nothing here regarding position/rotation, 
        // as that is handled in Shoot() before activation logic runs in Bullet.OnEnable
        bullet.SetActive(true);
    }

    void OnReturnToPool(GameObject bullet)
    {
        bullet.SetActive(false);
    }
    #endregion

    void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned in GunController!");
            return;
        }

        if (pool == null)
        {
            Debug.LogError("Pool is not initialized in GunController!");
            return;
        }

        // Get from pool
        GameObject bullet = pool.Get();

        if (bullet != null)
        {
            // Position the bullet with random Y offset
            float randomYOffset = Random.Range(-yOffset, yOffset);
            bullet.transform.position = firePoint.position + new Vector3(0f, randomYOffset, 0f);

            // Determine direction based on firePoint scale
            float dir = Mathf.Sign(firePoint.lossyScale.x);

            // If facing left (negative scale), rotate 180 degrees around Y
            Quaternion checkRotation = (dir < 0) ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

            // Combine with firepoint's actual rotation
            bullet.transform.rotation = firePoint.rotation * checkRotation;

            // randomize damage logic if Bullet script is present
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = Random.Range(minDamage, maxDamage + 1);
            }

            // Launch the projectile
            Projectile projectile = bullet.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Launch();
            }
        }

        // Emit shell particle
        if (shellParticleSystem != null)
        {
            shellParticleSystem.Emit(1);
        }

        // Activate muzzle flash object
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(true);
        }

        // Start coroutine to disable after duration
        if (muzzleFlashObject != null)
        {
            StartCoroutine(DisableMuzzleFlashAfter(muzzleFlashDuration));
        }

        // Generate camera impulse
        ImpulseController.Instance.TriggerShotImpulse(impulseForce);

        // Invoke the shot event
        onShot?.Invoke();

        // Decrease ammo
        currentAmmo--;
    }

    IEnumerator DisableMuzzleFlashAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
    }

    public void EmitOldMagazine()
    {
        if (oldMagazineVFX != null)
        {
            oldMagazineVFX.Emit(1);
        }
    }
}
