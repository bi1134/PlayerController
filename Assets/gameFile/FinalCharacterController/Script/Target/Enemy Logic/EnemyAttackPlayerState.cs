using UnityEngine;

public class EnemyAttackPlayerState : EnemyState
{
    private float pickupCheckTimer = 3f;
    private const float pickupCheckCooldown = 3f;

    public void Enter(Enemy enemy)
    {
        pickupCheckTimer = pickupCheckCooldown;

        enemy.weapons.ActivateWeapon();
        enemy.weapons.SetTarget(enemy.playerTransform);
        enemy.navMeshAgent.stoppingDistance = 5f;
    }

    public void Exit(Enemy enemy)
    {
        enemy.navMeshAgent.stoppingDistance = 0f;
        enemy.weapons.SetFiring(false);
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.AttackPlayer;
    }

    public void Update(Enemy enemy)
    {
        if (enemy.playerTransform == null)
            return;

        // check if player is dead
        if (enemy.playerTransform.GetComponent<HealthSystem>().IsDead())
        {
            enemy.stateMachine.ChangeState(EnemyStateID.Idle);
            return;
        }

        float sqrDistance = (enemy.playerTransform.position - enemy.transform.position).sqrMagnitude;
        float meleeSqrRange = enemy.config.meleeRange * enemy.config.meleeRange;
        float gunSqrRange = enemy.config.gunRange * enemy.config.gunRange;

        // check for weapon pickups if enemy is unarmed
        if (!enemy.weapons.HasWeapon())
        {
            pickupCheckTimer -= Time.deltaTime;

            if (pickupCheckTimer <= 0f && WeaponInSight(enemy))
            {
                enemy.stateMachine.ChangeState(EnemyStateID.FindWeapon);
                return;
            }
        }
        else
        {
            pickupCheckTimer = pickupCheckCooldown;
        }

        // if player is within melee range
        if (sqrDistance <= meleeSqrRange)
        {
            enemy.navMeshAgent.isStopped = true;

            if (enemy.weapons.HasWeapon())
                UpdateFiring(enemy); // still allow firing if armed and in melee

            if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsTag("Melee"))
                enemy.animator.SetTrigger("isAttacking");

            return;
        }

        // if enemy has weapon and is within gun range
        if (enemy.weapons.HasWeapon() && sqrDistance <= gunSqrRange)
        {
            float preferredRange = Random.Range(enemy.config.gunRange / 2f, enemy.config.gunRange - 1f);
            float preferredSqrRange = preferredRange * preferredRange;

            if (sqrDistance > preferredSqrRange)
            {
                // move closer to preferred range
                enemy.navMeshAgent.stoppingDistance = preferredRange;
                enemy.navMeshAgent.isStopped = false;
                enemy.navMeshAgent.destination = enemy.playerTransform.position;
            }
            else
            {
                // stop moving and fire
                enemy.navMeshAgent.isStopped = true;
            }

            enemy.weapons.SetTarget(enemy.playerTransform);
            UpdateFiring(enemy);
            return;
        }

        // if out of range, chase the player
        UpdateFiring(enemy); // this will disable firing if not in sight
        enemy.navMeshAgent.isStopped = false;
        enemy.navMeshAgent.stoppingDistance = 0f;
        enemy.navMeshAgent.destination = enemy.playerTransform.position;
    }

    private void UpdateFiring(Enemy enemy)
    {
        if (enemy.sensor.IsInsight(enemy.playerTransform.gameObject))
        {
            enemy.weapons.SetFiring(true);
        }
        else
        {
            enemy.weapons.SetFiring(false);
        }
    }

    private bool WeaponInSight(Enemy enemy)
    {
        foreach (var obj in enemy.sensor.Objects)
        {
            if (obj.TryGetComponent<ItemPickup>(out var pickup) &&
                pickup.ItemData != null &&
                pickup.ItemData.ItemType == ItemType.Weapon)
            {
                return true;
            }
        }

        return false;
    }
}
