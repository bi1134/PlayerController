using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public int experienceAmount = 5;

    [Header("Orb Settings")]
    public float moveSpeed = 10f;
    public float pickupRange = 1.5f;
    public float hoverTime = 0.3f;
    public float hoverHeight = 1f;

    private static Transform cachedPlayerTransform;

    private bool isCollected = false;
    private float hoverTimer;
    private Vector3 hoverOffset;

    private void OnEnable()
    {
        isCollected = false;
        hoverTimer = hoverTime;
        hoverOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(hoverHeight, hoverHeight + 1f),
            Random.Range(-0.5f, 0.5f)
        );

        // Optional: turn off rigidbody physics if any
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Cache player only once (static)
        if (cachedPlayerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                cachedPlayerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (!isCollected)
        {
            hoverTimer -= Time.deltaTime;

            transform.position = Vector3.Lerp(
                transform.position,
                transform.position + hoverOffset,
                Time.deltaTime * 2f
            );

            if (hoverTimer <= 0f && cachedPlayerTransform != null)
            {
                isCollected = true;
            }
        }
        else
        {
            if (cachedPlayerTransform == null) return;

            Vector3 chestTarget = cachedPlayerTransform.position + Vector3.up * 1.5f;
            transform.position = Vector3.MoveTowards(
                transform.position,
                chestTarget,
                moveSpeed * Time.deltaTime
            );

            if ((transform.position - chestTarget).sqrMagnitude <= pickupRange * pickupRange)
            {
                if (cachedPlayerTransform.TryGetComponent(out PlayerLevel playerLevel))
                {
                    playerLevel.AddExperience(experienceAmount);
                }

                ObjectPooler.ReturnToPool("ExpOrb", gameObject);
            }
        }
    }
}
