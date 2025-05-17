using UnityEngine;

public class EnemyHealth : HealthSystem
{
    Enemy enemy;

    protected override void OnStart()
    {
        enemy = GetComponent<Enemy>();
    }
    protected override void OnDeath(Vector3 direction)
    {
        if (enemy.isDead) return;

        enemy.isDead = true;

        Debug.Log("dead trigger");
        var deathState = enemy.stateMachine.GetEnemyState(EnemyStateID.Death) as EnemyDeathState;
        deathState.SetDeathDirection(direction);

        enemy.stateMachine.ChangeState(EnemyStateID.Death);
    }
    protected override void OnDamage(Vector3 direction)
    {

    }
}
