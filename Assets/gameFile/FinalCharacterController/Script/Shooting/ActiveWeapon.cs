using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ActiveWeapon : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] private Rig handIK;

    private PlayerActionInput playerActionInput;
    private WeaponRaycast weapon;

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
        }
        else
        {
            handIK.weight = 0.0f;
        }
    }
    #endregion

    #region Func
    public void EquipWeapon(WeaponRaycast newWeapon)
    {
        weapon = newWeapon;
        handIK.weight = 1.0f;
    }

    #endregion
}
