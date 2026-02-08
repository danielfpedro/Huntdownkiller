using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class GunController : MonoBehaviour
{
    public enum FireMode
    {
        Manual,
        Automatic
    }

    #region Gun Settings
    [Header("Gun Configuration")]
    [Tooltip("The transform position where the bullet will be instantiated.")]
    public Transform firePoint;

    [Tooltip("The prefab of the bullet to be shot.")]
    public GameObject bulletPrefab;

    [Tooltip("Determines the firing behavior: Manual (single shot) or Automatic (continuous).")]
    public FireMode fireMode = FireMode.Automatic;

    [Tooltip("The time interval in seconds between shots.")]
    public float fireRate = 0.2f;

    [Header("Damage Stats")]
    [Tooltip("The minimum damage a single bullet can inflict.")]
    public int minDamage = 1;

    [Tooltip("The maximum damage a single bullet can inflict.")]
    public int maxDamage = 3;

    [Header("Accuracy")]
    [Tooltip("Random vertical offset applied to the bullet spawn position for shot variation.")]
    public float yOffset = 0f;

    [Header("Ammo Configuration")]
    [Tooltip("The maximum number of bullets a single magazine can hold.")]
    public int magazineSize = 10;

    [Tooltip("The number of full magazines available in reserve. Set to int.MaxValue for infinite ammo.")]
    public int totalMagazines = 10;
    
    [Tooltip("The time in seconds it takes to complete a reload.")]
    public float reloadDuration = 1.0f;

    [Tooltip("If enabled, the gun auto-reloads when firing with an empty magazine.")]
    public bool autoReload = true;
    #endregion

    #region Visual Effects
    [Header("Muzzle Flash")]
    [Tooltip("The GameObject representing the muzzle flash effect.")]
    public GameObject muzzleFlashObject;

    [Tooltip("How long the muzzle flash remains visible after a shot.")]
    public float muzzleFlashDuration = 0.1f;

    [Header("Particle Systems")]
    [Tooltip("Particle system played when a shell casing is ejected.")]
    public ParticleSystem shellParticleSystem;

    [Tooltip("Particle system played when the old magazine is ejected during reload.")]
    public ParticleSystem oldMagazineVFX;

    [Tooltip("Delay in seconds before the old magazine particle is emitted during reload.")]
    public float oldMagazineDelay = 0.5f;

    [Header("Camera Effects")]
    [Tooltip("The force intensity of the camera shake/impulse when shooting.")]
    public float impulseForce = 1f;
    #endregion

    #region Events
    [Header("Events")]
    [Tooltip("Event invoked whenever a shot is successfully fired.")]
    public UnityEvent onShot;

    [Tooltip("Event invoked when the reload sequence begins.")]
    public UnityEvent onReloadStart;
    #endregion

    #region Pooling Setup
    [Header("Pooling System")]
    [Tooltip("The initial number of items to create in the pool.")]
    public int defaultCapacity = 20;

    [Tooltip("The maximum number of items the pool can hold.")]
    public int maxPoolSize = 100;
    #endregion

    // Logic Variables
    private ObjectPool<GameObject> pool;
    private float nextFireTime = 0f;
    private bool isFiring = false;
    private bool isReloading = false;
    private int currentAmmo;
    private Coroutine muzzleFlashCoroutine;

    // Properties
    public int CurrentAmmo => currentAmmo;
    public int TotalMagazines => totalMagazines;
    [Header("UI Settings")]
    [Tooltip("The sprite used for UI representation of this gun.")]
    public Sprite icon; // For UI representation

    private void Awake()
    {
        // Fallback if firePoint is not set
        if (firePoint == null)
        {
            firePoint = transform;
        }

        InitializePool();

        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }

        currentAmmo = magazineSize;
    }

    private void InitializePool()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: CreateBullet,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnToPool,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );
    }

    private void Update()
    {
        if (isFiring && fireMode == FireMode.Automatic)
        {
            AttemptShoot();
        }
    }

    #region Public API

    /// <summary>
    /// Starts the firing sequence. Used by input handlers.
    /// </summary>
    public void StartFiring()
    {
        isFiring = true;
        AttemptShoot();
    }

    /// <summary>
    /// Stops the firing sequence.
    /// </summary>
    public void StopFiring()
    {
        isFiring = false;
    }

    /// <summary>
    /// Initiates the reload process if conditions are met.
    /// </summary>
    public void Reload()
    {
        if (isReloading || (totalMagazines != -1 && totalMagazines <= 0) || currentAmmo >= magazineSize)
        {
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    /// <summary>
    /// Manually emit the empty magazine effect (e.g. from animation event).
    /// </summary>
    public void EmitOldMagazine()
    {
        if (oldMagazineVFX != null)
        {
            oldMagazineVFX.Emit(1);
        }
    }

    #endregion

    #region Shooting Logic

    public void AttemptShoot()
    {
        if (isReloading) return;

        // Handle auto-reload
        if (currentAmmo <= 0)
        {
            if (autoReload && (totalMagazines == -1 || totalMagazines > 0))
            {
                Reload();
            }
            return;
        }

        // Fire rate check
        if (Time.time >= nextFireTime)
        {
            PerformShoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void PerformShoot()
    {
        if (!ValidateComponents()) return;

        GameObject bullet = pool.Get();
        if (bullet != null)
        {
            SetupBullet(bullet);
        }

        PlayShootEffects();

        // Apply Camera Impulse
        if (ImpulseController.Instance != null)
        {
            ImpulseController.Instance.TriggerShotImpulse(impulseForce);
        }

        onShot?.Invoke();
        currentAmmo--;
    }

    private bool ValidateComponents()
    {
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned in GunController!");
            return false;
        }
        if (pool == null)
        {
            Debug.LogError("Pool is not initialized in GunController!");
            return false;
        }
        return true;
    }

    private void SetupBullet(GameObject bullet)
    {
        // 1. Set Position
        float randomYOffset = Random.Range(-yOffset, yOffset);
        bullet.transform.position = firePoint.position + new Vector3(0f, randomYOffset, 0f);

        // 2. Set Rotation
        float dir = Mathf.Sign(firePoint.lossyScale.x);
        Quaternion checkRotation = (dir < 0) ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;
        bullet.transform.rotation = firePoint.rotation * checkRotation;

        // 3. Apply Stats
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = Random.Range(minDamage, maxDamage + 1);
        }

        // 4. Launch
        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Launch();
        }
    }

    private void PlayShootEffects()
    {
        // Shell Ejection
        if (shellParticleSystem != null)
        {
            shellParticleSystem.Emit(1);
        }

        // Muzzle Flash
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(true);

            // Restart the disable timer
            if (muzzleFlashCoroutine != null) StopCoroutine(muzzleFlashCoroutine);
            muzzleFlashCoroutine = StartCoroutine(DisableMuzzleFlashAfter(muzzleFlashDuration));
        }
    }

    private IEnumerator DisableMuzzleFlashAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
        muzzleFlashCoroutine = null;
    }

    #endregion

    #region Reload Logic

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        onReloadStart?.Invoke();

        // Schedule magazine drop effect
        if (oldMagazineVFX != null)
        {
            StartCoroutine(EmitMagazineAfterDelay(oldMagazineDelay));
        }

        yield return new WaitForSeconds(reloadDuration);

        // Magazine-style reload: discard remaining ammo, use one full magazine
        if (totalMagazines == -1 || totalMagazines > 0)
        {
            if (totalMagazines != -1)
            {
                totalMagazines--;
            }
            currentAmmo = magazineSize;
        }

        isReloading = false;
    }

    private IEnumerator EmitMagazineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EmitOldMagazine();
    }

    #endregion

    #region Pooling Callbacks

    private GameObject CreateBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned in GunController!");
            return null;
        }

        GameObject bullet = Instantiate(bulletPrefab);

        // Inject pool dependency check
        Projectile projectileScript = bullet.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetPool(pool);
        }

        return bullet;
    }

    private void OnTakeFromPool(GameObject bullet)
    {
        bullet.SetActive(true);
    }

    private void OnReturnToPool(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    #endregion
}
