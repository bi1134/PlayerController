using UnityEngine;

public class EnemyFindWeaponState : EnemyState
{
    public void Enter(Enemy enemy)
    {
        ItemPickup pickup = FindClosestWeapon(enemy);
        if (pickup == null)
        {
            Debug.Log("No weapon found, staying in place.");
            return;  // Prevents setting a null destination
        }
        enemy.navMeshAgent.destination = pickup.transform.position;
        enemy.navMeshAgent.speed = 5;
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
        if(enemy.weapons.HasWeapon())
        {
            enemy.stateMachine.ChangeState(EnemyStateID.AttackPlayer);
        }
    }

    private ItemPickup FindClosestWeapon(Enemy enemy)
    {
        ItemPickup[] items = GameObject.FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);

        if (items.Length == 0)
        {
            Debug.Log("No weapons found!");
            return null;
        }

        ItemPickup closestWeapon = null;
        float closestDistance = float.MaxValue;

        foreach (var item in items)
        {
            if (item.ItemData == null || item.ItemData.ItemType != ItemType.Weapon)
                continue;  // Skip non-weapons

            float distanceToWeapon = Vector3.Distance(enemy.transform.position, item.transform.position);
            if (distanceToWeapon < closestDistance)
            {
                closestDistance = distanceToWeapon;
                closestWeapon = item;
            }
        }

        Debug.Log($"Closest weapon found at {closestWeapon.transform.position}");
        return closestWeapon;
    }

}
