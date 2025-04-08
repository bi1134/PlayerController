using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] public GameObject tracer;
    public BulletPropertiesSO settings;

    private WeaponProjectile shooter;
    private int bounceRemaining;
    public bool isActive;
    private Rigidbody rb;
    private GameObject shooterGameObject;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        isActive = true;
        bounceRemaining = settings.maxBounces;

        if (tracer != null)
        {
            tracer.SetActive(true);
            if (tracer.TryGetComponent(out TrailRenderer trail))
            {
                trail.Clear();
                trail.transform.position = transform.position;
            }
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
            target.TakeDamage(shooter, rb.linearVelocity.normalized);
            if (settings.bulletType == BulletType.Explosive)
            {
                Explode(collision.contacts[0].point);
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
                if (enemy.gameObject == shooterGameObject) continue;
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


