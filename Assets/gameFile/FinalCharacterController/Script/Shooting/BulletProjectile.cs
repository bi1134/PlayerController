using System.Collections;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] public GameObject tracer;
    public BulletPropertiesSO settings;

    private WeaponProjectile shooter;
    public bool isActive;
    private Rigidbody rb;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 direction, float bulletSpeed, float upwardForce, float lifeTime)
    {
        isActive = true;
        tracer?.SetActive(true);

        rb.linearVelocity = Vector3.zero;
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

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (other.TryGetComponent(out HitBox target))
        {
            target.TakeDamage(shooter, rb.linearVelocity.normalized);
        }

        Deactivate();
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;

        // Reset tracer (if using)
        if (tracer != null) tracer.SetActive(false);

        StopAllCoroutines();
        ObjectPooler.ReturnToPool("Bullet", gameObject);
    }

}


