using NUnit.Framework.Interfaces;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ItemPickup : MonoBehaviour
{
    public float PickupRadius = 1f;

    public InventoryItemData ItemData;

    private SphereCollider itemCollider;
    public Transform outlineMesh;
    public Quaternion PickupRotation = Quaternion.identity;

    private void Awake()
    {
        itemCollider = GetComponent<SphereCollider>();
        itemCollider.isTrigger = true;
        itemCollider.radius = PickupRadius;
    }

    private void Start()
    {
        if (ItemData != null && ItemData.Prefab != null && outlineMesh != null)
        {
            GameObject fullItem = Instantiate(ItemData.Prefab);

            // Find the "Item Mesh" inside the instantiated weapon
            Transform itemMeshRoot = fullItem.transform.Find("ItemMesh");
            if (itemMeshRoot == null)
            {
                Debug.LogWarning("ItemPickup: 'Item Mesh' not found in weapon prefab.");
                Destroy(fullItem);
                return;
            }

            // Combine all child meshes
            Mesh combinedMesh = MeshCombiner.CombineMeshes(itemMeshRoot.gameObject);
            if (combinedMesh == null)
            {
                Debug.LogWarning("ItemPickup: Could not combine mesh from 'Item Mesh'");
                Destroy(fullItem);
                return;
            }

            // Get a representative material from the first renderer
            Material itemMaterial = null;
            MeshRenderer firstRenderer = itemMeshRoot.GetComponentInChildren<MeshRenderer>();
            if (firstRenderer != null && firstRenderer.sharedMaterials.Length > 0)
            {
                itemMaterial = firstRenderer.sharedMaterials[0];
            }

            // Apply mesh and materials to the outline object
            MeshFilter mf = outlineMesh.GetComponent<MeshFilter>();
            MeshRenderer mr = outlineMesh.GetComponent<MeshRenderer>();
            if (mf != null) mf.mesh = combinedMesh;

            if (mr != null && itemMaterial != null)
            {
                var originalOutlineMat = mr.sharedMaterials.Length > 0 ? mr.sharedMaterials[0] : null;
                mr.materials = new Material[] { originalOutlineMat, itemMaterial };
            }

            Destroy(fullItem);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        //player pick up
        var inventory = other.GetComponent<InventoryHolder>();
        if (inventory) // if inventory = true
        {
            if (ItemData == null)
            {
                Debug.LogError("ItemPickup: itemData is NULL!");
                return;
            }

            bool pickedUp = inventory.PickUpItem(ItemData);
            if (pickedUp)
            {
                Destroy(gameObject);
            }

        }

        //enemy pick up
        EnemyWeapon enemyWeapon = other.GetComponentInChildren<EnemyWeapon>();
       
        if (enemyWeapon && ItemData.ItemType == ItemType.Weapon)
        {
            if (enemyWeapon.HasWeapon()) return;

            WeaponBase newWeapon = Instantiate(ItemData.Prefab).GetComponent<WeaponBase>();

            // Equip the weapon
            enemyWeapon.Equip(newWeapon);

            Destroy(gameObject); // Destroy the pickup
        }
    }

    public void SetItemData(InventoryItemData data)
    {
        ItemData = data;
    }
}
