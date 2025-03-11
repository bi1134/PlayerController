using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    public float recoilX = -2f; // Vertical recoil (Pitch)
    public float recoilY = 1f;  // Horizontal recoil (Yaw)
    public float recoilZ = 0.5f; // Roll recoil

    public float snappiness = 6f;  // Speed of recoil transition
    public float returnSpeed = 2f; // Speed at which recoil resets

    private Vector3 targetRecoil;
    private Vector3 currentRecoil;

    [HideInInspector] public Transform cameraTransform;
    [HideInInspector] public Animator rigController;

    private void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("[WeaponRecoil] Camera reference is missing!");
        }
    }

    public void GenerateRecoil(string weaponName)
    {
        targetRecoil += new Vector3(
            recoilX,
            Random.Range(-recoilY, recoilY),
            0f // Keep Z-axis zero to avoid unnecessary tilt
        );
        rigController.Play("WeaponRecoil" + weaponName, 1, 0.0f);
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Smoothly apply recoil
        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, snappiness * Time.deltaTime);
        cameraTransform.localRotation = Quaternion.Euler(currentRecoil) * cameraTransform.localRotation;

        // Gradually reset recoil to zero
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, returnSpeed * Time.deltaTime);
    }
}
