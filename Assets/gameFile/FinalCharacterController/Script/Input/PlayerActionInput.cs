using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerActionInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
{
    #region Class Variables
    [SerializeField] private Rig aimRig;


    public bool attackPressed { get; private set; }
    public bool attackAnimation { get; private set; }

    public bool dodgePressed { get; private set; }
    public bool dodgeAnimation { get; private set; }

    private PlayerLocomotionInput playerLocomotionInput;
    private PlayerState playerState;

    public float inCombatTimer = 5f;
    public float maxInCombatTimer = 10f;

    public float dodgeCooldown = 0f;
    public float maxDodgeCooldown = 3f;

    private float aimRigWeight;

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
        DodgeCooldown();

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);
    }

    #endregion


    #region Input Callback
    public void OnAttack(InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
        IsAttackPressed(context.performed);
        inCombatTimer = maxInCombatTimer;
    }

    public void OnDodging(InputAction.CallbackContext context)
    {
        if (!context.performed || dodgeCooldown > 0)
            return;

        IsDodgePressed(context.performed);
        dodgeCooldown = maxDodgeCooldown;
    }
  
    #endregion

    #region Functions

    private void OutOfCombatState()
    {
        if (playerState.currentPlayerCombatState == PlayerCombatState.InCombat &&
             !attackPressed)
        {
            inCombatTimer -= Time.deltaTime;
            if (inCombatTimer < 0)
            {
                playerState.SetPlayerCombatState(PlayerCombatState.notInCombat);
                aimRigWeight = 0f;
            }
        }
    }

    private void DodgeCooldown()
    {
        if (!dodgeAnimation)
        {
            dodgeCooldown -= Time.deltaTime;
            if (dodgeCooldown <= 0)
            {
                dodgeCooldown = 0;
            }
        }
    }

    public void IsAttackPressed(bool attack)
    {
        attackPressed = attack;
        attackAnimation = true;
        aimRigWeight = 1f;
    }

    public void SetAttackPressedFalse()
    {
        attackAnimation = false;
    }

    public void IsDodgePressed(bool dodge)
    {
        dodgePressed = dodge;
        dodgeAnimation = true;
    }

    public void SetDodgePressedFalse()
    {
       dodgeAnimation = false;
    }
    #endregion
}
