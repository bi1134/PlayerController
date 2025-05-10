using UnityEngine;

public class EnemyDeathState : EnemyState
{
    public delegate void DeathEvent();
    public static event DeathEvent OnDeath;

    public Vector3 direction;

    public EnemyStateID GetID()
    {
        return EnemyStateID.Death;
    }

    public void Enter(Enemy enemy)
    {
        if (enemy.isDead) return;

        enemy.isDead = true;
        enemy.weapons.DropWeapon();
        enemy.ragdoll.ActivateRagdoll();
        direction.y = 1;
        enemy.ragdoll.ApplyForce(direction * enemy.config.ragdollForce);
        enemy.ragdoll.DisableCollidersDelayed(5.0f);

        enemy.GetComponent<EnemyAI>().enabled = false;
        enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        enemy.GetComponentInChildren<Animator>().enabled = false;
        enemy.GetComponent<EnemySensor>().enabled = false;

        var pooled = enemy.GetComponent<EnemyPooled>();
        EnemyCorpseTracker.Register(enemy.gameObject);
        OnDeath?.Invoke();
        if (pooled != null)
        {
            pooled.OnDeath();
        }
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {
    }

}
