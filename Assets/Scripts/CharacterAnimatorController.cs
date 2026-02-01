using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    Animator animator;
    PlayerMovement playerMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("Crouching", playerMovement.isCrouching);
        animator.SetBool("Dashing", playerMovement.isDashing);
    }
}
