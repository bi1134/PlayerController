using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Splines;

public class WeaponRaycast : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] public string weaponName;
    [SerializeField] private Transform bulletSpawnPosition;
    [SerializeField] private ParticleSystem[] muzzleFlash;

    [Header("Stats")]
    public float damage = 10f;
    public int fireRate = 25;
    public int bulletSpeed = 100;
    public BulletPropertiesSO bulletProperties;


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
        foreach(var particle in muzzleFlash)
        {
            particle.Emit(1);
        }
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
        Vector3 velocity = aimDir * bulletSpeed;

        GameObject bulletObject = ObjectPooler.SpawnFromPool("Bullet", bulletSpawnPosition.position, Quaternion.LookRotation(aimDir));
        if (bulletObject == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet pooling failed!");
            return;
        }

        BulletProjectile bullet = ObjectPooler.SpawnFromPool("Bullet", bulletSpawnPosition.position, Quaternion.identity).GetComponent<BulletProjectile>();
        if (bullet != null)
        {
            bullet.Initialize(bulletSpawnPosition.position, bulletSpawnPosition.forward * bulletSpeed, bulletProperties);
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

        playerState.SetPlayerCombatState(PlayerCombatState.InCombat);
    }

    private IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
    }
    #endregion
}
