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
            enemy.weapons.ActivateWeapon();
        }
    }

    private ItemPickup FindClosestWeapon(Enemy enemy)
    {
        ItemPickup[] weapons = GameObject.FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);

        if (weapons.Length == 0)
        {
            Debug.Log("No weapons found!");
            return null;
        }

        ItemPickup closestWeapon = null;
        float closestDistance = float.MaxValue;

        foreach (var weapon in weapons)
        {
            float distanceToWeapon = Vector3.Distance(enemy.transform.position, weapon.transform.position);
            if (distanceToWeapon < closestDistance)
            {
                closestDistance = distanceToWeapon;
                closestWeapon = weapon;
            }
        }

        Debug.Log($"Closest weapon found at {closestWeapon.transform.position}");
        return closestWeapon;
    }

}
