using UnityEngine;
using UnityEngine.InputSystem;

public class GoreController : MonoBehaviour
{
    public GameObject memberContainer;
    public float explosionForce = 10f;
    public float explosionTorque = 10f;
    public bool debugMode = false;
    public GameObject debugGrenade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        memberContainer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExplodeBody(debugGrenade.transform, 1f);
        }

        if (debugMode && debugGrenade != null)
        {
            foreach (Transform child in memberContainer.transform)
            {
                Vector3 direction = GetExplosionDirection(child, debugGrenade.transform);
                Debug.DrawRay(child.position, direction * 5f, Color.red);
                Debug.DrawRay(debugGrenade.transform.position, child.position - debugGrenade.transform.position, Color.blue);
            }
        }
    }

    private Vector3 GetExplosionDirection(Transform child, Transform grenade)
    {
        return (child.position - grenade.position).normalized;
    }

    public void ExplodeBody(Transform grenadeTransform, float force)
    {
        Debug.Log("Exploding body into gore pieces!");
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
            else
            {
                Debug.LogWarning("No Rigidbody2D found on gore piece: " + child.name);
            }
        }
    }
}
