using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private Transform vfxHitBlue;
    [SerializeField] private Transform vfxHitRed;

    private Rigidbody bulletRigidbody;
    public float bulletSpeed = 10f;

    private Vector3 targetPoint;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Vector3 direction = (targetPoint - transform.position).normalized;
        bulletRigidbody.linearVelocity = direction * bulletSpeed ;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

    public void HandleHit(Vector3 hitPosition, bool hitTarget)
    {
        Transform vfxPrefab = hitTarget ? vfxHitBlue : vfxHitRed;
        Instantiate(vfxPrefab, hitPosition, Quaternion.identity);

    }

    public void SetTarget(Vector3 target)
    {
        targetPoint = target;
    }

}
