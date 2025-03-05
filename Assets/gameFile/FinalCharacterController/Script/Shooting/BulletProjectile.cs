using System.Collections;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private TrailRenderer tracer;

    private Rigidbody bulletRigidbody;
    private Vector3 initialPosition;
    private Vector3 initialVelocity;
    private float time;
    private bool isActive = false;

    public float bulletDrop = 9.81f; // Simulating gravity

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 position, Vector3 velocity, Collider shooterCollider, float maxLifeTime)
    {
        initialPosition = position;
        initialVelocity = velocity;
        time = 0f;
        isActive = true;

        transform.position = position;
        bulletRigidbody.linearVelocity = velocity;

        if (tracer != null)
        {
            tracer.Clear();
            tracer.transform.position = position;
        }
        StartCoroutine(DeactivateAfterTime(maxLifeTime)); // Bullet auto-deactivates
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        time += Time.fixedDeltaTime;

        // Apply Gravity: s = ut + 0.5 * at²
        Vector3 gravity = Vector3.down * bulletDrop;
        Vector3 displacement = (initialVelocity * time) + (0.5f * gravity * time * time);
        transform.position = initialPosition + displacement;

        // Update tracer effect position
        if (tracer != null)
        {
            tracer.transform.position = transform.position;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Vector3 pushDirection = collision.relativeVelocity.normalized;
            bulletRigidbody.AddForce(pushDirection * 0.1f, ForceMode.Impulse);
        }

        isActive = false;
        gameObject.SetActive(false);

        // Spawn bullet hit effect
        ObjectPooler.SpawnFromPool("BulletHit", transform.position, Quaternion.identity);
    }


    private IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        isActive = false;
        gameObject.SetActive(false);
    }

}
