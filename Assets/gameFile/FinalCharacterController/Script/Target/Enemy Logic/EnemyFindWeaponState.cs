using UnityEngine;

public class EnemyFindWeaponState : EnemyState
{
    private GameObject pickUp;
    private GameObject[] pickUps = new GameObject[1];

    private float timer = 0f;
    private const float maxSearchDuration = 3f;

    public void Enter(Enemy enemy)
    {
        Debug.Log("Entered: FindWeapon");
        pickUp = null;
        timer = maxSearchDuration;
        enemy.navMeshAgent.speed = enemy.config.findWeaponSpeed;
    }

    public void Exit(Enemy enemy)
    {
    }

    public EnemyStateID GetID()
    {
        return EnemyStateID.FindWeapon;
    }

    public void Update(Enemy enemy)
    {
        timer -= Time.deltaTime;

        //check for weapon insight
        if (!pickUp)
        {
            pickUp = FindPickup(enemy);

            if (pickUp)
            {
                CollectPickup(enemy, pickUp);
            }
        }

        // If found a weapon or already equipped one, switch to attack
        if (enemy.weapons.HasWeapon())
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            return;
        }

        if (timer <= 0f)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
            return;
        }


        // If player is closer than any weapon and in melee range, attack instead
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);
        if (distanceToPlayer <= enemy.config.meleeRange)
        {
            enemy.stateMachine.ChangeState(EnemyStateID.AttackTarget);
        }
    }

    GameObject FindPickup(Enemy enemy)
    {
        int count = enemy.sensor.Filter(pickUps, "Pickup");
        for (int i = 0; i < count; i++)
        {
            ItemPickup pickup = pickUps[i].GetComponent<ItemPickup>();
            if (pickup != null && pickup.ItemData != null && pickup.ItemData.ItemType == ItemType.Weapon)
            {
                return pickup.gameObject;
            }
        }
        return null;
    }

    private void CollectPickup(Enemy enemy, GameObject pickUp)
    {
        enemy.navMeshAgent.destination = pickUp.transform.position;
    }
}
