using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
  [Header("References")]
  [Tooltip("The target game object with movement and weapons components.")]
  public GameObject target;

  private InputSystem_Actions inputActions;
  private PlayerMovement playerMovement;
  private WeaponsManager weaponsManager;

  void Awake()
  {
    inputActions = new InputSystem_Actions();
    if (target != null)
    {
      playerMovement = target.GetComponent<PlayerMovement>();
      weaponsManager = target.GetComponent<WeaponsManager>();
    }
  }

  void OnEnable()
  {
    inputActions.Enable();
    inputActions.Player.Jump.performed += OnJump;
    inputActions.Player.Dash.performed += OnDash;
    inputActions.Player.Next.performed += OnNextWeapon;
    inputActions.Player.Shot.started += OnPrimaryFireStarted;
    inputActions.Player.Shot.canceled += OnPrimaryFireCanceled;
    inputActions.Player.ShotSecondary.started += OnSecondaryFireStarted;
    inputActions.Player.ShotSecondary.canceled += OnSecondaryFireCanceled;
    inputActions.Player.Reload.performed += OnReload;
  }

  void OnDisable()
  {
    inputActions.Player.Jump.performed -= OnJump;
    inputActions.Player.Dash.performed -= OnDash;
    inputActions.Player.Next.performed -= OnNextWeapon;
    inputActions.Player.Shot.started -= OnPrimaryFireStarted;
    inputActions.Player.Shot.canceled -= OnPrimaryFireCanceled;
    inputActions.Player.ShotSecondary.started -= OnSecondaryFireStarted;
    inputActions.Player.ShotSecondary.canceled -= OnSecondaryFireCanceled;
    inputActions.Player.Reload.performed -= OnReload;
    inputActions.Disable();
  }

  void Update()
  {
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

  private void OnPrimaryFireStarted(InputAction.CallbackContext context)
  {
    if (weaponsManager?.CurrentWeapon != null)
    {
      weaponsManager.CurrentWeapon.StartFiring();
    }
  }

  private void OnPrimaryFireCanceled(InputAction.CallbackContext context)
  {
    if (weaponsManager?.CurrentWeapon != null)
    {
      weaponsManager.CurrentWeapon.StopFiring();
    }
  }

  private void OnSecondaryFireStarted(InputAction.CallbackContext context)
  {
    if (weaponsManager?.CurrentSecondaryWeapon != null)
    {
      weaponsManager.CurrentSecondaryWeapon.StartFiring();
    }
  }

  private void OnSecondaryFireCanceled(InputAction.CallbackContext context)
  {
    if (weaponsManager?.CurrentSecondaryWeapon != null)
    {
      weaponsManager.CurrentSecondaryWeapon.StopFiring();
    }
  }

  private void OnReload(InputAction.CallbackContext context)
  {
    if (weaponsManager?.CurrentWeapon != null)
    {
      weaponsManager.CurrentWeapon.Reload();
    }
    if (weaponsManager?.CurrentSecondaryWeapon != null)
    {
      weaponsManager.CurrentSecondaryWeapon.Reload();
    }
  }
}
