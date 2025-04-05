using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;

public class WeaponProjectile : WeaponBase
{
    //Bullet
    [SerializeField] public ParticleSystem muzzleFlash;

    //bullet force
    private int bulletsLeft, bulletsShot;

    //Gun stats
    public float timeBetweenShooting, timeBetweenShots;
    public int magazineSize;
    public bool allowButtonHold;

    //bools
    public bool readyToShoot = true, reloading;


    //graphics
    public Camera playerCamera;
    public TextMeshProUGUI ammunitionDisplay;

    //bug fix
    public bool allowInvoke = true;
    public Vector3 lastAimPosition;

    private void Awake()
    {
        //make sure magazine is full
        bulletsLeft = weaponProperties.magazineSize;
        recoil = GetComponentInParent<WeaponRecoil>();
        playerCamera = GetComponentInParent<Camera>();
    }

    private void Update()
    {
        timeBetweenShooting = 1f / weaponProperties.fireRate; //calculate time between shots

        ammunitionDisplay = GameObject.Find("AmmoDisplay").GetComponent<TextMeshProUGUI>(); //find ammo display in scene
        //set ammo display, if it exists
        if(ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(bulletsLeft / weaponProperties.bulletsPerTap + " / " + weaponProperties.magazineSize / weaponProperties.bulletsPerTap); //for shot gun
        }
    }

    public override void StartFiring(Vector3 aimPosition)
    {
        if (reloading || bulletsLeft <= 0) return;

        if (!allowButtonHold && isFiring) return;

        lastAimPosition = aimPosition;
        isFiring = true;
        readyToShoot = false;
        bulletsShot = 0; //reset bullets shot

        FireProjectileBullet(aimPosition);

        // Set cooldown to allow shooting again
        if (allowInvoke)
        {
            Invoke(nameof(ResetShot), timeBetweenShooting);
            allowInvoke = false;
        }
    }


    public override void UpdateFiring(float deltaTime, Vector3 aimPosition, bool isShooting)
    {
        if (reloading || !isFiring || (!allowButtonHold && !isShooting)) return;

        if (!readyToShoot) return;

        if (bulletsLeft <= 0)
        {
            Reload();
            StopFiring();
            return;
        }

        lastAimPosition = aimPosition;
        readyToShoot = false;
        bulletsShot = 0;

        FireProjectileBullet(aimPosition);

        if (allowInvoke)
        {
            Invoke(nameof(ResetShot), timeBetweenShooting);
            allowInvoke = false;
        }
    }

    public override void Initialize()
    {
        ActiveWeapon activeWeapon = GetComponentInParent<ActiveWeapon>();
        if (activeWeapon == null)
        {
            Debug.LogWarning("[WeaponRaycast] ActiveWeapon not found! Is the weapon parented correctly?");
            return;
        }
    }
    private void FireProjectileBullet(Vector3 aimPosition)
    {
        if (bulletSpawnPosition == null || bulletsLeft <= 0) return;

        Vector3 directionWithoutSpread = (aimPosition - bulletSpawnPosition.position).normalized;

        float x = Random.Range(-weaponProperties.spread, weaponProperties.spread);
        float y = Random.Range(-weaponProperties.spread, weaponProperties.spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        GameObject bulletObject = ObjectPooler.SpawnFromPool("Bullet", bulletSpawnPosition.position, Quaternion.LookRotation(directionWithSpread.normalized));
        if (bulletObject != null)
        {
            BulletProjectile bullet = bulletObject.GetComponent<BulletProjectile>();
            bullet.Initialize(
            directionWithSpread,
            weaponProperties.bulletSpeed,
            weaponProperties.upwardForce,
            weaponProperties.bulletLifetime
            );
        }

        recoil.GenerateRecoil(weaponProperties.weaponName.ToString());
        muzzleFlash?.Play();

        bulletsLeft--;
        bulletsShot++;


        if (bulletsShot < weaponProperties.bulletsPerTap && bulletsLeft > 0)
        {
            Invoke(nameof(ShootNextBullet), timeBetweenShots);
        }
    }

    public void ShootNextBullet()
    {
        FireProjectileBullet(lastAimPosition);
    }

    public override void StopFiring()
    {
        isFiring = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    public override void Reload()
    {
        if (reloading) return;

        reloading = true;
        Invoke(nameof(ReloadFinished), weaponProperties.reloadTime);
    }


    private void ReloadFinished()
    {
        bulletsLeft = weaponProperties.magazineSize;
        reloading = false;
    }
}
