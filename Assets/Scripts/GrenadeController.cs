using UnityEngine;

public class GrenadeController : Projectile
{
    public float timeToExplode = 3f;
    public float radius = 5f;
    public float baseDamage = 10f;
    public float shootForce = 10f;
    private float timer = 0f;



    void OnEnable()
    {
        timer = 0f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public override void Launch()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(transform.right * shootForce, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HealthController>() != null)
        {
            Explode();
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeToExplode)
        {
            Explode();
        }
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            GoreController gc = hit.GetComponent<GoreController>();
            HealthController healthController = hit.GetComponent<HealthController>();
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            float force = Mathf.Max(0f, 1f - (distance / radius));
            if (gc != null && healthController != null)
            {
                int resultingHealth = healthController.currentHealth - (int)(baseDamage * force);
                healthController.TakeDamage((int)(baseDamage * force));
                if (resultingHealth <= 0)
                {
                    gc.ExplodeBody(transform, force);
                }
            }
        }
        ReturnToPool();
    }
}