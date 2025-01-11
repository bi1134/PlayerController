using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerActionInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
{
    #region Class Variables
    public bool attackPressed { get; private set; }

    private PlayerLocomotionInput playerLocomotionInput;
    private PlayerState playerState;

    #endregion

    #region Startup

    private void Awake()
    {
        playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        playerState = GetComponent<PlayerState>();
    }

    private void OnEnable()
    {
        //if instance is not null, then check playerControls is null or not
        if (PlayerInputManager.instance?.playerControls == null)
        {
            Debug.LogError("Player Controls is not initialized - can't enable");
            return;
        }
        PlayerInputManager.instance.playerControls.PlayerActionMap.Enable();
        PlayerInputManager.instance.playerControls.PlayerActionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance?.playerControls == null)
        {
            Debug.LogError("Player Controls is not initialized - can't disable");
            return;
        }

        PlayerInputManager.instance.playerControls.PlayerActionMap.Disable();
        PlayerInputManager.instance.playerControls.PlayerActionMap.RemoveCallbacks(this);
    }


    #endregion

    #region Update Logic

    private void Update()
    {
        if( playerLocomotionInput.movementInput != Vector2.zero || 
            playerState.currentPlayerMovementState == PlayerMovementState.Jumping ||
            playerState.currentPlayerMovementState == PlayerMovementState.Falling)
        {

        }
    }

    public void SetAttackPressedFalse()
    {
        attackPressed = false;
    }

    #endregion


    #region Input Callback
    public void OnAttack(InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
        attackPressed = true;
    }
    #endregion
}
