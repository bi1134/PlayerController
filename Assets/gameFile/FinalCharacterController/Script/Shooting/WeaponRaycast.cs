using UnityEngine;
using UnityEngine.Playables;

public class WeaponRaycast : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform bulletSpawnPosition;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();

    [Header("Stats")]
    public float damage = 10f;
    public int fireRate = 25;


    //Shooting variables
    public bool isFiring = false;
    private float accumulatedTime;
    private float fireInterval;
    private Vector3 mouseWorldPosition;
    private Ray ray;
    private Transform hitTransform = null;
    private float lerpRotationSpeed = 10f;

    //get components stuff
    private PlayerState playerState;

    #endregion

    #region Startup
    private void Awake()
    {
        playerState = GetComponentInParent<PlayerState>();
        fireInterval = 1.0f / fireRate;
    }

    #endregion

    #region Update
    private void Update()
    {
        HandleShootPosition();
    }
    #endregion

    #region Shooting Logic
    private void HandleShootPosition()
    {
        mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
            hitPoint.position = raycastHit.point;
        }

        //lerp aim point to hit point
        aimPoint.position = Vector3.Lerp(aimPoint.position, hitPoint.position, Time.deltaTime * lerpRotationSpeed);

    }

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

        Vector3 aimDir = (mouseWorldPosition - bulletSpawnPosition.position).normalized;
        Transform bulletTransform = Instantiate(pfBulletProjectile, bulletSpawnPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

        if (bulletTransform == null)
        {
            Debug.LogWarning("[WeaponRaycast] Bullet instantiation failed!");
            return;
        }

        BulletProjectile bulletProjectile = bulletTransform.GetComponent<BulletProjectile>();
        if (bulletProjectile != null)
        {
            bulletProjectile.SetTarget(hitPoint.position);
        }
        else
        {
            Debug.LogWarning("[WeaponRaycast] BulletProjectile script is missing!");
        }

        if (hitTransform != null)
        {
            bool hitTarget = hitTransform.GetComponent<BulletTarget>() != null;
            bulletProjectile?.HandleHit(hitPoint.position, hitTarget);

            Rigidbody rb2d = hitTransform.GetComponent<Rigidbody>();
            if (rb2d != null)
            {
                rb2d.AddForceAtPosition(ray.direction * 20, hitPoint.position, ForceMode.Impulse);
            }

            HitBox hitBox = hitTransform.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.OnRaycastHit(this, ray.direction);
            }
        }

        playerState.SetPlayerCombatState(PlayerCombatState.InCombat);
    }
    #endregion
}
