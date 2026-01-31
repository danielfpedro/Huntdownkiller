using UnityEngine;
using UnityEngine.Pool;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    [Tooltip("The position where the bullet is spawned")]
    public Transform firePoint;
    [Tooltip("The Bullet Prefab to shoot")]
    public GameObject bulletPrefab;
    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.2f;
    [Tooltip("Damage dealt per bullet")]
    public int damage = 1;
    [Tooltip("Particle system for shell ejection")]
    public ParticleSystem shellParticleSystem;

    [Header("Pooling System")]
    [Tooltip("Initial number of bullets to pool")]
    public int defaultCapacity = 20;
    public int maxPoolSize = 100;

    // Unity ObjectPool
    private ObjectPool<GameObject> pool;
    private float nextFireTime = 0f;

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
    }

    public void AttemptShoot()
    {
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
            // Position the bullet
            bullet.transform.position = firePoint.position;

            // Determine direction based on firePoint scale
            float dir = Mathf.Sign(firePoint.lossyScale.x);

            // If facing left (negative scale), rotate 180 degrees around Y
            Quaternion checkRotation = (dir < 0) ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

            // Combine with firepoint's actual rotation
            bullet.transform.rotation = firePoint.rotation * checkRotation;

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
    }
}
