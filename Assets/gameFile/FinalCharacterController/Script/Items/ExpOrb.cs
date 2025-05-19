using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public int experienceAmount = 5;

    [Header("Orb Movement")]
    public float minSpeed = 7f;
    public float maxSpeed = 11f;
    public float pickupRange = 1.2f;
    public float hoverTime = 0.3f;
    public float hoverHeight = 1.5f;

    private static Transform cachedPlayer;
    private Vector3 velocity = Vector3.zero;
    private bool isFollowing = false;
    private float hoverTimer;

    private void OnEnable()
    {
        velocity = Vector3.zero;
        hoverTimer = hoverTime;
        isFollowing = false;

        if (cachedPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                cachedPlayer = playerObj.transform;
        }
    }

    private void Update()
    {
        if (cachedPlayer == null) return;

        if (!isFollowing)
        {
            hoverTimer -= Time.deltaTime;
            Vector3 hoverOffset = new Vector3(
                0f,
                Mathf.Sin(Time.time * 6f) * 0.1f,
                0f
            );
            transform.position += hoverOffset;

            if (hoverTimer <= 0f)
                isFollowing = true;
        }
        else
        {
            Vector3 targetPos = cachedPlayer.position + Vector3.up * 1.5f;
            float distance = Vector3.Distance(transform.position, targetPos);

            // Smooth damp speed gets faster as orb gets closer
            float smoothTime = Mathf.Lerp(1f / maxSpeed, 1f / minSpeed, distance / 10f); // near = fast
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref velocity,
                smoothTime
            );

            if (distance <= pickupRange)
            {
                if (cachedPlayer.TryGetComponent(out PlayerLevel playerLevel))
                {
                    playerLevel.AddExperience(experienceAmount);
                }

                ObjectPooler.ReturnToPool("ExpOrb", gameObject);
            }
        }
    }
}
