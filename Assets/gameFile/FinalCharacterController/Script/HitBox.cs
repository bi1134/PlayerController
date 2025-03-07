using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HealthSystem healthSystem;
    
    public void OnRaycastHit(WeaponRaycast playerAttack, Vector3 direction)
    {
        if (playerAttack == null) return;

        float damage = playerAttack.GetWeaponDamage();
        healthSystem.TakeDamage(damage, direction);
    }

}
