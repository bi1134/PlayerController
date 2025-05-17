using UnityEngine;

public class EnemyChasePlayerState : EnemyState
{
    private float timer = 0.1f;

    public EnemyStateID GetID()
    {
        return EnemyStateID.ChasePlayer;
    }

    public void Enter(Enemy enemy)
    {
        Debug.Log("Entered: chasePlater");
        enemy.navMeshAgent.speed = enemy.config.chaseTargetSpeed;
    }

    public void Update(Enemy enemy)
    {
        if (!enemy.enabled)
            return;

        timer -= Time.deltaTime;
        if(!enemy.navMeshAgent.hasPath)
            enemy.navMeshAgent.destination = enemy.playerTransform.position;

        if (enemy.playerTransform != null && timer <= 0f)
        {
            float sqrDistance = (enemy.playerTransform.position - enemy.transform.position).sqrMagnitude;
            float meleeSqrRange = enemy.config.meleeRange * enemy.config.meleeRange;
            float gunSqrRange = enemy.config.gunRange * enemy.config.gunRange;

            if (sqrDistance <= meleeSqrRange ||
                (enemy.weapons.HasWeapon() && sqrDistance <= gunSqrRange))
            {
                enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            }

            timer = enemy.config.maxTime;
        }
    }
    public void Exit(Enemy enemy)
    {

    }
}
