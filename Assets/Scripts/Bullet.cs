using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage;

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
        }
        
        // Destroy (disable) bullet on impact
        // Only disable if it hits something that isn't the player or trigger
        if (!hitInfo.isTrigger) 
        {
             ReturnToPool();
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
