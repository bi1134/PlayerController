using System.Linq;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float locomotionBlendSpeed = 0.02f;

    private PlayerLocomotionInput playerLocomotionInput;
    private PlayerState playerState;
    private PlayerController playerController;
    private PlayerActionInput playerActionInput;

    //locomotion
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isDodgingHash = Animator.StringToHash("isDodging");
   
    //camera/rotation
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
    
    //player action
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    private int[] actionHashes;
    private static int isCombatHash = Animator.StringToHash("isCombat");

    private Vector2 currentBlendInput = Vector2.zero;
    private float sprintMaxBlendValue = 1.5f;
    private float runMaxBlendValue = 1f;
    private float walkMaxBlendValue = 0.5f;

    private void Awake()
    {
        playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        playerState = GetComponent<PlayerState>();
        playerController = GetComponent<PlayerController>();
        playerActionInput = GetComponent<PlayerActionInput>();

        actionHashes = new int[] { };
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isIdling = playerState.currentPlayerMovementState == PlayerMovementState.Idling;
        bool isRunning = playerState.currentPlayerMovementState == PlayerMovementState.Running;
        bool isSprinting = playerState.currentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isJumping = playerState.currentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = playerState.currentPlayerMovementState == PlayerMovementState.Falling;
        bool isGrounded = playerState.InGroundedState();
        bool isPlayingAction = actionHashes.Any(hash => animator.GetBool(hash));
        bool isInCombat = playerState.currentPlayerCombatState == PlayerCombatState.InCombat;

        bool isRunBlendValue = isRunning || isSprinting || isFalling;
        Vector2 inputTarget = isSprinting ? playerLocomotionInput.movementInput * sprintMaxBlendValue
                             :isRunBlendValue ? playerLocomotionInput.movementInput * runMaxBlendValue : playerLocomotionInput.movementInput * walkMaxBlendValue;
                                
        currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        animator.SetBool(isGroundedHash, isGrounded);
        animator.SetBool(isIdlingHash, isIdling);
        animator.SetBool(isJumpingHash, isJumping);
        animator.SetBool(isFallingHash, isFalling);
        animator.SetBool(isRotatingToTargetHash, playerController.isRotatingToTarget);
        animator.SetBool(isAttackingHash, playerActionInput.attackAnimation);
        animator.SetBool(isPlayingActionHash, isPlayingAction);
        animator.SetBool(isCombatHash, isInCombat);
        animator.SetBool(isDodgingHash, playerActionInput.dodgeAnimation);

        animator.SetFloat(inputXHash, currentBlendInput.x);
        animator.SetFloat(inputYHash, currentBlendInput.y);
        animator.SetFloat(inputMagnitudeHash, currentBlendInput.magnitude);
        animator.SetFloat(rotationMismatchHash, playerController.rotationMismatch);
    }

}
