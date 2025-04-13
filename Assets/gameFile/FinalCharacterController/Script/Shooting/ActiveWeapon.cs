using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ActiveWeapon : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] public Transform[] weaponSlots;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private LayerMask aimColliderMask;
    [SerializeField] private Rig handIK;
    [SerializeField] private Camera playerCamera;

    [Header("Rigs")]
    [SerializeField] private Transform leftGrip;
    [SerializeField] private Transform rightGrip;
    [SerializeField] private Animator rigController;

    //references
    private PlayerActionInput playerActionInput;
    private WeaponBase[] equippedWeapon = new WeaponBase[2];
    private int activeWeaponIndex;
    private InventoryHolder inventoryHolder;

    //aiming
    public float lerpRotationSpeed = 10f;
    public bool canAim = true;
    private Vector3 mouseWorldPosition;
    private Ray ray;
    private RaycastHit hitRaycast;

    //holster Weapon
    public bool isHolstered;
    private bool isSwitching = false;


    #endregion

    #region Start
    private void Start()
    {
        playerActionInput = GetComponent<PlayerActionInput>();
        inventoryHolder = GetComponent<InventoryHolder>();

        //check if the player already has a weapon and equip it
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
        HandleWeaponActions();
    }
    #endregion

    #region Weapon Handling
    private void HandleWeaponActions()
    {
        WeaponBase weapon = GetWeaponIndex(activeWeaponIndex);

        //if having weapon and NOT holstered (Im not putting the !before isholstered because its working mamamia)
        if (weapon && isHolstered)
        {
            if (playerActionInput.attackPressed)
            {
                if (!weapon.isFiring) // Start firing only once
                {
                    weapon.StartFiring(mouseWorldPosition);
                }
                weapon.UpdateFiring(Time.deltaTime, mouseWorldPosition, playerActionInput.attackPressed);
            }
            else // Stop firing when button is released
            {
                weapon.StopFiring();
            }
        }
        if (playerActionInput.holsterPressed)
        {
            ToggleActiveWeapon();
        }

        if (playerActionInput.reloadPressed)
        {
            if (weapon)
            {
                weapon.Reload();
            }
        }

        if (playerActionInput.weaponButton == 1)
        {
            if (inventoryHolder.weaponSlot1)
            {
                isHolstered = true;
                EquipWeaponFromInventory(inventoryHolder.weaponSlot1);
            }
        }
        else if (playerActionInput.weaponButton == 2)
        {
            if (inventoryHolder.weaponSlot2)
            {
                isHolstered = true;
                EquipWeaponFromInventory(inventoryHolder.weaponSlot2);
            }
        }
    }
    public void EquipWeaponFromInventory(InventoryItemData weaponData)
    {
        if (weaponData == null || weaponData.ItemType != ItemType.Weapon)
            return; // Not a valid weapon

        WeaponBase weaponPrefab = weaponData.Prefab.GetComponent<WeaponBase>();
        if (weaponPrefab == null)
        {
            Debug.LogError($"Weapon prefab for {weaponData} is missing WeaponRaycast component!");
            return;
        }

        int weaponSlotIndex = (weaponPrefab.weaponSlot == WeaponSlot.Primary) ? 0 :
                                (weaponPrefab.weaponSlot == WeaponSlot.Secondary) ? 1 : -1;

        if (weaponSlotIndex == -1 || weaponSlotIndex >= weaponSlots.Length)
        {
            Debug.LogError("Invalid weapon slot index: " + weaponSlotIndex);
            return;
        }

        // Check if weapon is already equipped
        if (equippedWeapon[weaponSlotIndex] != null)
        {
            SetActiveWeapon((WeaponSlot)weaponSlotIndex);
            equippedWeapon[weaponSlotIndex].UpdateAmmoUI();
            return;
        }

        // Instantiate only if weapon is not already stored
        WeaponBase newWeapon = Instantiate(weaponData.Prefab, weaponSlots[weaponSlotIndex]).GetComponent<WeaponBase>();
        equippedWeapon[weaponSlotIndex] = newWeapon;

        newWeapon.weaponSlot = weaponPrefab.weaponSlot;

        // Assign references
        newWeapon.recoil.cameraTransform = playerCamera.transform;
        newWeapon.recoil.rigController = rigController;
        isHolstered = true;

        // Set active weapon
        SetActiveWeapon((WeaponSlot)weaponSlotIndex);
        newWeapon.Initialize();
    }

    public void EquipWeapon(WeaponBase newWeapon)
    {
        int weaponSlotIndex = (int)newWeapon.weaponSlot;
        var weapon = GetWeaponIndex(weaponSlotIndex);

        //if a weapon is already equipped in this slot, destroy it
        if (weapon)
        {
            //destroy the game object not component
            Destroy(weapon.gameObject);
        }

        //equip the new weapon
        weapon = newWeapon;
        weapon.recoil.cameraTransform = playerCamera.transform;
        weapon.recoil.rigController = rigController;
        weapon.transform.SetParent(weaponSlots[weaponSlotIndex], false);
        equippedWeapon[weaponSlotIndex] = weapon;

        //start the equip animation sequence
        SetActiveWeapon(newWeapon.weaponSlot);
        weapon.Initialize();
        isHolstered = true;

    }

    private void ToggleActiveWeapon()
    {
        if (equippedWeapon[activeWeaponIndex] == null) return;

        //get current holster state
        isHolstered = rigController.GetBool("isHolster");

        //toggle holster state and play corresponding animation
        if (isHolstered)
        {
            StartCoroutine(ActivateWeapon(activeWeaponIndex));
        }
        else
        {
            StartCoroutine(HolsterWeapon(activeWeaponIndex));
        }
    }

    private void SetActiveWeapon(WeaponSlot weaponSlot)
    {
        if (isSwitching)
            return;
        isSwitching = true;
        int holsterIndex = activeWeaponIndex;
        int activateIndex = (int)weaponSlot;

        if (holsterIndex == activateIndex)
        {
            holsterIndex = -1;
        }

        //switch animation using coroutines for smooth animations
        StartCoroutine(SwitchWeapon(holsterIndex, activateIndex));
    }

    private IEnumerator SwitchWeapon(int holsterIndex, int activateIndex)
    {
        //wait for holster animation to complete
        yield return StartCoroutine(HolsterWeapon(holsterIndex));

        //then wait for equip animation to complete
        yield return StartCoroutine(ActivateWeapon(activateIndex));

        //update active weapon index
        activeWeaponIndex = activateIndex;
        isSwitching = false;
    }

    private IEnumerator HolsterWeapon(int index)
    {
        var weapon = GetWeaponIndex(index);
        if (weapon)
        {
            weapon.CancelAllActions();
            //set holster state to true and play the animation
            rigController.SetBool("isHolster", true);

            //wait for holster animation to finish 
            do
            {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }
    }

    private IEnumerator ActivateWeapon(int index)
    {
        var weapon = GetWeaponIndex(index);
        if (weapon)
        {
            if (playerActionInput.attackPressed)
            {
                weapon.StartFiring(mouseWorldPosition);
            }
            //set holster state to false and play the animation
            rigController.SetBool("isHolster", false);

            //wait for equip animation to finish
            rigController.Play("Equip" + weapon.weaponProperties.weaponName);
            do
            {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }
    }

    public WeaponBase GetActiveWeapon()
    {
        return GetWeaponIndex(activeWeaponIndex);
    }

    private WeaponBase GetWeaponIndex(int index)
    {
        if (index < 0 || index >= equippedWeapon.Length)
            return null;

        return equippedWeapon[index];
    }

    public void DropWeapon()
    {
        var currentWeapon = GetActiveWeapon();
        if (currentWeapon)
        {
            currentWeapon.transform.SetParent(null);
            currentWeapon.gameObject.GetComponent<BoxCollider>().enabled = true;
            currentWeapon.gameObject.AddComponent<Rigidbody>();
            equippedWeapon[activeWeaponIndex] = null;
        }
    }

    #endregion

    #region Aiming
    private void HandleAiming()
    {
        if (!canAim || Camera.main == null) return;

        // Get the center of the screen as the aiming direction
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
            hitRaycast = raycastHit;
            hitPoint.position = raycastHit.point;
            hitPoint.forward = raycastHit.normal;
        }

        // Smoothly move the aim point to follow the hit point
        aimTarget.position = Vector3.Lerp(aimTarget.position, hitPoint.position, Time.deltaTime * lerpRotationSpeed);
    }

    public void DisableAiming()
    {
        canAim = false;
    }
    #endregion
}

public enum WeaponSlot
{
    Primary,
    Secondary
}