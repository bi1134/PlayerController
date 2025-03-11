using System.Collections;
using Unity.Collections;
using UnityEngine;

public class WeaponRaycast : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Transform bulletSpawnPosition;
    [SerializeField] private ParticleSystem[] muzzleFlash;

    [Header("Stats")]
    [SerializeField] private WeaponPropertiesSO weaponProperties;

    [Header("Weapon Info (Read-Only)")]
    [ReadOnly] public string weaponName;
    [ReadOnly] public float damage;
    [ReadOnly] public int fireRate;
    [ReadOnly] public int bulletSpeed;
    [ReadOnly] public WeaponSlot weaponSlot;

    public BulletPropertiesSO bulletProperties;
    public WeaponRecoil recoil;


    //Shooting variables
    public bool isFiring = false;
    private float accumulatedTime;
    private float fireInterval;


    //get components stuff
    private ActiveWeapon activeWeapon;
    private PlayerState playerState;

    #endregion

    #region Startup
    private void Awake()
    {
        recoil = GetComponentInParent<WeaponRecoil>();
    }

    private void Start()
    {
        // Initial setup
        playerState = GetComponentInParent<PlayerState>();
        activeWeapon = GetComponentInParent<ActiveWeapon>();
    }

    private void OnValidate()
    {
        if (weaponProperties != null)
        {
            weaponName = weaponProperties.weaponName;
            damage = weaponProperties.damage;
            fireRate = weaponProperties.fireRate;
            bulletSpeed = weaponProperties.bulletSpeed;
            weaponSlot = weaponProperties.weaponSlot;
        }
    }



    public void Initialize()
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

    public void StartFiring()
    {
        isFiring = true;
        fireInterval = 1.0f / weaponProperties.fireRate;
        accumulatedTime = 0.0f;

        FireBullet();
    }

    public void UpdateFiring(float deltaTime)
    {
        if (!isFiring) 
            return;

        accumulatedTime += deltaTime;
        while(accumulatedTime >= fireInterval)
        {
            
            FireBullet();
            accumulatedTime -= fireInterval;
        }
        
    }

    public void StopFiring()
    {
        isFiring = false;
        accumulatedTime = 0f;
    }

    private void FireBullet()
    {
    
        if (bulletSpawnPosition == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet spawn position is missing!");
            return;
        }
        Vector3 aimDir = (activeWeapon.GetAimPosition() - bulletSpawnPosition.position).normalized;
        Vector3 velocity = aimDir * weaponProperties.bulletSpeed;

        GameObject bulletObject = ObjectPooler.SpawnFromPool("Bullet", bulletSpawnPosition.position, Quaternion.LookRotation(aimDir));
        if (bulletObject == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet pooling failed!");
            return;
        }

        BulletProjectile bullet = bulletObject.GetComponent<BulletProjectile>();
        if (bullet != null)
        {
            bullet.Initialize(bulletSpawnPosition.position, velocity, bulletProperties);
        }
        else
        {
            Debug.LogWarning("[WeaponRaycast] BulletProjectile script is missing!");
        }

        Transform hitTransform = activeWeapon.GetHitTransform();

        if (hitTransform != null)
        {
            bool hitTarget = hitTransform.GetComponent<BulletTarget>() != null;

            Rigidbody rb2d = hitTransform.GetComponent<Rigidbody>();
            if (rb2d != null)
            {
                rb2d.AddForceAtPosition(activeWeapon.GetRay().direction * 20, activeWeapon.GetHitPoint().position, ForceMode.Impulse);
            }

            HitBox hitBox = hitTransform.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.OnRaycastHit(this, activeWeapon.GetRay().direction);
            }
        }
        recoil.GenerateRecoil(weaponName);
        foreach (var particle in muzzleFlash)
        {
            if (!particle.isPlaying)
            {
                particle.Emit(1);
            }
        }
        playerState.SetPlayerCombatState(PlayerCombatState.InCombat);
    }

    private IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
    }

    public float GetWeaponDamage()
    {
        return weaponProperties != null ? weaponProperties.damage : 0f;
    }

    private void UpdateStats()
    {
        weaponName = weaponProperties.weaponName;
        damage = weaponProperties.damage;
        fireRate = weaponProperties.fireRate;
        bulletSpeed = weaponProperties.bulletSpeed;
        weaponSlot = weaponProperties.weaponSlot;
    }

    #endregion



}
