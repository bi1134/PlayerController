using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HealthSystem healthSystem;
    
    public void TakeDamage(WeaponBase weapon, Vector3 direction)
    {
        if (weapon == null || healthSystem == null) return;

        float damage = weapon.GetWeaponDamage();
        healthSystem.TakeDamage(damage, direction);
    }

}
