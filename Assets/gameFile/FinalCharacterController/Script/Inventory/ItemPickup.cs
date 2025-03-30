using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ItemPickup : MonoBehaviour
{
    public float PickupRadius = 1f;

    public InventoryItemData ItemData;

    private SphereCollider itemCollider;

    private void Awake()
    {
        itemCollider = GetComponent<SphereCollider>();
        itemCollider.isTrigger = true;
        itemCollider.radius = PickupRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        //player pick up
        var inventory = other.GetComponent<InventoryHolder>();
        if (inventory) // if inventory = true
        {
            if (inventory.PickUpItem(ItemData))
            {
                Destroy(gameObject);
                return; 
            }
        }

        //enemy pick up
        EnemyWeapon enemyWeapon = other.GetComponentInChildren<EnemyWeapon>();
       
        if (enemyWeapon && ItemData.ItemType == ItemType.Weapon)
        {
            Debug.Log("Enemy picked up weapon!");

            WeaponRaycast newWeapon = Instantiate(ItemData.Prefab).GetComponent<WeaponRaycast>();

            // Equip the weapon
            enemyWeapon.Equip(newWeapon);

            Destroy(gameObject); // Destroy the pickup
        }
    }
}
