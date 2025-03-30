using UnityEngine;

public class BulletProjectile : MonoBehaviour, IPooledObject
{
    [SerializeField] private TrailRenderer tracer;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private BulletPropertiesSO defaultProperties;

    private Rigidbody bulletRigidbody;
    private Vector3 initialPosition;
    private Vector3 initialVelocity;
    private bool isActive = false;
    private BulletPropertiesSO bulletProperties;

    public float time;
    public int bounce;

    private float bulletDamage;
    private WeaponDamageType damageType;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 position, Vector3 velocity, BulletPropertiesSO properties)
    {
        bulletProperties = properties ?? defaultProperties;

        initialPosition = position;
        initialVelocity = velocity;
        time = 0f;
        isActive = true;

        transform.position = position;
        bulletRigidbody.rotation = Quaternion.LookRotation(velocity.normalized);

        bulletRigidbody.angularVelocity = Vector3.zero;
        bulletRigidbody.linearVelocity = velocity;

        if (tracer != null)
        {
            tracer.Clear();
            tracer.transform.position = position;
        }

        bounce = bulletProperties.maxBounces;
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        time = 0f;

        if (bulletProperties != null)
        {
            bounce = bulletProperties.maxBounces;
            time = bulletProperties.maxLifeTime;
        }

        // Ensure it's visible and properly positioned
        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        time += Time.fixedDeltaTime;

        if (time >= bulletProperties.maxLifeTime) // bullet expired
        {
            isActive = false;
            ObjectPooler.ReturnToPool("Bullet", gameObject);
            return;
        }

        // Apply Gravity: s = ut + 0.5 * at²
        Vector3 gravity = Vector3.down * bulletProperties.bulletDrop;
        Vector3 displacement = (initialVelocity * time) + (0.5f * gravity * time * time);
        Vector3 nextPosition = initialPosition + displacement;

        Vector3 moveDirection = nextPosition - transform.position;
        float moveDistance = moveDirection.magnitude;

        if (Physics.Raycast(transform.position, moveDirection.normalized, out RaycastHit hit, moveDistance, collisionMask))
        {
            HitBox hitBox = hit.collider.GetComponent<HitBox>();
            if (hitBox != null && damageType == WeaponDamageType.BulletCollision)
            {
                hitBox.OnBulletHit(bulletDamage, initialVelocity.normalized);
            }

            ObjectPooler.SpawnFromPool("BulletHit", hit.point, Quaternion.identity);
            // If bullet still has bounces left, reflect and continue
            if (bounce > 0)
            {
                bounce--;
                time = 0;
                initialPosition = hit.point + hit.normal * 0.01f;
                Vector3 reflectedVelocity = Vector3.Reflect(initialVelocity, hit.normal);
                if (Vector3.Dot(reflectedVelocity, initialVelocity) < 0) // Ensures it doesn't bounce backward
                {
                    initialVelocity = reflectedVelocity;
                }
            }
            else
            {
                // If no bounces left, destroy bullet
                isActive = false;
                ObjectPooler.ReturnToPool("Bullet", gameObject);
            }
        }
        else
        {
            // Move normally if no hit
            transform.position = nextPosition;
        }

        // Update tracer effect position
        if (tracer != null)
        {
            tracer.transform.position = transform.position;
        }
    }

    public void SetDamageInfo(float damage, WeaponDamageType type)
    {
        bulletDamage = damage;
        damageType = type;
    }

}
