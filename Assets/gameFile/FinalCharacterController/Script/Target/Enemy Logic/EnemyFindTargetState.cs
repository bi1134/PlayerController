using UnityEngine;

public class EnemyFindTargetState : EnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entered: FindTarget");
        enemy.navMeshAgent.speed = enemy.config.findTargetSpeed;
    }

    public void Exit(Enemy enemy)
    {
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.FindTarget;
    }

    public void Update(Enemy enemy)
    {
       
        if (enemy.targeting.HasTarget)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.AttackTarget);
        }

        if (!enemy.navMeshAgent.hasPath || enemy.navMeshAgent.remainingDistance < 1f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * enemy.config.wanderRadius;
            randomOffset.y = 0;
            Vector3 wanderTarget = enemy.playerTransform.position + randomOffset;

            enemy.navMeshAgent.destination = wanderTarget;
        }
    }

}
