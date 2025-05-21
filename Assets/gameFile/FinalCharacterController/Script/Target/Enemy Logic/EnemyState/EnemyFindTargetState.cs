using UnityEngine;

public class EnemyFindTargetState : EnemyState
{
    private float pickupCheckCooldown = 1.5f;
    private float pickupCheckTimer;
    private GameObject currentPickupTarget;

    public void Enter(Enemy enemy)
    {
        enemy.navMeshAgent.speed = enemy.config.findTargetSpeed;
        pickupCheckTimer = pickupCheckCooldown;
        currentPickupTarget = null;
    }

    public void Exit(Enemy enemy)
    {
        currentPickupTarget = null;
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.FindTarget;
    }

    public void Update(Enemy enemy)
    {
        if (enemy.isDead) return;

        // === Melee Logic ===
        if (enemy.CanMelee && enemy.targeting.HasTarget)
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);

            if (distanceToPlayer <= enemy.config.meleeRange)
            {
                enemy.stateMachine.ChangeState(EnemyStateID.AttackMelee);
                return;
            }
            else
            {
                if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
                {
                    enemy.navMeshAgent.SetDestination(enemy.playerTransform.position);
                }
                return;
            }
        }

        // === Ranged Logic ===
        if (enemy.CanUseWeapons && enemy.weapons.HasWeapon() && enemy.targeting.HasTarget)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.AttackTarget);
            return;
        }

        enemy.teleportCooldownTimer -= Time.deltaTime;
        if (enemy.teleportCooldownTimer <= 0f)
        {
            if (BossUtility.ShouldTeleport(enemy))
            {
                enemy.teleportCooldownTimer = enemy.config.behaviorProfile.dashCooldown;
                enemy.stateMachine.ChangeState(EnemyStateID.BossTeleport);
                return;
            }
        }

        //bravery (override the coward flee if the weapon is in range)
        if (currentPickupTarget != null && currentPickupTarget.activeInHierarchy)
        {
            enemy.navMeshAgent.speed = enemy.config.findWeaponSpeed;
            enemy.navMeshAgent.SetDestination(currentPickupTarget.transform.position);
            return;
        }

        // doesn't have weapon but can use one? Search for pickup
        if (!enemy.weapons.HasWeapon() && enemy.CanUseWeapons)
        {
            pickupCheckTimer -= Time.deltaTime;

            if (pickupCheckTimer <= 0f)
            {
                pickupCheckTimer = pickupCheckCooldown;

                foreach (var obj in enemy.sensor.Objects)
                {
                    if (obj.TryGetComponent<ItemPickup>(out var pickup) &&
                        pickup.ItemData != null &&
                        pickup.ItemData.ItemType == ItemType.Weapon)
                    {
                        Debug.Log("[FindTarget] Found weapon pickup. Risking life to grab it.");
                        currentPickupTarget = pickup.gameObject;
                        enemy.navMeshAgent.speed = enemy.config.findWeaponSpeed;
                        enemy.navMeshAgent.SetDestination(currentPickupTarget.transform.position);
                        return;
                    }
                }
            }

            // No weapon found, fallback coward or default wandering
            if (enemy.IsRangedOnly)
                WanderNearPlayer(enemy);
            else
                DefaultWandering(enemy);

            return;
        }

        // Armed but no target, or fallback if not ranged only
        if (enemy.IsRangedOnly)
            WanderNearPlayer(enemy);
        else
            DefaultWandering(enemy);
    }

    private bool ShouldTeleport(Enemy enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);
        return distance > 5f && enemy.canMeleeAttack; // or use cooldown, time alive, etc
    }

    private void DefaultWandering(Enemy enemy)
    {
        if (!enemy.navMeshAgent.hasPath || enemy.navMeshAgent.remainingDistance < 1f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * enemy.config.wanderRadius;
            randomOffset.y = 0;
            Vector3 wanderTarget = enemy.playerTransform.position + randomOffset;
            if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
            {
                enemy.navMeshAgent.SetDestination(wanderTarget);
            }
        }
    }

    private void WanderNearPlayer(Enemy enemy)
    {
        float safeRange = enemy.config.gunRange * 0.75f;
        Vector3 directionFromPlayer = (enemy.transform.position - enemy.playerTransform.position).normalized;
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);

        if (distanceToPlayer < enemy.config.meleeRange + 1f)
        {
            // flee
            Vector3 fleeTarget = enemy.transform.position + directionFromPlayer * safeRange;
            enemy.navMeshAgent.speed = enemy.config.findWeaponSpeed;
            enemy.navMeshAgent.SetDestination(fleeTarget);
        }
        else
        {
            // wander cowardly
            if (!enemy.navMeshAgent.hasPath || enemy.navMeshAgent.remainingDistance < 1.5f)
            {
                Vector3 randomOffset = Random.insideUnitSphere * (enemy.config.wanderRadius * 0.5f);
                randomOffset.y = 0;

                Vector3 wanderTarget = enemy.playerTransform.position + directionFromPlayer * safeRange + randomOffset;
                enemy.navMeshAgent.speed = enemy.config.findTargetSpeed;
                enemy.navMeshAgent.SetDestination(wanderTarget);
            }
        }
    }
}