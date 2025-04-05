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

    //Shooting variables
    private float accumulatedTime;
    private float fireInterval;

    //get components stuff
    private ActiveWeapon activeWeapon;
    public TextMeshProUGUI ammunitionDisplay;

    #endregion

    #region Startup
    private void Awake()
    {
        recoil = GetComponent<WeaponRecoil>();
        bulletCount = weaponProperties.magazineSize; //make sure magazine is full
    }

    private void Start()
    {
        // Initial setup
        activeWeapon = GetComponentInParent<ActiveWeapon>();
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
        ammunitionDisplay = GameObject.Find("AmmoDisplay").GetComponent<TextMeshProUGUI>(); //find ammo display in scene
        //set ammo display, if it exists
        if (ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(bulletCount / weaponProperties.bulletsPerTap + " / " + weaponProperties.magazineSize / weaponProperties.bulletsPerTap); //for shot gun
        }
    }
    #endregion

    #region Shooting Logic

    public override void StartFiring(Vector3 aimPosition)
    {
        if (weaponProperties == null) return;

        isFiring = true;
        fireInterval = 1.0f / weaponProperties.fireRate;
        accumulatedTime = 0.0f;

        FireRaycastBullet(aimPosition);
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

        recoil.GenerateRecoil(weaponProperties.weaponName.ToString());
        muzzleFlash?.Play();
    }

    private IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
    }

    public override void Reload()
    {
        bulletCount = weaponProperties.magazineSize;
    }
    #endregion
}
