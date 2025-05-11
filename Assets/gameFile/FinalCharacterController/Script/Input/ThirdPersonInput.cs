using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
{
    #region Class Variables
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float idleZoom = 1.5f;
    [SerializeField] private float runZoom = 4.5f;
    [SerializeField] private float dashZoom = 5.5f;
    [SerializeField] private float zoomLerpSpeed = 5f;
    [SerializeField] private float dashLerpSpeed = 10f;

    private PlayerState playerState;

    private CinemachineThirdPersonFollow thirdPersonFollow;

    #endregion

    #region Startup

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
        thirdPersonFollow = cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
    }

    private void OnEnable()
    {
        //if instance is not null, then check playerControls is null or not
        if (PlayerInputManager.instance?.playerControls == null)
        {
            Debug.LogError("Player Controls is not initialized - can't enable");
            return;
        }
        PlayerInputManager.instance.playerControls.ThirdPersonMap.Enable();
        PlayerInputManager.instance.playerControls.ThirdPersonMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance?.playerControls == null)
        {
            Debug.LogError("Player Controls is not initialized - can't disable");
            return;
        }

        PlayerInputManager.instance.playerControls.ThirdPersonMap.Disable();
        PlayerInputManager.instance.playerControls.ThirdPersonMap.RemoveCallbacks(this);
    }

    #endregion

    #region Update Logic
    private void LateUpdate()
    {
        float targetZoom = idleZoom;
        float lerpSpeed = zoomLerpSpeed;

        if (playerState.currentPlayerDashingState == PlayerDashState.Dashing)
        {
            targetZoom = dashZoom;
            lerpSpeed = dashLerpSpeed;
        }
        else
        {
            switch (playerState.currentPlayerMovementState)
            {
                case PlayerMovementState.Sprinting:
                case PlayerMovementState.Jumping:
                case PlayerMovementState.Falling:
                    targetZoom = runZoom;
                    break;
                case PlayerMovementState.Running:
                case PlayerMovementState.Idling:
                case PlayerMovementState.Walking:
                    targetZoom = idleZoom;
                    break;
            }
        }

        thirdPersonFollow.CameraDistance = Mathf.Lerp(
            thirdPersonFollow.CameraDistance,
            targetZoom,
            Time.deltaTime * lerpSpeed
        );
    }

    #endregion


    #region Input Callback
    public void OnScrollCamera(InputAction.CallbackContext context)
    {
        
    }
    #endregion
}
