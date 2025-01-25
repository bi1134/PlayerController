using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HealthSystem healthSystem;
    
    public void OnRaycastHit(PlayerAttack playerAttack, Vector3 direction)
    {
        healthSystem.TakeDamage(playerAttack.damage, direction);
    }

}
