using UnityEngine;

public class GunAnimatorController : MonoBehaviour
{
    private GunController gunController;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        gunController = GetComponent<GunController>();
        if (gunController != null)
        {
            gunController.onShot.AddListener(() => SetTrigger("Shot"));
            gunController.onReloadStart.AddListener(() => SetTrigger("Reload"));
        }
    }

    public void SetTrigger(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}
