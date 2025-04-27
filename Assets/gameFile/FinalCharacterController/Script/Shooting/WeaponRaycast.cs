using System.Collections;
using System.Xml.Serialization;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class WeaponRaycast : WeaponBase
{
    #region Variables
    [Header("Components")]
    [SerializeField] private ParticleSystem muzzleFlash;

    //bullets
    public int bulletCount;
    private bool allowInvoke = true;

    //Shooting variables
    private float accumulatedTime;
    private float fireInterval;

    //get components stuff
    private ActiveWeapon activeWeapon;
    public TextMeshProUGUI ammunitionDisplay;
    private PlayerHUD playerHUD;

    #endregion

    #region Startup
    private void Awake()
    {
        playerHUD = GetComponentInParent<ActiveWeapon>()?.hud;
        recoil = GetComponent<WeaponRecoil>();
        bulletCount = weaponProperties.magazineSize; //make sure magazine is full
    }

    private void Start()
    {
        // Initial setup
        activeWeapon = GetComponentInParent<ActiveWeapon>();
        if (playerHUD != null)
        {
            playerHUD.UpdateAmmo(bulletCount, weaponProperties.magazineSize, weaponProperties.bulletsPerTap);
        }
    }

    public override void Initialize()
    {
        activeWeapon = GetComponentInParent<ActiveWeapon>();
        if (activeWeapon == null)
        {
            Debug.LogWarning("[WeaponRaycast] ActiveWeapon not found! Is the weapon parented correctly?");
            return;
        }
    }

    #endregion

    #region Update
    private void Update()
    {

    }
    #endregion

    #region Shooting Logic

    public override void StartFiring(Vector3 aimPosition)
    {
        if (weaponProperties == null) return;

        if (reloading || bulletCount <= 0)
        {
            if (!reloading && bulletCount <= 0)
            {
                Reload();
            }
            return;
        }


        isFiring = true;
        fireInterval = 1.0f / weaponProperties.fireRate;
        accumulatedTime = 0.0f;

        FireRaycastBullet(aimPosition);

        if (allowInvoke)
        {
            Invoke(nameof(ResetShot), fireInterval);
            allowInvoke = false;
        }
    }

    public override void UpdateFiring(float deltaTime, Vector3 aimPosition, bool isShooting)
    {
        if (!isFiring) 
            return;

        accumulatedTime += deltaTime;
        while(accumulatedTime >= fireInterval)
        {
            FireRaycastBullet(aimPosition);
            accumulatedTime -= fireInterval;
        }
        
    }
    public override void StopFiring()
    {
        isFiring = false;
        accumulatedTime = 0f;
    }

    private void FireRaycastBullet(Vector3 aimPosition)
    {
        if (bulletSpawnPosition == null) return;

        if (bulletCount <= 0)
            return;

        bulletCount--;
        UpdateAmmoUI();

        Vector3 aimDir = (aimPosition - bulletSpawnPosition.position).normalized;

        // Raycast logic
        if (Physics.Raycast(bulletSpawnPosition.position, aimDir, out RaycastHit hit, Mathf.Infinity))
        {
            // Check if the hit object has a HitBox component
            if (hit.transform.TryGetComponent(out HitBox hitBox))
            {
                hitBox.TakeDamage(this, aimDir); //apply damage
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(aimDir * 20, hit.point, ForceMode.Impulse);
            }
        }

        if(recoil)
        {
            recoil.GenerateRecoil(weaponProperties.weaponName.ToString());
        }    
        muzzleFlash?.Play();
    }

    private IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
    }

    public override void Reload()
    {
        if (reloading || (bulletCount / weaponProperties.bulletsPerTap) >= (weaponProperties.magazineSize / weaponProperties.bulletsPerTap)) return;

        reloading = true;
        isFiring = false;
        Invoke(nameof(ReloadFinished), weaponProperties.reloadTime);
    }

    private void ResetShot()
    {
        allowInvoke = true;
    }

    private void ReloadFinished()
    {
        bulletCount = weaponProperties.magazineSize;
        reloading = false;

        if (this == GetComponentInParent<ActiveWeapon>()?.GetActiveWeapon())
        {
            UpdateAmmoUI();
        }
    }

    public override void UpdateAmmoUI()
    {
        if (playerHUD != null)
        {
            playerHUD.UpdateAmmo(bulletCount, weaponProperties.magazineSize, weaponProperties.bulletsPerTap);
        }
    }

    public override void CancelAllActions()
    {
        CancelInvoke();
        isFiring = false;
        reloading = false;
        allowInvoke = true;
    }
    #endregion
}
