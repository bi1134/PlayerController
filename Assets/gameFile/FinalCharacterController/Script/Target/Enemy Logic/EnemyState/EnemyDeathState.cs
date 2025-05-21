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

        DropExperience(enemy);
        DropMoney(enemy);

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

    private void DropExperience(Enemy enemy)
    {
        int totalExp = Mathf.RoundToInt(enemy.enemyStats.baseStats.exp);
        int maxExpOrbs = 6;
        int orbsToSpawn = Mathf.Min(maxExpOrbs, totalExp);

        for (int i = 0; i < orbsToSpawn; i++)
        {
            int amount = totalExp / orbsToSpawn;
            GameObject orb = ObjectPooler.SpawnFromPool("ExpOrb", enemy.transform.position, Quaternion.identity);

            if (orb.TryGetComponent(out ExpOrb expOrb))
            {
                expOrb.experienceAmount = amount;

                // randomize scale and offset
                orb.transform.localScale = Vector3.one * Random.Range(0.15f, 0.25f);
                orb.transform.position += Random.insideUnitSphere * 1.5f;
            }
        }
    }

    private void DropMoney(Enemy enemy)
    {
        int totalMoney = enemy.enemyStats.baseStats.money;
        int maxCoins = 8;
        int coinsToSpawn = Mathf.Min(maxCoins, totalMoney);

        for (int i = 0; i < coinsToSpawn; i++)
        {
            int amount = totalMoney / coinsToSpawn;
            GameObject coin = ObjectPooler.SpawnFromPool("Money", enemy.transform.position, Quaternion.identity);

            if (coin.TryGetComponent(out MoneyItem money))
            {
                money.moneyAmount = amount;

                // randomize scale and offset
                float scale = Random.Range(0.04f, 0.1f);
                coin.transform.localScale = new Vector3(scale, scale * 0.5f, scale);
            }
        }
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {
    }

}
