using Unity.Cinemachine;
using UnityEngine;


public class ImpulseController : MonoBehaviour
{
    public static ImpulseController Instance { get; private set; }


    [Header("Impulse Sources")]
    [Tooltip("Cinemachine impulse source for shot effects")]
    public CinemachineImpulseSource shotImpulse;
    [Tooltip("Cinemachine impulse source for explosion effects")]
    public CinemachineImpulseSource explosionImpulse;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TriggerShotImpulse(float force = 1f)
    {
        if (shotImpulse != null)
        {
            shotImpulse.GenerateImpulseWithForce(force);
        }
    }

    public void TriggerExplosionImpulse(float force = 1f)
    {
        if (explosionImpulse != null)
        {
            explosionImpulse.GenerateImpulseWithForce(force);
        }
    }
}
