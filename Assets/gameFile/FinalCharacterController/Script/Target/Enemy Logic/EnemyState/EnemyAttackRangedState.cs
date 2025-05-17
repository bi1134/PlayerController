using UnityEngine;

public class EnemyAttackRangedState : EnemyState
{
    public EnemyStateID GetID() => EnemyStateID.AttackTarget;

    public void Enter(Enemy enemy)
    {
        Debug.Log("Entered: Ranged Attack State");
        enemy.weapons.ActivateWeapon();
    }

    public void Update(Enemy enemy)
    {
        if (!enemy.targeting.HasTarget)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            return;
        }

        float sqrDistance = (enemy.playerTransform.position - enemy.transform.position).sqrMagnitude;
        float gunSqrRange = enemy.config.gunRange * enemy.config.gunRange;

        if (sqrDistance > gunSqrRange * 1.2f)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            return;
        }

        if (sqrDistance > gunSqrRange)
        {
            enemy.navMeshAgent.isStopped = false;
            enemy.navMeshAgent.destination = enemy.playerTransform.position;
        }
        else
        {
            enemy.navMeshAgent.isStopped = true;
        }

        enemy.weapons.SetTarget(enemy.playerTransform);
        enemy.weapons.SetFiring(enemy.targeting.IsTargetInSight);
    }

    public void Exit(Enemy enemy)
    {
        enemy.weapons.SetFiring(false);
    }
}
