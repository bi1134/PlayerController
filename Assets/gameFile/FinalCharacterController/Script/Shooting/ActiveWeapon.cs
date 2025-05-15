using System.Collections;
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
    [SerializeField] public PlayerHUD hud;
    [SerializeField] private GameObject itemPickupPrefab;

    [Header("Rigs")]
    [SerializeField] private Transform leftGrip;
    [SerializeField] private Transform rightGrip;
    [SerializeField] private Animator rigController;

    [Header("Melee")]
    public bool hasWeaponEquipped;
    public bool canMeleeAttack = true;
    public Collider meleeHitbox;
    private float meleeCooldown = 1f;
    private float meleeCooldownTimer = 0f;

    //references
    private PlayerActionInput playerActionInput;
    private WeaponBase[] equippedWeapon = new WeaponBase[4];
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
    private bool isDropping = false;

    #endregion

    #region Start
    private void Start()
    {
        playerActionInput = GetComponent<PlayerActionInput>();
        inventoryHolder = GetComponent<InventoryHolder>();

        SyncEquippedWeaponsWithInventory();
    }
    #endregion

    #region Update
    private void Update()
    {
        HandleAiming();
        HandleWeaponActions();

        if (meleeCooldownTimer > 0f)
            meleeCooldownTimer -= Time.deltaTime;
    }
    #endregion

    #region Weapon Handling
    private void HandleWeaponActions()
    {
        WeaponBase weapon = GetWeaponIndex(activeWeaponIndex);

        bool wantsMelee =
        (playerActionInput.meleeAttackPressed && hasWeaponEquipped) ||
        (playerActionInput.attackPressed && (!hasWeaponEquipped || !isHolstered));

        if (wantsMelee && meleeCooldownTimer <= 0f)
        {
            MeleeAttack();
            meleeCooldownTimer = meleeCooldown;
            return; // skip other input this frame
        }

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

        if (weapon)
        {
            if (playerActionInput.reloadPressed && !weapon.reloading)
            {
                weapon.Reload();
            }
        }

        if (!isDropping && playerActionInput.weaponButton == 1 && inventoryHolder.weaponSlot1 != null)
        {
            isHolstered = true;
            EquipWeaponFromInventory(inventoryHolder.weaponSlot1);
        }
        else if (!isDropping && playerActionInput.weaponButton == 2 && inventoryHolder.weaponSlot2 != null)
        {
            isHolstered = true;
            EquipWeaponFromInventory(inventoryHolder.weaponSlot2);
        }
        if (playerActionInput.canDropWeapon)
        {
            DropEquippedWeapon();
        }
    }
    public void EquipWeaponFromInventory(InventoryItemData weaponData)
    {
        if (weaponData == null || weaponData.ItemType != ItemType.Weapon)
            return;

        WeaponBase weaponPrefab = weaponData.Prefab.GetComponent<WeaponBase>();
        if (weaponPrefab == null)
        {
            Debug.LogError($"Weapon prefab for {weaponData} is missing WeaponRaycast component!");
            return;
        }

        // Check if weapon is already equipped in any slot
        for (int i = 0; i < equippedWeapon.Length; i++)
        {
            if (equippedWeapon[i] != null && equippedWeapon[i].inventoryData.ID == weaponData.ID)
            {
                SetActiveWeapon((WeaponSlot)i);
                equippedWeapon[i].UpdateAmmoUI();
                hasWeaponEquipped = true;
                return;
            }
        }
        // Not equipped, find a free slot based on weapon type
        int weaponSlotIndex = GetCorrectWeaponIndex(weaponData);

        if (weaponSlotIndex == -1)
        {
            Debug.LogError("No available weapon slot");
            return;
        }

        if (weaponSlotIndex >= weaponSlots.Length)
        {
            Debug.LogError("Invalid weapon slot index: " + weaponSlotIndex);
            return;
        }
        // Instantiate and equip new weapon

        WeaponBase newWeapon = Instantiate(weaponData.Prefab, weaponSlots[weaponSlotIndex]).GetComponent<WeaponBase>();
        equippedWeapon[weaponSlotIndex] = newWeapon;

        newWeapon.weaponSlot = weaponPrefab.weaponSlot;
        newWeapon.inventoryData = weaponData;
        newWeapon.recoil.cameraTransform = playerCamera.transform;
        newWeapon.recoil.rigController = rigController;

        isHolstered = true;

        SetActiveWeapon((WeaponSlot)weaponSlotIndex);
        newWeapon.Initialize();
        SubscribeReloadEvents(newWeapon);
    }

    private void MeleeAttack()
    {
        if (!hasWeaponEquipped)
        {
            rigController.SetTrigger("meleeAttack");
        }
        else
        {
            // Directly play a specific animation clip
            WeaponBase weapon = GetActiveWeapon();
            if (weapon != null)
            {
                string slotNumber = (weapon.inventoryData.ID == inventoryHolder.weaponSlot1.ID) ? "1" : "2";
                string animName = "PunchAttackWith" + weapon.weaponProperties.weaponName + slotNumber;

                rigController.Play(animName); 
            }
        }
    }

    public void EnableMeleeHitbox()
    {
        if (canMeleeAttack)
        {
            meleeHitbox.enabled = true;
            PoolRunner.Instance.RunCoroutine(DisableMeleeHitbox());
        }
    }

    public IEnumerator DisableMeleeHitbox()
    {
        yield return Helpers.GetWaitForSecond(0.1f);
            
        meleeHitbox.enabled = false;
    }

    public void EquipWeapon(WeaponBase newWeapon)
    {
        int weaponSlotIndex = (int)newWeapon.weaponSlot;
        UnsubscribeReloadEvents(equippedWeapon[weaponSlotIndex]);
        var weapon = GetWeaponIndex(weaponSlotIndex);
        SubscribeReloadEvents(weapon);

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
        hasWeaponEquipped = true;
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
            rigController.SetBool("isHolster", true);
            hasWeaponEquipped = false;

            SpeedUpCurrentAnimation(0.25f); // e.g. compress animation to 0.25s
            StartCoroutine(WaitAndResetSpeed(0.25f));

            do
            {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }
    }

    private IEnumerator ActivateWeapon(int index)
    {
        var weapon = GetWeaponIndex(index);
        if (weapon != null)
        {
            weapon.StopFiring();

            yield return null;


            string slotNumber = "1";
            if (inventoryHolder.weaponSlot1 != null && weapon.inventoryData.ID == inventoryHolder.weaponSlot1.ID)
                slotNumber = "1";
            else if (inventoryHolder.weaponSlot2 != null && weapon.inventoryData.ID == inventoryHolder.weaponSlot2.ID)
                slotNumber = "2";

            rigController.SetBool("isHolster", false);
            hasWeaponEquipped = true;

            rigController.Play("Equip" + weapon.weaponProperties.weaponName + slotNumber);

            do
            {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);

        }
        else
        {
            Debug.LogWarning("Weapon is null in ActivateWeapon()");
        }
    }

    private void SubscribeReloadEvents(WeaponBase weapon)
    {
        if (weapon == null) return;

        weapon.OnReloadStarted += HandleReloadStart;
    }

    private void UnsubscribeReloadEvents(WeaponBase weapon)
    {
        if (weapon == null) return;

        weapon.OnReloadStarted -= HandleReloadStart;
    }

    private void HandleReloadStart()
    {
        rigController.SetTrigger("isReloading");
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
        for (int i = 0; i < equippedWeapon.Length; i++)
        {
            var weapon = equippedWeapon[i];
            if (weapon != null)
            {
                UnsubscribeReloadEvents(weapon);
                weapon.transform.SetParent(null);

                var collider = weapon.gameObject.GetComponent<BoxCollider>();
                if (collider != null)
                    collider.enabled = true;

                if (!weapon.gameObject.TryGetComponent<Rigidbody>(out _))
                    weapon.gameObject.AddComponent<Rigidbody>();

                InventoryItemData droppedItem = weapon.inventoryData;

                // Remove from holder and UI inventory
                if (inventoryHolder.weaponSlot1 != null && inventoryHolder.weaponSlot1.ID == droppedItem.ID)
                {
                    inventoryHolder.weaponInventory.RemoveToInventory(droppedItem, 1);
                    inventoryHolder.weaponSlot1 = null;
                }
                else if (inventoryHolder.weaponSlot2 != null && inventoryHolder.weaponSlot2.ID == droppedItem.ID)
                {
                    inventoryHolder.weaponInventory.RemoveToInventory(droppedItem, 1);
                    inventoryHolder.weaponSlot2 = null;
                }

                equippedWeapon[i] = null;
                hasWeaponEquipped = false;
            }
        }

        rigController.Play("CharacterUnarmed");
        InventoryHolder.OnWeaponInventoryChanged?.Invoke(inventoryHolder.weaponInventory);
    }

    int GetCorrectWeaponIndex(InventoryItemData weaponData)
    {
        if (weaponData == inventoryHolder.weaponSlot1)
        {
            return weaponData.Prefab.GetComponent<WeaponBase>().weaponSlot == WeaponSlot.Primary ? 0 : 1;
        }
        else if (weaponData == inventoryHolder.weaponSlot2)
        {
            return weaponData.Prefab.GetComponent<WeaponBase>().weaponSlot == WeaponSlot.Primary ? 2 : 3;
        }

        Debug.LogWarning("Weapon not found in inventory slots!");
        return -1;
    }


    public void SyncEquippedWeaponsWithInventory()
    {
        // Ensure only weapons in inventory are equipped
        TryEquipFromInventory(inventoryHolder.weaponSlot1);
        TryEquipFromInventory(inventoryHolder.weaponSlot2);

        // Optional: remove weapons not present in inventory
        for (int i = 0; i < equippedWeapon.Length; i++)
        {
            if (equippedWeapon[i] != null)
            {
                var id = equippedWeapon[i].inventoryData.ID;
                if ((inventoryHolder.weaponSlot1 == null || inventoryHolder.weaponSlot1.ID != id) &&
                    (inventoryHolder.weaponSlot2 == null || inventoryHolder.weaponSlot2.ID != id))
                {
                    Destroy(equippedWeapon[i].gameObject);
                    equippedWeapon[i] = null;
                }
            }
        }
    }

    private void TryEquipFromInventory(InventoryItemData data)
    {
        if (data == null || data.ItemType != ItemType.Weapon)
            return;
        hasWeaponEquipped = true;
        // Already equipped?
        for (int i = 0; i < equippedWeapon.Length; i++)
        {
            if (equippedWeapon[i] != null && equippedWeapon[i].inventoryData.ID == data.ID)
                return;
        }

        // Equip if not already equipped
        EquipWeaponFromInventory(data);
    }

    public void DropEquippedWeapon()
    {
        if (isDropping || GetActiveWeapon() == null) return;

        InventoryHolder.OnWeaponInventoryChanged?.Invoke(inventoryHolder.weaponInventory);

        isDropping = true;

        WeaponBase currentWeapon = GetActiveWeapon();
        if (currentWeapon == null) return;

        int index = activeWeaponIndex;

        UnsubscribeReloadEvents(currentWeapon);
        currentWeapon.CancelAllActions();
        currentWeapon.transform.SetParent(null);

        var rb = currentWeapon.gameObject.GetComponent<Rigidbody>();
        if (rb == null) rb = currentWeapon.gameObject.AddComponent<Rigidbody>();

        var collider = currentWeapon.gameObject.GetComponent<Collider>();
        if (collider == null) collider = currentWeapon.gameObject.AddComponent<BoxCollider>();
        collider.enabled = true;

        // Remove from InventoryHolder
        var droppedItem = currentWeapon.inventoryData;
        if (inventoryHolder.weaponSlot1 != null && inventoryHolder.weaponSlot1.ID == droppedItem.ID)
        {
            inventoryHolder.weaponInventory.RemoveToInventory(droppedItem, 1);
            inventoryHolder.weaponSlot1 = null;
        }
        else if (inventoryHolder.weaponSlot2 != null && inventoryHolder.weaponSlot2.ID == droppedItem.ID)
        {
            inventoryHolder.weaponInventory.RemoveToInventory(droppedItem, 1);
            inventoryHolder.weaponSlot2 = null;
        }

        equippedWeapon[index] = null;
        isHolstered = false;
        // Play unarmed animation
        rigController.Play("CharacterUnarmed");
        hasWeaponEquipped = false;

        InventoryHolder.OnWeaponInventoryChanged?.Invoke(inventoryHolder.weaponInventory);
        PoolRunner.Instance.RunCoroutine(DelayedDropReset());


        // Convert into pickup
        StartCoroutine(DelayedPickupSpawn(currentWeapon.gameObject, droppedItem));
    }

    private IEnumerator DelayedDropReset()
    { 
        yield return Helpers.GetWaitForSecond(0.5f);
        isDropping = false;
    }

    private IEnumerator DelayedPickupSpawn(GameObject weaponGO, InventoryItemData itemData)
    {
        yield return Helpers.GetWaitForSecond(2.5f);

        var rb = weaponGO.gameObject.GetComponent<Rigidbody>();
        if (rb == null) rb = weaponGO.gameObject.AddComponent<Rigidbody>();

        var collider = weaponGO.gameObject.GetComponent<Collider>();
        if (collider == null) collider = weaponGO.gameObject.AddComponent<BoxCollider>();
        collider.enabled = false;

        // Spawn ItemPickup in the same position
        Vector3 dropPosition = weaponGO.transform.position;
        Quaternion dropRotation = weaponGO.transform.rotation;

        Destroy(weaponGO); // Clean up weapon

        GameObject pickup = Instantiate(itemPickupPrefab, dropPosition, dropRotation);

        if (pickup == null)
        {
            Debug.LogError("ItemPickup prefab not found in Resources!");
            yield break;
        }

        var pickupComponent = pickup.GetComponent<ItemPickup>();
        if (pickupComponent != null)
        {
            pickupComponent.SetItemData(itemData);
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

    private void SpeedUpCurrentAnimation(float targetDuration)
    {
        AnimatorStateInfo state = rigController.GetCurrentAnimatorStateInfo(0);
        AnimationClip clip = GetCurrentAnimationClip(state);

        if (clip != null)
        {
            float originalDuration = clip.length;
            float newSpeed = originalDuration / targetDuration;
            rigController.speed = newSpeed;
        }
        else
        {
            Debug.LogWarning("Failed to retrieve current animation clip.");
        }
    }

    private IEnumerator WaitAndResetSpeed(float duration)
    {
        yield return Helpers.GetWaitForSecond(duration);
        rigController.speed = 1f; // Reset to normal speed
    }

    private AnimationClip GetCurrentAnimationClip(AnimatorStateInfo state)
    {
        RuntimeAnimatorController controller = rigController.runtimeAnimatorController;

        foreach (var clip in controller.animationClips)
        {
            if (state.IsName(clip.name))
            {
                return clip;
            }
        }

        return null;
    }
    #endregion
}

public enum WeaponSlot
{
    Primary,
    Secondary
}