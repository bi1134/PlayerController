using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HealthSystem healthSystem;
    
    public void OnRaycastHit(WeaponRaycast playerAttack, Vector3 direction)
    {
        healthSystem.TakeDamage(playerAttack.damage, direction);
    }

}
