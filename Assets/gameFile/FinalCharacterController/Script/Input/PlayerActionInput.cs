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

    private PlayerLocomotionInput playerLocomotionInput;
    private PlayerState playerState;

    public float inCombatTimer = 5f;
    public float maxInCombatTimer = 100f;

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

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);
    }

    public void SetAttackPressedFalse()
    {
        attackAnimation = false;
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

    public void IsAttackPressed(bool attack)
    {
        attackPressed = attack;
        attackAnimation = true;
        aimRigWeight = 1f;
    }

    #endregion
}
