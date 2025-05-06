using System.Collections;
using UnityEngine;

public class WeaponDropHandler : MonoBehaviour
{
    [SerializeField] private float pickupRadius = 3f;
    [SerializeField] private float dropForce = 5f;
    [SerializeField] private GameObject weaponDropPrefab;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float dropDelay = 0.1f;
    private WeaponBase weapon;
    public GameObject itemPickupPrefab; // Reference to a generic pickup prefab



    private void Awake()
    {
        weapon = GetComponent<WeaponBase>();
    }

    private void Start()
    {
        StartCoroutine(DelayedConvertToPickup());
    }

    private IEnumerator DelayedConvertToPickup()
    {
        //yield return Helpers.GetWaitForSecond(delayBeforePickup);

        if (weapon == null || weapon.inventoryData == null)
        {
            Debug.LogWarning("DroppedWeaponHandler: Weapon data is missing.");
            yield break;
        }

        GameObject pickup = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
        ItemPickup itemPickup = pickup.GetComponent<ItemPickup>();
        if (itemPickup)
        {
            itemPickup.ItemData = weapon.inventoryData;
        }

        Destroy(gameObject);
    }
}
