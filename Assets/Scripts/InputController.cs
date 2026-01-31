using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
  [Header("References")]
  [Tooltip("The movement script to control.")]
  public PlayerMovement playerMovement;

  private InputSystem_Actions inputActions;
  private WeaponsManager weaponsManager;

  void Awake()
  {
    inputActions = new InputSystem_Actions();
    weaponsManager = playerMovement.gameObject.GetComponent<WeaponsManager>();
  }

  void OnEnable()
  {
    inputActions.Enable();
    inputActions.Player.Jump.performed += OnJump;
    inputActions.Player.Dash.performed += OnDash;
    inputActions.Player.Next.performed += OnNextWeapon;
  }

  void OnDisable()
  {
    inputActions.Player.Jump.performed -= OnJump;
    inputActions.Player.Dash.performed -= OnDash;
    inputActions.Player.Next.performed -= OnNextWeapon;
    inputActions.Disable();
  }

  void Update()
  {
    if (weaponsManager?.CurrentWeapon != null)
    {
      // Check if the 'Attack' button is currently held down
      if (inputActions.Player.Attack.IsPressed())
      {
        weaponsManager.CurrentWeapon.AttemptShoot();
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

  private void OnNextWeapon(InputAction.CallbackContext context)
  {
    weaponsManager?.Next();
  }
}
