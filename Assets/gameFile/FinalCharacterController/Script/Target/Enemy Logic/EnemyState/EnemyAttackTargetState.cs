using UnityEngine;

public class EnemyAttackTargetState : EnemyState
{
    private float pickupCheckTimer = 3f;
    private const float pickupCheckCooldown = 3f;


    public void Enter(Enemy enemy)
    {
        pickupCheckTimer = pickupCheckCooldown;
        enemy.weapons.ActivateWeapon();
        enemy.navMeshAgent.stoppingDistance = enemy.config.chaseTargetSpeed;
    }

    public void Exit(Enemy enemy)
    {
        enemy.navMeshAgent.stoppingDistance = 0f;
        enemy.weapons.SetFiring(false);
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.AttackTarget;
    }

    public void Update(Enemy enemy)
    {
        if (enemy.isDead) return;

        // check if player is dead
        if (!enemy.targeting.HasTarget)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            return;
        }

        enemy.weapons.SetTarget(enemy.targeting.Target.transform);
        float sqrDistance = (enemy.playerTransform.position - enemy.transform.position).sqrMagnitude;
        float meleeSqrRange = enemy.config.meleeRange * enemy.config.meleeRange;
        float gunSqrRange = enemy.config.gunRange * enemy.config.gunRange;

        // check for weapon pickups if enemy is unarmed
        if (!enemy.weapons.HasWeapon())
        {
            pickupCheckTimer -= Time.deltaTime;

            if (pickupCheckTimer <= 0f)
            {
                pickupCheckTimer = pickupCheckCooldown;
                if (WeaponInSight(enemy))
                {
                    enemy.stateMachine.ChangeState(EnemyStateID.FindWeapon);
                    return;
                }
            }
        }
        else
        {
            pickupCheckTimer = pickupCheckCooldown;
        }

        // if player is within melee range
        if (sqrDistance <= meleeSqrRange)
        {
            if (enemy.isInMelee)
                LerpNavSpeed(enemy, enemy.config.chaseAttackTargetSpeed);
            else
                LerpNavSpeed(enemy, enemy.config.chaseTargetSpeed);

            if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsTag("Melee"))
            {
                enemy.navMeshAgent.isStopped = true;
                enemy.navMeshAgent.ResetPath(); 
                enemy.animator.SetTrigger("isAttacking");

                enemy.isInMelee = true;
                enemy.weapons.SetFiring(false);
            }

            if (!enemy.isInMelee && enemy.weapons.HasWeapon())
            {
                UpdateFiring(enemy);
            }
            return;
        }

        // if enemy has weapon and is within gun range
        if (enemy.weapons.HasWeapon() && sqrDistance <= gunSqrRange)
        {
            enemy.weapons.ActivateWeapon();

            float preferredRange = Random.Range(enemy.config.gunRange / 2f, enemy.config.gunRange - 1f);
            float preferredSqrRange = preferredRange * preferredRange;

            if (sqrDistance > preferredSqrRange)
            {
                // move closer to preferred range
                enemy.navMeshAgent.stoppingDistance = preferredRange;
                enemy.navMeshAgent.isStopped = false;
                enemy.navMeshAgent.destination = enemy.targeting.TargetPosition;
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

        //this is for if the enemy is standing still
        if (!enemy.weapons.HasWeapon() && !enemy.navMeshAgent.hasPath)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindWeapon);
            return;
        }
    }

    private void UpdateFiring(Enemy enemy)
    {
        if (enemy.targeting.IsTargetInSight)
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

    private void LerpNavSpeed(Enemy enemy, float targetSpeed, float lerpRate = 5f)
    {
        enemy.navMeshAgent.speed = Mathf.Lerp(enemy.navMeshAgent.speed, targetSpeed, Time.deltaTime * lerpRate);
    }

}
