using UnityEngine;

public class EnemyPhaseControllerState : EnemyState
{
    public EnemyStateID GetID() => EnemyStateID.PhaseController;

    public void Enter(Enemy enemy)
    {
        Debug.Log("Boss Phase Controller Started");
        // Check HP and switch to Attack / Rage / Summon
    }

    public void Update(Enemy enemy)
    {
        float hpPercent = enemy.enemyStats.CurrentHPPercent;

        if (hpPercent > 0.7f)
            enemy.stateMachine.ChangeState(EnemyStateID.AttackTarget);
        else if (hpPercent > 0.3f)
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget); // maybe summon
        else
            enemy.stateMachine.ChangeState(EnemyStateID.Death); // or rage
    }

    public void Exit(Enemy enemy) { }
}

