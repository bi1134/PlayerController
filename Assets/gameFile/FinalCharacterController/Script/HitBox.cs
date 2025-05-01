using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HealthSystem healthSystem;

    public void TakeDamage(WeaponBase weapon, Vector3 direction, float multiplier = 1f)
    {
        if (weapon == null || healthSystem == null) return;

        float damage = weapon.GetWeaponDamage() * multiplier;
        healthSystem.TakeDamage(damage, direction);
    }
}
