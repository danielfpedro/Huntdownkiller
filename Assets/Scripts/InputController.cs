using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
  [Header("References")]
  [Tooltip("The gun this input controller will fire.")]
  public GunController targetWeapon;
  [Tooltip("The movement script to control.")]
  public PlayerMovement playerMovement;

  private InputSystem_Actions inputActions;

  void Awake()
  {
    inputActions = new InputSystem_Actions();
  }

  void OnEnable()
  {
    inputActions.Enable();
    inputActions.Player.Jump.performed += OnJump;
    inputActions.Player.Dash.performed += OnDash;
  }

  void OnDisable()
  {
    inputActions.Player.Jump.performed -= OnJump;
    inputActions.Player.Dash.performed -= OnDash;
    inputActions.Disable();
  }

  void Update()
  {
    if (targetWeapon != null)
    {
      // Check if the 'Attack' button is currently held down
      if (inputActions.Player.Attack.IsPressed())
      {
        targetWeapon.AttemptShoot();
      }
    }

    if (playerMovement != null)
    {
      // Move
      Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
      playerMovement.Move(moveInput);

      // Crouch
      bool isCrouching = inputActions.Player.Crouch.IsPressed();
      playerMovement.SetCrouch(isCrouching);
    }
  }

  private void OnJump(InputAction.CallbackContext context)
  {
    if (playerMovement != null)
    {
      playerMovement.Jump();
    }
  }

  private void OnDash(InputAction.CallbackContext context)
  {
    if (playerMovement != null)
    {
      playerMovement.AttemptDash();
    }
  }
}
