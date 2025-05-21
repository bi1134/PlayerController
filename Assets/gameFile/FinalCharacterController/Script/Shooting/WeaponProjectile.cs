using TMPro;
using UnityEngine;

public class WeaponProjectile : WeaponBase
{
    //reference
    private PlayerHUD playerHUD;

    //Bullet
    [SerializeField] public ParticleSystem muzzleFlash;
    [SerializeField] private BulletPropertiesSO bulletProperties;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    //bullet force
    private int bulletsLeft, bulletsShot;

    //Gun stats
    public float timeBetweenShooting, timeBetweenShots;
    public bool allowButtonHold;

    //bools
    public bool readyToShoot = true;

    //floats
    private float fireCooldown;


    //graphics
    public Camera playerCamera;
    public TextMeshProUGUI ammunitionDisplay;

    //bug fix
    public bool allowInvoke = true;
    private Vector3 lastAimPosition;

    #region Startup 
    private void Awake()
    {
        //make sure magazine is full
        bulletsLeft = weaponProperties.magazineSize;
        recoil = GetComponentInParent<WeaponRecoil>();
        playerCamera = GetComponentInParent<Camera>();
        playerHUD = GetComponentInParent<ActiveWeapon>()?.hud;
    }

    private void Start()
    {
        if (playerHUD != null)
        {
            playerHUD.UpdateAmmo(bulletsLeft, weaponProperties.magazineSize, weaponProperties.bulletsPerTap);
        }
    }

    #endregion

    #region Update
    private void Update()
    {
        timeBetweenShooting = 1f / weaponProperties.fireRate; //calculate time between shots


        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    #endregion


    #region Shooting Logic
    public override void StartFiring(Vector3 aimPosition)
    {
        if (bulletsLeft <= 0)
        {
            if (!reloading)
            {
                Reload();
            }
            return;
        }

        if (reloading) return;

        // NEW: For button hold, use UpdateFiring instead
        if (allowButtonHold)
        {
            isFiring = true;
            return;
        }

        // for semi-auto or burst weapons
        if (!readyToShoot) return;

        lastAimPosition = aimPosition;
        isFiring = true;
        readyToShoot = false;
        fireCooldown = timeBetweenShooting;
        bulletsShot = 0;
        FireProjectileBullet(aimPosition);
        Invoke(nameof(ResetShot), timeBetweenShooting);
    }

    public override void UpdateFiring(float deltaTime, Vector3 aimPosition, bool isShooting)
    {
        if (!isFiring || reloading || fireCooldown > 0f || !readyToShoot) return;

        if (!allowButtonHold || !isShooting)
        {
            StopFiring();
            return;
        }

        if (bulletsLeft <= 0)
        {
            Reload();
            StopFiring();
            return;
        }

        lastAimPosition = aimPosition;
        bulletsShot = 0;
        readyToShoot = false;

        FireProjectileBullet(aimPosition);
        fireCooldown = timeBetweenShooting;
        Invoke(nameof(ResetShot), timeBetweenShooting);
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

        string bulletTag = bulletProperties.bulletPoolTag;

        GameObject bulletObject = ObjectPooler.SpawnFromPool(bulletTag, bulletSpawnPosition.position, Quaternion.LookRotation(directionWithSpread.normalized));
        if (bulletObject != null)
        {
            PlayShootSound();
            BulletProjectile bullet = bulletObject.GetComponent<BulletProjectile>();
            bullet.settings = bulletProperties;
            bullet.Initialize(
            directionWithSpread,
            weaponProperties.bulletSpeed,
            weaponProperties.upwardForce,
            weaponProperties.bulletLifetime
            );
            bullet.SetShooter(this.gameObject);
        }

        if (recoil)
        { 
            recoil.GenerateRecoil(weaponProperties.weaponName.ToString());
        }
        
        muzzleFlash?.Play();

        bulletsLeft--;
        bulletsShot++;
        UpdateAmmoUI();


        if (bulletsShot < weaponProperties.bulletsPerTap && bulletsLeft > 0)
        {
            Invoke(nameof(ShootNextBullet), timeBetweenShots);
        }
    }

    private void PlayShootSound()
    {
        if (shootClip == null || audioSource == null) return;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(shootClip);
    }

    private void PlayReloadSound()
    {
        if (reloadClip == null || audioSource == null) return;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(reloadClip);
    }

    public void ShootNextBullet()
    {
        FireProjectileBullet(lastAimPosition);
    }

    public override void StopFiring()
    {
        isFiring = false;
        readyToShoot = true;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    public override void Reload()
    {
        if (reloading || (bulletsLeft / weaponProperties.bulletsPerTap) >= (weaponProperties.magazineSize / weaponProperties.bulletsPerTap)) return;
        PlayReloadSound();
        base.Reload();

        Invoke(nameof(ReloadFinished), weaponProperties.reloadTime);
    }

    public override bool IsAmmoEmpty()
    {
        return bulletsLeft <= 0;
    }

    private void ReloadFinished()
    {
        bulletsLeft = weaponProperties.magazineSize;
        reloading = false;
        if (this == GetComponentInParent<ActiveWeapon>()?.GetActiveWeapon())
        {
            UpdateAmmoUI();
        }
    }

    #endregion

    #region Other Func
    public override void UpdateAmmoUI()
    {
        if (playerHUD != null)
        {
            playerHUD.UpdateAmmo(bulletsLeft, weaponProperties.magazineSize, weaponProperties.bulletsPerTap);
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
