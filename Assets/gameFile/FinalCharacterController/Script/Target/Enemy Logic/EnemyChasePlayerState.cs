using UnityEngine;
using UnityEngine.AI;

public class EnemyChasePlayerState : EnemyState
{
    private float timer = 0.1f;

    public EnemyStateID GetID()
    {
        return EnemyStateID.ChasePlayer;
    }

    public void Enter(Enemy enemy)
    {
       
        
    }

    public void Update(Enemy enemy)
    {
        if (!enemy.enabled)
            return;

        timer -= Time.deltaTime;
        if(!enemy.navMeshAgent.hasPath)
            enemy.navMeshAgent.destination = enemy.playerTransform.position;

        if (enemy.playerTransform != null)
        {
            if (timer <= 0f)
            {
                float sqrtDistance = (enemy.playerTransform.position - enemy.navMeshAgent.destination).sqrMagnitude;
                if (sqrtDistance > enemy.config.maxDistance * enemy.config.maxDistance)
                {
                    if(enemy.navMeshAgent.pathStatus != NavMeshPathStatus.PathPartial)
                    {
                        enemy.navMeshAgent.destination = enemy.playerTransform.position;
                    }
                }
                timer = enemy.config.maxTime;
            }
        }
    }
    public void Exit(Enemy enemy)
    {

    }
}
