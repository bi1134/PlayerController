using UnityEngine;

public class EnemyMemoryRelay : MonoBehaviour
{
    public static void ShareMemory(Vector3 origin, GameObject target, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius);
        foreach (var hit in hits)
        {
            var targeting = hit.GetComponent<EnemyTargetingSystem>();
            if (targeting != null)
            {
                targeting.InjectMemory(target);
            }
        }
    }
}
