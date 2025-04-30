using UnityEngine;

public class EnemyHealth : HealthSystem
{
    Enemy enemy;

    protected override void OnStart()
    {
        enemy = GetComponent<Enemy>();
        maxHealth = enemy.config.health;
    }
    protected override void OnDeath(Vector3 direction)
    {
        EnemyDeathState deathState = enemy.stateMachine.GetEnemyState(EnemyStateID.Death) as EnemyDeathState;

        deathState.direction = direction;
        enemy.stateMachine.ChangeState(EnemyStateID.Death);
    }
    protected override void OnDamage(Vector3 direction)
    {

    }
}
