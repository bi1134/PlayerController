using UnityEngine;

public class EnemyAttackPlayerState : EnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.weapons.ActivateWeapon();
        enemy.weapons.SetTarget(enemy.playerTransform);
        enemy.navMeshAgent.stoppingDistance = 5f;
    }

    public void Exit(Enemy enemy)
    {
        enemy.navMeshAgent.stoppingDistance = 0f;
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.AttackPlayer;
    }

    public void Update(Enemy enemy)
    {
        enemy.navMeshAgent.destination = enemy.playerTransform.position;
    }
}
