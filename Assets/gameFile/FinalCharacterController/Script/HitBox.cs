using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HealthSystem healthSystem;

    public void TakeDamage(float damage, Vector3 direction, float multiplier = 1f)
    {
        if (healthSystem == null) return;

        healthSystem.TakeDamage(damage * multiplier, direction);
    }
}
