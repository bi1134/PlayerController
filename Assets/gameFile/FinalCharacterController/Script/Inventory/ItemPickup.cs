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
            Debug.Log("ItemPickup: Found InventoryHolder on " + other.name);
            if (ItemData == null)
            {
                Debug.LogError("ItemPickup: itemData is NULL!");
                return;
            }

            inventory.PickUpItem(ItemData);
            
            Destroy(gameObject);
            
        }

        //enemy pick up
        EnemyWeapon enemyWeapon = other.GetComponentInChildren<EnemyWeapon>();
       
        if (enemyWeapon && ItemData.ItemType == ItemType.Weapon)
        {
            WeaponBase newWeapon = Instantiate(ItemData.Prefab).GetComponent<WeaponBase>();

            // Equip the weapon
            enemyWeapon.Equip(newWeapon);

            Destroy(gameObject); // Destroy the pickup
        }
    }
}
