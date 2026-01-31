using UnityEngine;

public class GoreController : MonoBehaviour
{
    public GameObject memberContainer;
    public float explosionForce = 10f;
    public float explosionTorque = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        memberContainer.SetActive(false);
    }

    public void ExplodeBody(Transform grenadeTransform, float force)
    {
        memberContainer.transform.parent = null;
        memberContainer.SetActive(true);

        foreach (Transform child in memberContainer.transform)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector3 direction = GetExplosionDirection(child, grenadeTransform);
                Vector2 forceDirection = direction * explosionForce * force;

                rb.AddForce(forceDirection, ForceMode2D.Impulse);
                rb.AddTorque(explosionTorque, ForceMode2D.Impulse);
            }
        }
    }

    private Vector3 GetExplosionDirection(Transform child, Transform grenade)
    {
        return (child.position - grenade.position).normalized;
    }
}
