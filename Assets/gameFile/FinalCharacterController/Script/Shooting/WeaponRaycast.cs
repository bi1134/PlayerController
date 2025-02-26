using UnityEngine;
using UnityEngine.Playables;

public class WeaponRaycast : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] public string weaponName;
    [SerializeField] private Transform bulletSpawnPosition;

    [Header("Stats")]
    public float damage = 10f;
    public int fireRate = 25;


    //Shooting variables
    public bool isFiring = false;
    private float accumulatedTime;
    private float fireInterval;
    public WeaponSlot weaponSlot;


    //get components stuff
    private ActiveWeapon activeWeapon;
    private PlayerState playerState;

    #endregion

    #region Startup
    private void Start()
    {
        // Initial setup
        playerState = GetComponentInParent<PlayerState>();
        activeWeapon = GetComponentInParent<ActiveWeapon>();
        fireInterval = 1.0f / fireRate;
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
        if (bulletSpawnPosition == null || pfBulletProjectile == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet spawn position or projectile prefab is missing!");
            return;
        }

        Vector3 aimDir = (activeWeapon.GetAimPosition() - bulletSpawnPosition.position).normalized;
        Transform bulletTransform = Instantiate(pfBulletProjectile, bulletSpawnPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

        if (bulletTransform == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet instantiation failed!");
            return;
        }

        BulletProjectile bulletProjectile = bulletTransform.GetComponent<BulletProjectile>();
        if (bulletProjectile != null)
        {
            bulletProjectile.SetTarget(activeWeapon.GetHitPoint().position);
        }
        else
        {
            Debug.LogWarning("[WeaponRaycast] BulletProjectile script is missing!");
        }

        Transform hitTransform = activeWeapon.GetHitTransform();

        if (hitTransform != null)
        {
            bool hitTarget = hitTransform.GetComponent<BulletTarget>() != null;
            bulletProjectile?.HandleHit(activeWeapon.GetHitPoint().position, hitTarget);

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

        playerState.SetPlayerCombatState(PlayerCombatState.InCombat);
    }
    #endregion
}
