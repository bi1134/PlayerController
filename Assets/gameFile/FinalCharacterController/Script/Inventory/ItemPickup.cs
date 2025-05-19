using NUnit.Framework.Interfaces;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ItemPickup : MonoBehaviour, IInteractable
{
    public float PickupRadius = 1f;
    public float pickUpTimer = 10f;
    public float pickUpMaxTimer = 15f;

    public InventoryItemData ItemData;

    private SphereCollider itemCollider;
    public Transform outlineMesh;
    public Quaternion PickupRotation = Quaternion.identity;
    private AudioSource audioSource;

    private bool isPickedUp = false;
    private bool isInitialized = false;

    public string interactionPrompt => ItemData != null ? $"pick up {ItemData.ItemName}" : "pick up";
    public bool isInteractable => true;

    public string sidePrompt => null;

    private void Awake()
    {
        itemCollider = GetComponent<SphereCollider>();
        itemCollider.isTrigger = true;
        itemCollider.radius = PickupRadius;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (!audioSource) return;
        float randomPitch = Random.Range(1f, 1.2f);
        audioSource.pitch = randomPitch;
        audioSource.Play();
    }

    private void Update()
    {
        if (isPickedUp) return;

        pickUpTimer -= Time.deltaTime;
        if(pickUpTimer <= 0)
        {
            pickUpTimer = pickUpMaxTimer;
            ObjectPooler.ReturnToPool("ItemPickup", gameObject);
        }
    }

    public bool ShouldDisplayPrompt() => !isPickedUp && isInitialized;


    public bool Interact(Interactor interactor)
    {
        var inventory = interactor.GetComponent<InventoryHolder>();
        if (inventory && inventory.PickUpItem(ItemData))
        {
            isPickedUp = true;
            PickupNotificationManager.Instance.EnqueuePickup(ItemData);
            ObjectPooler.ReturnToPool("ItemPickup", gameObject);
        }
        return isPickedUp;
    }

    public InteractionPromptPanelUI GetInteractionPromptUI()
    {
        return null; // If you're using a global prompt UI, return null
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        if (!isInitialized || ItemData == null) return;
        var inventory = other.GetComponent<InventoryHolder>();
        if (inventory)
        {
            if (inventory.PickUpItem(ItemData))
            {
                isPickedUp = true;
                PickupNotificationManager.Instance.EnqueuePickup(ItemData);
                ObjectPooler.ReturnToPool("ItemPickup", gameObject);
                return;
            }
        }

        var enemy = other.GetComponent<Enemy>();
        var enemyWeapon = other.GetComponentInChildren<EnemyWeapon>();
        if (enemyWeapon && !enemyWeapon.HasWeapon() && ItemData.ItemType == ItemType.Weapon && enemy.config.canUseWeapons)
        {
            var health = other.GetComponent<EnemyHealth>();
            if (health != null && health.IsDead()) return;
            isPickedUp = true;
            WeaponBase newWeapon = Instantiate(ItemData.Prefab).GetComponent<WeaponBase>();
            if (newWeapon == null)
            {
                Debug.LogWarning("ItemPickup: Could not instantiate weapon from ItemData.");
                return;
            }
            newWeapon.inventoryData = ItemData;
            enemyWeapon.Equip(newWeapon);
            ObjectPooler.ReturnToPool("ItemPickup", gameObject);
        }
    }

    public void SetItemData(InventoryItemData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SetItemData called with NULL data!");
            return;
        }

        ItemData = data;
        isInitialized = true;
        pickUpTimer = pickUpMaxTimer;

        if (ItemData == null || ItemData.Prefab == null || outlineMesh == null)
            return;

        GameObject fullItem = Instantiate(ItemData.Prefab);

        Transform itemMeshRoot = fullItem.transform.Find("ItemMesh");
        if (itemMeshRoot == null)
        {
            Debug.LogWarning("ItemPickup: 'Item Mesh' not found in weapon prefab.");
            Destroy(fullItem);
            return;
        }

        Mesh combinedMesh = MeshCombiner.CombineMeshes(itemMeshRoot.gameObject);
        if (combinedMesh == null)
        {
            Debug.LogWarning("ItemPickup: Could not combine mesh from 'Item Mesh'");
            Destroy(fullItem);
            return;
        }

        Material itemMaterial = null;
        MeshRenderer firstRenderer = itemMeshRoot.GetComponentInChildren<MeshRenderer>();
        if (firstRenderer != null && firstRenderer.sharedMaterials.Length > 0)
        {
            itemMaterial = firstRenderer.sharedMaterials[0];
        }

        MeshFilter mf = outlineMesh.GetComponent<MeshFilter>();
        MeshRenderer mr = outlineMesh.GetComponent<MeshRenderer>();

        if (mf != null) mf.mesh = combinedMesh;

        if (mr != null && itemMaterial != null)
        {
            var outlineMat = new Material(mr.sharedMaterials[0]); // duplicate to avoid shared instance overwrite
            outlineMat.SetColor("_Color", RarityColorManager.Instance.GetColor(ItemData.rarity));

            mr.materials = new Material[] { outlineMat, itemMaterial };
        }

        Destroy(fullItem);
    }
}
