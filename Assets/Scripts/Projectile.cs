using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    protected ObjectPool<GameObject> pool;

    public void SetPool(ObjectPool<GameObject> p)
    {
        pool = p;
    }

    protected void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(gameObject);
        }
    }

    public virtual void Launch()
    {
        // Default: do nothing
    }
}