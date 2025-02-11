using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditorInternal.VersionControl.ListControl;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    #region Class Varialbles
    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;

    public float rotationMismatch { get; private set; } = 0f;
    public bool isRotatingToTarget { get; private set; } = false;


    [Header("Base Movement")]
    public float runAcceleration = 25f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 50f;
    public float sprintSpeed = 7f;
    public float drag = 15f;
    public float jumpSpeed = 1.0f;
    public float jumpHeight = 3f;
    public float gravity = 25f;
    public float terminalVelocity = 50f;
    public float movingThreshold = 0.01f;
    public float inAirAcceleration = 25f;

    [Header("Animation")]
    public float playerModelRotationSpeed = 10f;
    public float rotateTargetTime = 0.25f;

    [Header("Dashing Settings")]
    public float dashForce = 10f;
    public float dashUpwardForce = 0f;
    public float dashLerp = 0.5f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 90f;

    [Header("Environmental Details")]
    [SerializeField] private LayerMask groundLayers;

    //get components stuff
    private PlayerLocomotionInput playerLocomotionInput;
    private PlayerState playerState;
    private PlayerActionInput playerActionInput;

    private Vector2 cameraRotation = Vector2.zero;
    private Vector2 playerTargetRotation = Vector2.zero;

    //rotate and jump
    private bool jumpLastFrame = false;
    private bool isRotatingClockwise = false;
    private float verticalVelocity = 0f;
    private float rotatingToTargetTimer = 0f;
    private float antiBump;
    private float stepOffset;
    private PlayerMovementState lastMovementState = PlayerMovementState.Falling;

    //dashing
    private Vector3 dashDirection;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private float speedChangeFactor;
    private float dashSpeedChangeFactor;
    private bool keepMomentum;
    private float moveSpeed;
    private PlayerDashState lastDashState = PlayerDashState.notDashing;

    #endregion

    #region Startup
    private void Awake()
    {
        playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        playerState = GetComponent<PlayerState>();
        playerActionInput = GetComponent<PlayerActionInput>();

        antiBump = sprintSpeed;
        stepOffset = characterController.stepOffset;
    }

    private void OnAnimatorMove()
    {
        //empty function to use the existing movement func because multi parent constrain forces the animator to half use the rootmotion
    }
    #endregion

    #region Update Logic
    private void Update()
    {
        HandleMovement();
        UpdateMovementState();
        HandleVerticalMovement();

        //handle actions
        HandleDashing();
    }

    private void UpdateMovementState()
    {
        lastMovementState = playerState.currentPlayerMovementState;

        bool canRun = CanRun();
        bool isMovementInput = playerLocomotionInput.movementInput != Vector2.zero;     //order
        bool isMovingLaterally = IsMovingLaterally();                                   //matter
        bool isSprinting = playerLocomotionInput.sprintToggleOn && isMovingLaterally && canRun;   //order matters
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting : isMovingLaterally 
                                        || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;
        playerState.SetPlayerMovementState(lateralState);

        //playerDashing
        if (playerActionInput.dashPressed)
        {
            desiredMoveSpeed = dashForce;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        //control airborn state
        if (!isGrounded || jumpLastFrame)
        {
            if (characterController.velocity.y > 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                jumpLastFrame = false;
                characterController.stepOffset = 0f;
                desiredMoveSpeed = desiredMoveSpeed < sprintSpeed ? runSpeed : sprintSpeed;
            }
            else
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                jumpLastFrame = false;
                characterController.stepOffset = 0f;
            }
        }
        else
        {
            characterController.stepOffset = stepOffset;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastDashState == PlayerDashState.Dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            StopAllCoroutines();
            if (keepMomentum)
            {
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastDashState = playerState.currentPlayerDashingState;
    }

    private void HandleVerticalMovement()
    {
        bool isGrounded = playerState.InGroundedState();

        verticalVelocity -= gravity * Time.deltaTime;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -antiBump;
        }

        if (playerLocomotionInput.jumpPressed && isGrounded)
        {
            verticalVelocity += Mathf.Sqrt(jumpSpeed * jumpHeight * gravity);
            jumpLastFrame = true;
        }

        if (playerState.IsStateGroundedState(lastMovementState) && !isGrounded)
        {
            verticalVelocity += antiBump;
        }

        if(Math.Abs(verticalVelocity) > Mathf.Abs(terminalVelocity))
        {
            verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
        }
    }

    private void HandleMovement()
    {
        //create quick references for current state
        bool isSprinting = playerState.currentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = playerState.InGroundedState();

        //state dependent acceleration and speed
        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                     isSprinting ? sprintAcceleration : runAcceleration;
        desiredMoveSpeed =   !isGrounded ? sprintSpeed :
                                        isSprinting ? sprintSpeed : runSpeed;

        //the movement direction is relative to the camera's orientation
        Vector3 movementDirection = GetCameraDirection();

        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        //new velocity = current velocity + our movement delta
        Vector3 newVelocity = characterController.velocity + movementDelta;
        //drag is also an acceleration so we will multiply with delta time again (kinematic equation)
        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        //use this so doesn't have to use a small if else statement
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), desiredMoveSpeed);
        newVelocity.y += verticalVelocity;
        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

        //move character (only call this once per tick)
        characterController.Move(newVelocity * Time.deltaTime);
    }

    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= characterController.slopeLimit;
        
        if(!validAngle && verticalVelocity < 0f)
        {
            velocity = Vector3.ProjectOnPlane(velocity, normal);
        }

        return velocity;
    }

    private void HandleDashing()
    {
        // Determine dash direction based on player input
        Vector3 dashDirection = GetCameraDirection();

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward; // Default to forward dash if no input
        }

        // Normalize dash direction and apply dash speed
        Vector3 forceToApply = dashDirection.normalized * (dashForce + moveSpeed) + transform.up * dashUpwardForce;

        if (playerActionInput.dashPressed && playerActionInput.dashAnimation)
        {
            // Set vertical velocity to zero to ignore gravity during dash
            verticalVelocity = 0f;
            // Move character
            characterController.Move(forceToApply * Time.deltaTime);
        }
    }


    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0f;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed) + dashLerp;
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }


    private Vector3 GetCameraDirection()
    {
        Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
        Vector3 cameraDirection = cameraRightXZ * playerLocomotionInput.movementInput.x + cameraForwardXZ * playerLocomotionInput.movementInput.y;

        return cameraDirection;
    }

    #endregion

    #region Late Update Logic
    private void LateUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        //update horizontal camera rotation based on player input and sensitivity
        cameraRotation.x += lookSenseH * playerLocomotionInput.lookInput.x;

        //clamping to limit how far the player can look up or down.
        //Mathf.Clamp Ensures the vertical rotation stays within the range [-lookLimitV, lookLimitV]
        cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSenseV * playerLocomotionInput.lookInput.y, -lookLimitV, lookLimitV);

        playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * playerLocomotionInput.lookInput.x;
        
        float rotationTolerance = 90f;
        bool isIdling = playerState.currentPlayerMovementState == PlayerMovementState.Idling;
        isRotatingToTarget = rotatingToTargetTimer > 0;

        //rotate if not idling
        if (!isIdling)
        {
            RotatePlayerToTarget();
        }
        //if rotation mismatch is not within tolerance, or rotating to target is true then rotate
        else if(Mathf.Abs(rotationMismatch) > rotationTolerance || isRotatingToTarget)
        {
            UpdateIdleRotation(rotationTolerance);
        }


        //apply the updated horizontal rotation to the player's body
        playerCamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);
        
        //get angle between camera and player
        Vector3 camFowardedProjectedXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camFowardedProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct,transform.up));
        rotationMismatch = sign * Vector3.Angle(transform.forward, camFowardedProjectedXZ);

    }

    private void UpdateIdleRotation(float rotationTolerance)
    {
        //initiate new rotation direction
        if (Mathf.Abs(rotationMismatch) > rotationTolerance)
        {
            rotatingToTargetTimer = rotateTargetTime;
            isRotatingClockwise = rotationMismatch > rotationTolerance;
        }
        rotatingToTargetTimer -= Time.deltaTime;


        //rotate player
        if(isRotatingClockwise && rotationMismatch > 0f||
            !isRotatingClockwise && rotationMismatch < 0f)
        { 
            RotatePlayerToTarget(); 
        }
    }

    private void RotatePlayerToTarget()
    {
        Quaternion targetRotationX = Quaternion.Euler(0f, playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }

    #endregion

    #region State Check  

    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.y);

        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()
    {
        bool grounded = playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

        return grounded;
    }
    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePostion = new Vector3(transform.position.x, transform.position.y - characterController.radius, transform.position.z);
        
        bool grounded = Physics.CheckSphere(spherePostion, characterController.radius, groundLayers, QueryTriggerInteraction.Ignore);

        return grounded;
    }

    private bool IsGroundedWhileAirborne()
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= characterController.slopeLimit;

        return characterController.isGrounded && validAngle;
    }

    private bool CanRun()
    {
        //this means the player is moving diagonally at 45 degrees or foward, if so, we can run
        return playerLocomotionInput.movementInput.y >= Mathf.Abs(playerLocomotionInput.movementInput.x);
    }

    #endregion
}
