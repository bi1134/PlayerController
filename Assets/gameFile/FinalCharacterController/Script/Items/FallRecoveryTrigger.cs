using UnityEngine;
using UnityEngine.AI;

public class FallRecoveryTrigger : MonoBehaviour
{
    [SerializeField] private float damagePercent = 0.2f;
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private float maxSampleHeight = 100f;
    [SerializeField] private float minSampleHeight = -10f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxAllowedSlope = 40f;
    [SerializeField] private Transform[] fallbackRecoveryPoints;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            int damage = Mathf.RoundToInt(health.maxHealth * damagePercent);
            health.TakeDamage(damage, Vector3.up);
        }

        Vector3 safePosition = FindNearestSafeGround(other.transform.position);

        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            other.transform.position = safePosition;
            controller.enabled = true;
        }
        else
        {
            other.transform.position = safePosition;
        }

        var playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            var field = typeof(PlayerController).GetField("verticalVelocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(playerController, -1f);
        }
    }

    private Vector3 FindNearestSafeGround(Vector3 fromPosition)
    {
        Vector3 rayOrigin = fromPosition + Vector3.up * maxSampleHeight;

        // 1. Raycast down
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, maxSampleHeight * 2f, groundMask))
        {
            if (IsValidSlope(hit.normal) && hit.point.y > 10f && hit.point.y < 200f)
            {
                Debug.Log("Raycast hit terrain at: " + hit.point);
                return hit.point + Vector3.up * 1f;
            }
        }

        // 2. Try NavMesh
        if (NavMesh.SamplePosition(fromPosition, out NavMeshHit navHit, searchRadius, NavMesh.AllAreas))
        {
            if (navHit.position.y > minSampleHeight && navHit.position.y < 200f)
            {
                Debug.Log("NavMesh sample found: " + navHit.position);
                return navHit.position + Vector3.up * 1f;
            }
        }

        // 3. Try fallback recovery points
        if (fallbackRecoveryPoints != null && fallbackRecoveryPoints.Length > 0)
        {
            Transform closest = fallbackRecoveryPoints[0];
            float minDist = Vector3.Distance(fromPosition, closest.position);

            foreach (var point in fallbackRecoveryPoints)
            {
                float dist = Vector3.Distance(fromPosition, point.position);
                if (dist < minDist)
                {
                    closest = point;
                    minDist = dist;
                }
            }

            Debug.Log("Using fallback recovery point at: " + closest.position);
            return closest.position + Vector3.up * 1f;
        }

        // 4. Final fallback to world center
        Debug.LogWarning("No safe ground found. Fallback to origin.");
        return new Vector3(0f, 150f, 0f);
    }

    private bool IsValidSlope(Vector3 normal)
    {
        float angle = Vector3.Angle(normal, Vector3.up);
        return angle <= maxAllowedSlope;
    }
}