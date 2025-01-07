using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
{
    #region Class Variables
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float cameraZoomSpeed = 0.1f;
    [SerializeField] private float cameraMinZoom = 1f;
    [SerializeField] private float cameraMaxZoom = 5f;

    private CinemachineThirdPersonFollow thirdPersonFollow;

    public Vector2 scrollInputNormalized { get; private set; }
    #endregion

    #region Startup

    private void Awake()
    {
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
    private void Update()
    {
        thirdPersonFollow.CameraDistance = Mathf.Clamp(thirdPersonFollow.CameraDistance + scrollInputNormalized.y, cameraMinZoom, cameraMaxZoom);
    }

    private void LateUpdate()
    {
        scrollInputNormalized = Vector2.zero;
    }

    #endregion


    #region Input Callback
    public void OnScrollCamera(InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
            
        Vector2 scrollInput = context.ReadValue<Vector2>();
        scrollInputNormalized = -1f * scrollInput.normalized * cameraZoomSpeed; 
    }
    #endregion
}
