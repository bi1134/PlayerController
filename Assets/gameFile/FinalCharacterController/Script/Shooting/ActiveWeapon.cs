using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ActiveWeapon : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] public Transform weaponParent;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private LayerMask aimColliderMask;
    [SerializeField] private Rig handIK;

    [SerializeField] private Transform leftGrip;
    [SerializeField] private Transform rightGrip;
    [SerializeField]private Animator rigController;

    //references
    private PlayerActionInput playerActionInput;
    private WeaponRaycast weapon;


    //aiming
    public float lerpRotationSpeed = 10f;
    private Vector3 mouseWorldPosition;
    private Ray ray;
    private Transform hitTransform;

    #endregion

    #region Start
    private void Start()
    {

        playerActionInput = GetComponent<PlayerActionInput>();

        WeaponRaycast existingWeapon = GetComponentInChildren<WeaponRaycast>();
        if (existingWeapon)
        {
            EquipWeapon(existingWeapon);
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        HandleAiming();

        if (weapon)
        { 
            if (playerActionInput.attackPressed)
            {
                if (!weapon.isFiring) // Start firing only once
                {
                    weapon.StartFiring();
                }

                if (playerActionInput.holdToShoot) // Auto-fire mode
                {
                    weapon.UpdateFiring(Time.deltaTime);
                }
            }
            else if (weapon.isFiring) // Stop firing when button is released
            {
                weapon.StopFiring();
            }

            if(playerActionInput.holsterPressed)
            {
                bool isHolster = rigController.GetBool("isHolster");
                rigController.SetBool("isHolster", !isHolster);
            }
        }
       
    }
    #endregion

    #region Func

    private void HandleAiming()
    {
        if (Camera.main == null) return;

        // Get the center of the screen as the aiming direction
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
            hitPoint.position = raycastHit.point;
        }

        // Smoothly move the aim point to follow the hit point
        aimTarget.position = Vector3.Lerp(aimTarget.position, hitPoint.position, Time.deltaTime * lerpRotationSpeed);
    }

    public void EquipWeapon(WeaponRaycast newWeapon)
    {
        if (weapon)
        {
            //destroy the game object not component
            weapon.gameObject.SetActive(false);
        }
        weapon = newWeapon;

        weapon.transform.SetParent(weaponParent);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        rigController.Play("Equip" + weapon.weaponName);


        weapon.gameObject.SetActive(true);
        weapon.Initialize();

      
    }


    #endregion

    #region Return Values
    public Vector3 GetAimPosition()
    {
        return mouseWorldPosition;
    }

    public Transform GetHitTransform()
    {
        return hitTransform;
    }

    public Ray GetRay()
    {
        return ray;
    }

    public Transform GetHitPoint()
    {
        return hitPoint;
    }

    
    #endregion
}
