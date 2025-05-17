using UnityEngine;

public class EnemyDeathState : EnemyState
{
    public delegate void DeathEvent();
    public static event DeathEvent OnDeath;

    private Vector3 deathDirection = Vector3.zero;

    public void SetDeathDirection(Vector3 direction)
    {
        deathDirection = direction;
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.Death;
    }

    public void Enter(Enemy enemy)
    {
        Debug.LogWarning("Entered: Death for " + enemy.name);

        enemy.weapons.DropWeapon();
        enemy.ragdoll.ActivateRagdoll();

        // apply force
        var force = deathDirection;
        force.y = 1;
        enemy.ragdoll.ApplyForce(force * enemy.config.ragdollForce);

        DropExperienceOrb(enemy);

        enemy.ragdoll.DisableCollidersDelayed(5.0f);
        enemy.GetComponent<EnemyAI>().enabled = false;
        enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        enemy.GetComponentInChildren<Animator>().enabled = false;
        enemy.GetComponent<EnemySensor>().enabled = false;

        var pooled = enemy.GetComponent<EnemyPooled>();
        EnemyCorpseTracker.Register(enemy.gameObject);
        OnDeath?.Invoke();
        pooled?.OnDeath();
    }

    private void DropExperienceOrb(Enemy enemy)
    {
        GameObject orb = ObjectPooler.SpawnFromPool("ExpOrb", enemy.transform.position, Quaternion.identity);
        ExpOrb expOrb = orb.GetComponent<ExpOrb>();
        if (expOrb != null)
        {
            expOrb.experienceAmount = Mathf.RoundToInt(enemy.enemyStats.baseStats.exp); // From BaseStatsSO
        }
        else
        {
            Debug.LogWarning("No pooled ExpOrb available!");
        }
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {
    }

}
