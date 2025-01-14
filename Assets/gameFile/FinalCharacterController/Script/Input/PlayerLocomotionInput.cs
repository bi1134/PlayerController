using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput: MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    #region Class Variables
    [SerializeField] private bool holdToSprint = true;
    public bool sprintToggleOn {  get; private set; }
    public Vector2 movementInput { get; private set; }
    public Vector2 lookInput { get; private set; }

    public bool jumpPressed { get; private set; }

    #endregion

    #region Startup
    private void OnEnable()
    {
        //if instance is not null, then check playerControls is null or not
        if(PlayerInputManager.instance?.playerControls == null)
        {
           Debug.LogError("Player Controls is not initialized - can't enable");
            return;
        }
        PlayerInputManager.instance.playerControls.PlayerLocomotionMap.Enable();
        PlayerInputManager.instance.playerControls.PlayerLocomotionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance?.playerControls == null)
        {
            Debug.LogError("Player Controls is not initialized - can't disable");
            return;
        }

        PlayerInputManager.instance.playerControls.PlayerLocomotionMap.Disable();
        PlayerInputManager.instance.playerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Late Update Logic
    private void LateUpdate()
    {
        jumpPressed = false;
    }
    #endregion

    #region Input Callback
    public void OnMovement(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnToggleSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintToggleOn = holdToSprint || !sprintToggleOn;
        }
        else if (context.canceled)
        {
            sprintToggleOn = !holdToSprint && sprintToggleOn;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        jumpPressed = true;
    }

    #endregion
   
}
