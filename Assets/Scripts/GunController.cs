using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class GunController : MonoBehaviour
{
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

    [Header("Muzzle Flash")]
    [Tooltip("The sprite renderer for the muzzle flash")]
    public SpriteRenderer muzzleRenderer;
    [Tooltip("List of textures for muzzle flash")]
    public List<Sprite> muzzleTextures;
    [Tooltip("Duration the muzzle flash stays enabled")]
    public float muzzleFlashDuration = 0.1f;
    [Tooltip("The game object for muzzle flash")]
    public GameObject muzzleFlashObject;

    [Header("Camera Effects")]
    [Tooltip("Cinemachine impulse source for camera shake")]
    public CinemachineImpulseSource impulseSource;
    [Tooltip("Force of the camera impulse")]
    public float impulseForce = 1f;

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

        // Disable muzzle renderer on start
        if (muzzleRenderer != null)
        {
            muzzleRenderer.enabled = false;
        }

        // Disable muzzle flash object on start
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
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

        // Enable muzzle flash and set random texture
        if (muzzleRenderer != null)
        {
            muzzleRenderer.enabled = true;
            if (muzzleTextures != null && muzzleTextures.Count > 0)
            {
                Sprite randomTexture = muzzleTextures[Random.Range(0, muzzleTextures.Count)];
                muzzleRenderer.sprite = randomTexture;
            }
        }

        // Activate muzzle flash object
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(true);
        }

        // Start coroutine to disable after duration
        if (muzzleRenderer != null || muzzleFlashObject != null)
        {
            StartCoroutine(DisableMuzzleFlashAfter(muzzleFlashDuration));
        }

        // Generate camera impulse
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulseWithForce(impulseForce);
        }
    }

    IEnumerator DisableMuzzleFlashAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (muzzleRenderer != null)
        {
            muzzleRenderer.enabled = false;
        }
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
    }
}
