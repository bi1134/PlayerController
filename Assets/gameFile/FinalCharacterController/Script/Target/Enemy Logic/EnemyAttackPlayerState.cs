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
        enemy.weapons.SetFiring(false);
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.AttackPlayer;
    }

    public void Update(Enemy enemy)
    {
        if (enemy.playerTransform == null)
            return;

        float sqrDistance = (enemy.playerTransform.position - enemy.transform.position).sqrMagnitude;
        float meleeSqrRange = enemy.config.meleeRange * enemy.config.meleeRange;
        float gunSqrRange = enemy.config.gunRange * enemy.config.gunRange;


        // 1. Handle player death
        if (enemy.playerTransform.GetComponent<HealthSystem>().IsDead())
        {
            enemy.stateMachine.ChangeState(EnemyStateID.Idle);
            return;
        }

        // 2. If in melee range, melee regardless of weapon
        if (sqrDistance <= meleeSqrRange)
        {
            enemy.navMeshAgent.isStopped = true;

            if (enemy.weapons.HasWeapon())
            {
                enemy.weapons.SetFiring(false);
            }

            if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsTag("Melee"))
            {
                enemy.animator.SetTrigger("isAttacking");
            }
            return;
        }

        // 3. If enemy has a weapon and in shooting range
        if (enemy.weapons.HasWeapon() && sqrDistance > meleeSqrRange && sqrDistance <= gunSqrRange)
        {
            float preferredRange = Random.Range(enemy.config.gunRange / 2f, enemy.config.gunRange - 1f);
            float preferredSqrRange = preferredRange * preferredRange;

            if (sqrDistance > preferredSqrRange)
            {
                // Move closer until at preferred range
                enemy.navMeshAgent.stoppingDistance = preferredRange;
                enemy.navMeshAgent.isStopped = false;
                enemy.navMeshAgent.destination = enemy.playerTransform.position;
            }
            else
            {
                // Within preferred range, hold position and fire
                enemy.navMeshAgent.isStopped = true;
            }

            enemy.weapons.SetTarget(enemy.playerTransform);
            enemy.weapons.SetFiring(true);
            return;
        }

        // 4. If not in any range (too far), chase player
        enemy.weapons.SetFiring(false); // stop firing if too far
        enemy.navMeshAgent.isStopped = false;
        enemy.navMeshAgent.stoppingDistance = 0f;
        enemy.navMeshAgent.destination = enemy.playerTransform.position;
    }




}
