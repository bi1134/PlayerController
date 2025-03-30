using System.Collections;
using Unity.Collections;
using UnityEngine;

public enum WeaponDamageType
{
    Raycast,
    BulletCollision
}


public class WeaponRaycast : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] public Transform bulletSpawnPosition;
    [SerializeField] private ParticleSystem[] muzzleFlash;

    [Header("Stats")]
    [SerializeField] private WeaponPropertiesSO weaponProperties;

    [Header("Weapon Info (Read-Only)")]
    [ReadOnly] public string weaponName;
    [ReadOnly] public float damage;
    [ReadOnly] public int fireRate;
    [ReadOnly] public int bulletSpeed;
    [ReadOnly] public WeaponSlot weaponSlot;
    [ReadOnly] public WeaponDamageType damageType;

    public BulletPropertiesSO bulletProperties;
    public WeaponRecoil recoil;

    //Shooting variables
    public bool isFiring = false;
    private float accumulatedTime;
    private float fireInterval;


    //weapon raycasting
    private Ray ray;
    private RaycastHit hit;
    public LayerMask aimMask;


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
            damageType = weaponProperties.damageType;
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

    public void StartFiring(Vector3 target)
    {
        isFiring = true;
        fireInterval = 1.0f / weaponProperties.fireRate;
        accumulatedTime = 0.0f;

        FireBullet(target);
    }

    public void UpdateFiring(float deltaTime, Vector3 target)
    {
        if (!isFiring) 
            return;

        accumulatedTime += deltaTime;
        while(accumulatedTime >= fireInterval)
        {
            
            FireBullet(target);
            accumulatedTime -= fireInterval;
        }
        
    }

    public void StopFiring()
    {
        isFiring = false;
        accumulatedTime = 0f;
    }

    private void FireBullet(Vector3 target)
    {
        if (bulletSpawnPosition == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet spawn position is missing!");
            return;
        }

        Vector3 aimDir = (target - bulletSpawnPosition.position).normalized;
        Vector3 velocity = aimDir * weaponProperties.bulletSpeed;
        Vector3 spawnPosition = bulletSpawnPosition.position + aimDir * 0.1f;

        GameObject bulletObject = ObjectPooler.SpawnFromPool("Bullet", spawnPosition, Quaternion.LookRotation(aimDir));
        if (bulletObject == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet pooling failed!");
            return;
        }

        BulletProjectile bullet = bulletObject.GetComponent<BulletProjectile>();
        if (bullet != null)
        {
            bullet.Initialize(bulletSpawnPosition.position, velocity, bulletProperties);

            bullet.SetDamageInfo(weaponProperties.damage, weaponProperties.damageType);
        }


        // AI and Player handle hit detection differently
        if (weaponProperties.damageType == WeaponDamageType.Raycast)
        {
            if (Physics.Raycast(bulletSpawnPosition.position, aimDir, out hit, 999f,aimMask))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForceAtPosition(aimDir * 20, hit.point, ForceMode.Impulse);
                }
                HitBox hitBox = hit.collider.GetComponent<HitBox>();
                if (hitBox != null)
                {
                    hitBox.OnRaycastHit(this, aimDir);
                }
            }
        }

        if (recoil && recoil.rigController != null)
        {
            recoil.GenerateRecoil(weaponName);
        }

        //AI shouldn't use player state (Fix)
        if (playerState != null)
        {
            playerState.SetPlayerCombatState(PlayerCombatState.InCombat);
        }
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
