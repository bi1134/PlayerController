using System.Collections;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] public GameObject tracer;
    public BulletPropertiesSO settings;

    private WeaponProjectile shooter;
    private int bounceRemaining;
    public bool isActive;
    private Rigidbody rb;
    private GameObject shooterGameObject;
    private HealthSystem shooterHealth;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        isActive = true;
        bounceRemaining = settings.maxBounces;

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            var mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);

            Color color = Color.white;
            switch (settings.bulletType)
            {
                case BulletType.Normal:
                    color = new Color(191f / 255f, 131f / 255f, 0f, 1f); // yellow-ish
                    break;
                case BulletType.Explosive:
                    color = new Color(6f / 255f, 51f / 255f, 3f / 255f, 1f); // dark green
                    break;
            }

            mpb.SetColor("_Color", color); // Particle Unlit uses _Color
            renderer.SetPropertyBlock(mpb);
        }

        if (tracer != null && tracer.TryGetComponent(out TrailRenderer trail))
        {
            tracer.SetActive(true);
            trail.Clear();
            trail.transform.position = transform.position;

            var mpb = new MaterialPropertyBlock();
            trail.GetPropertyBlock(mpb);

            Color trailBase = Color.red;
            Color trailEmission = Color.yellow;

            switch (settings.bulletType)
            {
                case BulletType.Normal:
                    trailBase = new Color(1f, 0f, 0f); // red
                    trailEmission = new Color(191f / 255f, 102f / 255f, 0f) * 3.416924f;
                    break;
                case BulletType.Explosive:
                    trailBase = new Color(2f / 255f, 18f / 255f, 191f / 255f);
                    trailEmission = new Color(81f / 255f, 186f / 255f, 5f / 255f) * 2.923552f;
                    break;
            }

            mpb.SetColor("_BaseColor", trailBase);         // URP Lit base color
            mpb.SetColor("_EmissionColor", trailEmission); // HDR Emission
            trail.SetPropertyBlock(mpb);
        }
    }

    public void Initialize(Vector3 direction, float bulletSpeed, float upwardForce, float lifeTime)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(direction.normalized * bulletSpeed, ForceMode.Impulse);
        rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

        StartCoroutine(DestroySelf(lifeTime));
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        isActive = false;
    }

    IEnumerator DestroySelf(float delay)
    {
        yield return Helpers.GetWaitForSecond(delay);
        Deactivate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;

        GameObject fx = ObjectPooler.SpawnFromPool("BulletHit", hitPoint, Quaternion.identity);
        if (fx.TryGetComponent(out PooledEffect pooledFx))
        {
            pooledFx.SetPoolTag("BulletHit");
        }

        if (collision.collider.TryGetComponent(out HitBox target))
        {
            if (target.healthSystem == shooterHealth)
                return;

            target.TakeDamage(shooter, rb.linearVelocity.normalized);

            if (settings.bulletType == BulletType.Explosive)
            {
                Explode(contact.point);
                return;
            }
        }

        if (bounceRemaining > 0)
        {
            bounceRemaining--;

            // Reflect the velocity off the hit normal
            Vector3 incomingVelocity = rb.linearVelocity;
            Vector3 reflected = Vector3.Reflect(incomingVelocity, hitNormal).normalized;

            // Maintain speed (or slightly reduce to simulate energy loss if you want)
            float speed = incomingVelocity.magnitude;
            rb.linearVelocity = reflected * speed;

            // Offset to prevent getting stuck in surface
            transform.position = hitPoint + hitNormal * 0.01f;
        }
        else
        {
            if (settings.bulletType == BulletType.Explosive)
            {
                Explode(hitPoint);
            }
            else
            {
                Deactivate();
            }
        }
    }

    private void Explode(Vector3 point)
    {
        Collider[] enemies = Physics.OverlapSphere(point, settings.explosionRadius, settings.explosionMask);

        foreach (Collider enemy in enemies)
        {
            if (enemy.TryGetComponent(out HitBox target))
            {
                if (target.healthSystem == shooterHealth)
                    continue;

                Vector3 direction = (enemy.transform.position - point).normalized;
                target.TakeDamage(shooter, direction);
            }

            if (enemy.TryGetComponent<Rigidbody>(out var enemyRb))
            {
                enemyRb.AddExplosionForce(settings.explosionForce, point, settings.explosionRadius, 0.1f, ForceMode.Impulse);
            }
        }

        GameObject fx = ObjectPooler.SpawnFromPool("Explosion", point, Quaternion.identity);
            
        if (fx.TryGetComponent(out PooledEffect pooledFx))
        {
            pooledFx.SetPoolTag("Explosion");
        }
        Deactivate();
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        StopAllCoroutines();
        if (tracer != null) tracer.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        ObjectPooler.ReturnToPool(settings.bulletPoolTag, gameObject);
    }

    public void SetShooter(WeaponProjectile shooter)
    {
        this.shooter = shooter;
        shooterGameObject = shooter.gameObject;

        shooterHealth = shooter.GetComponentInParent<HealthSystem>();

        Collider[] bulletColliders = GetComponentsInChildren<Collider>();
        Collider[] shooterColliders = shooter.GetComponentsInChildren<Collider>();

        foreach (var bulletCol in bulletColliders)
        {
            foreach (var shooterCol in shooterColliders)
            {
                Physics.IgnoreCollision(bulletCol, shooterCol);
            }
        }
    }

}


