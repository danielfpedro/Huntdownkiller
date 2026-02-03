using UnityEngine;

public class GrenadeController : Projectile
{
    public float timeToExplode = 3f;
    public float radius = 5f;
    public float baseDamage = 10f;
    public float shootForce = 10f;
    public GameObject explosionVFX;
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
        ImpulseController.Instance.TriggerExplosionImpulse(0.5f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {

            HealthController healthController = hit.GetComponent<HealthController>();
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            float force = Mathf.Max(0f, 1f - (distance / radius));
            if (healthController != null)
            {
                int resultingHealth = healthController.currentHealth - (int)(baseDamage * force);
                healthController.TakeDamage((int)(baseDamage * force));

                GoreController gc = hit.GetComponent<GoreController>();
                if (gc != null && resultingHealth <= 0)
                {
                    Debug.Log("Exploding body with force: " + force);
                    gc.ExplodeBody(transform, force);
                }
            }
        }
        GameObject vfx = Instantiate(explosionVFX, transform.position * new Vector2(1, 1f), Quaternion.identity);
        vfx.transform.eulerAngles = new Vector3(-90, 0, 0);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        ps.Emit(20);
        ReturnToPool();
    }
}