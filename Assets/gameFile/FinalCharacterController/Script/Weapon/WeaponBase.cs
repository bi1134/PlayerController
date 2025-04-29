using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponPropertiesSO weaponProperties;
    public WeaponSlot weaponSlot;
    public Transform bulletSpawnPosition;
    public WeaponRecoil recoil;
    public RuntimeAnimatorController animator;
    public bool reloading = false;
    public event Action OnReloadStarted;

    [field: SerializeField] public bool isFiring { get; protected set; }

    // Each weapon will have its own firing logic
    public abstract void StartFiring(Vector3 aimPosition);
    public abstract void StopFiring();
    public abstract void UpdateFiring(float deltaTime, Vector3 aimPosition, bool isShooting);

    public abstract void Initialize();

    public virtual void Reload()
    {
        reloading = true;
        OnReloadStarted?.Invoke();
    }

    public virtual bool IsAmmoEmpty()
    {
        return false; // default
    }

    // Optional: Can be overridden for different attack types
    public virtual void Attack() { }

    public virtual void UpdateAmmoUI() { }

    public virtual void CancelAllActions() { }

    public virtual float GetWeaponDamage() => weaponProperties != null ? weaponProperties.damage : 0f;
}
