using UnityEngine;
using UnityEngine.AI;

public class EnemyIdleState : EnemyState
{
    public EnemyStateID GetID()
    {
        return EnemyStateID.Idle;
    }

    public void Enter(Enemy enemy)
    {
    }



    public void Update(Enemy enemy)
    {
        Vector3 playerDirection = enemy.playerTransform.position - enemy.transform.position;
        if(playerDirection.magnitude > enemy.config.maxSightDistance)
        {
            return;
        }

        Vector3 enemyDirection = enemy.transform.forward;

        playerDirection.Normalize();

        float dotProduct = Vector3.Dot(playerDirection, enemyDirection);

        if(dotProduct > 0)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.ChasePlayer);
        }
    }
    public void Exit(Enemy enemy)
    {
    }
}
