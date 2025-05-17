using UnityEngine;

public enum EnemyStateID
{
    ChasePlayer,
    Death,
    Idle,
    FindWeapon,
    AttackTarget,
    FindTarget,
    PhaseController,
}

public interface EnemyState
{
    EnemyStateID GetID();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}
