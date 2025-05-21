using UnityEngine;

public class EnemyAttackMeleeState : EnemyState
{
    public EnemyStateID GetID() => EnemyStateID.AttackMelee;

    public void Enter(Enemy enemy)
    {
        Debug.Log("Entered: Melee Attack");
        enemy.navMeshAgent.speed = enemy.config.chaseTargetSpeed;
    }

    public void Exit(Enemy enemy)
    {
        enemy.isInMelee = false;
        enemy.navMeshAgent.isStopped = false;
        enemy.animator.ResetTrigger("isAttacking");
    }

    public void Update(Enemy enemy)
    {
        if (enemy.isDead) return;

        if (!enemy.targeting.HasTarget)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            return;
        }

        Transform target = enemy.targeting.Target.transform;
        float distance = Vector3.Distance(enemy.transform.position, target.position);

        if (distance > enemy.config.meleeRange)
        {
            // Consider teleporting
            enemy.teleportCooldownTimer -= Time.deltaTime;
            if (BossUtility.ShouldTeleport(enemy))
            {
                enemy.teleportCooldownTimer = enemy.config.behaviorProfile.dashCooldown;
                enemy.stateMachine.ChangeState(EnemyStateID.BossTeleport);
                return;
            }

            // Regular chase
            enemy.isInMelee = false;
            if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
            {
                enemy.navMeshAgent.stoppingDistance = 0f;
                enemy.navMeshAgent.isStopped = false;
                enemy.navMeshAgent.SetDestination(target.position);
            }
            return;
        }

        // Always update destination to avoid standing still when behind
        if (!enemy.isInMelee)
        {
            enemy.navMeshAgent.SetDestination(target.position);
        }

        if (!enemy.isInMelee && enemy.canMeleeAttack)
        {
            enemy.animator.SetTrigger("isAttacking");
            enemy.isInMelee = true;
            enemy.navMeshAgent.isStopped = true;
        }
    }
}
