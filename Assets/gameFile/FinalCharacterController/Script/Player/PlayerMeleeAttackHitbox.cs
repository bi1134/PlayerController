using UnityEngine;

public class PlayerMeleeAttackHitbox : MonoBehaviour
{
    public PlayerStats playerStats;
    public LayerMask enemyMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) != 0)
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                Vector3 direction = (other.transform.position - transform.position).normalized;
                health.TakeDamage(playerStats.baseStats.baseDamage, direction);
            }
        }
    }
}
