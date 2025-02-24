using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerActionInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
{
    #region Class Variables
    [Header("References")]
    [SerializeField] private Rig aimRig;


    [Header("Combat")]
    public float inCombatTime = 5f;
    public float inCombatTimeTimer = 0f;

    [Header("Dash")]
    public float dashCooldown = 3f;
    private float dashCooldownTimer = 0f;
    public float dashDuration = 0.25f;
    private float dashDurationTimer = 0f;

    //aim rig
    private float aimRigWeight;

    [SerializeField] public bool holdToShoot = true;
    public bool attackPressed { get; private set; }
    public bool attackAnimation { get; private set; }

    public bool dashPressed { get; private set; }
    public bool dashAnimation { get; private set; }

    //get component stuff
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
        OutOfCombatState();
        DashCooldown();

        //aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);
    }

    #endregion


    #region Input Callback
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsAttackPressed(true);
            inCombatTimeTimer = inCombatTime;
        }
        else if (context.canceled)
        {
            IsAttackPressed(false);
        }
    }

    public void OnDodging(InputAction.CallbackContext context)
    {
        if (!context.performed || dashCooldownTimer > 0)
            return;

        IsDashPressed(context.performed);
        dashCooldownTimer = dashCooldown;
        dashDurationTimer = dashDuration;
    }
  
    #endregion

    #region Functions

    private void OutOfCombatState()
    {
        if (playerState.currentPlayerCombatState == PlayerCombatState.InCombat &&
             !attackPressed)
        {
            inCombatTimeTimer -= Time.deltaTime;
            if (inCombatTimeTimer < 0)
            {
                playerState.SetPlayerCombatState(PlayerCombatState.notInCombat);
                //aimRigWeight = 0f;
            }
        }
    }

    private void DashCooldown()
    {
        if (!dashAnimation)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                dashCooldownTimer = 0;
            }
        }

        dashDurationTimer -= Time.deltaTime;
        if(dashDurationTimer <= 0)
        {
            dashDurationTimer = 0;
            SetDashPressedFalse();
        }
    }

    public void IsAttackPressed(bool attack)
    {
        //aimRigWeight = 1;
        if (holdToShoot)
        {
            // Automatic Mode: Hold to shoot, stop when released
            attackPressed = attack;  // Directly assign true on press, false on release
        }
        else
        {
            // Semi-Auto Mode: Fire only one bullet per button press
            if (attack)
            {
                attackPressed = true;  // Fire one bullet when pressed
            }
            else
            {
                attackPressed = false; // Prevent continuous firing
            }
        }
    }

    public void SetAttackPressedFalse()
    {
        attackAnimation = false;
    }

    public void IsDashPressed(bool dash)
    {
        dashPressed = dash;
        dashAnimation = true;
        playerState.SetPlayerDashingState(PlayerDashState.Dashing);
    }

    public void SetDashPressedFalse()
    {
        dashAnimation = false;
        dashPressed = false;
        playerState.SetPlayerDashingState(PlayerDashState.notDashing);
    }
    #endregion
}
