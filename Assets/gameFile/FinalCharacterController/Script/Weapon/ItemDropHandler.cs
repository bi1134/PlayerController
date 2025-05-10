using System.Collections;
using UnityEngine;

public class ItemDropHandler : MonoBehaviour
{
    [SerializeField] private float transformDelay = 1.5f;
    [SerializeField] private GameObject itemPickupPrefab;

    private InventoryItemData itemData;

    public void Initialize(InventoryItemData item)
    {
        itemData = item;
        StartCoroutine(TransformToItem());
    }

    private IEnumerator TransformToItem()
    {
        yield return Helpers.GetWaitForSecond(transformDelay);

        if (itemData != null)
        {
            GameObject pickup = ObjectPooler.SpawnFromPool("ItemPickup", transform.position, Quaternion.identity);
            var pickupScript = pickup.GetComponent<ItemPickup>();
            if (pickupScript != null)
            {
                pickupScript.SetItemData(itemData);
            }
        }

        ObjectPooler.ReturnToPool("ItemOrb", gameObject);
    }
}
