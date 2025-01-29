using UnityEngine;

public class EnemyDeathState : EnemyState
{
    public Vector3 direction;

    public EnemyStateID GetID()
    {
        return EnemyStateID.Death;
    }

    public void Enter(Enemy enemy)
    {
        enemy.ragdoll.ActivateRagdoll();
        direction.y = 1;
        enemy.ragdoll.ApplyForce(direction * enemy.config.ragdollForce);
    }

    public void Update(Enemy enemy)
    {
    }

    public void Exit(Enemy enemy)
    {
    }

}
