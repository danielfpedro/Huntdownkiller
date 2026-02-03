using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage;
    public LayerMask returnOnHitLayers;
    public ParticleSystem hitVFX;

    private float timer;
    private IObjectPool<GameObject> pool;

    public void SetPool(IObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        // Move the bullet straight in its local right direction (rotation determines actual direction)
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Handle lifetime
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Try to get HealthController and hurt it
        HealthController health = hitInfo.GetComponent<HealthController>();
        if (health != null)
        {
            health.TakeDamage(damage, -transform.right);
            ReturnToPool();
            return;
        }
        // Return to pool if hit health or specific layers
        if ((returnOnHitLayers.value & (1 << hitInfo.gameObject.layer)) != 0)
        {
            if (hitVFX != null)
            {
                Vector2 normal = ((Vector2)transform.position - (Vector2)hitInfo.transform.position).normalized;
                float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
                GameObject vfx = Instantiate(hitVFX.gameObject, transform.position, Quaternion.Euler(0, 0, angle));
                vfx.GetComponent<ParticleSystem>().Emit(100);
            }
            ReturnToPool();
            return;
        }
    }

    void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
