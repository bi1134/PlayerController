using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoDisplay;
    public DynamicInventoryDisplay inventoryPanel;
    public WeaponDisplay weaponSlotDisplay;
    [SerializeField] private PlayerActionInput playerInput;
    [SerializeField] private InventoryHolder inventoryHolder;

    private bool inventoryWasOpen = false;

    private void Update()
    {
        if (playerInput == null || inventoryHolder == null) return;

        if (playerInput.tabPressed && !inventoryWasOpen)
        {
            inventoryWasOpen = true;
            inventoryPanel.gameObject.SetActive(true);
            inventoryPanel.RefreshDynamicInventory(inventoryHolder.PassiveItemInventory);

            if (weaponSlotDisplay != null)
                weaponSlotDisplay.AssignSlot(inventoryHolder.WeaponInventory);
        }
        else if (!playerInput.tabPressed && inventoryWasOpen)
        {
            inventoryWasOpen = false;
            inventoryPanel.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        InventoryHolder.OnWeaponInventoryChanged += DisplayWeaponInventory;
    }

    private void OnDisable()
    {
        InventoryHolder.OnWeaponInventoryChanged -= DisplayWeaponInventory;
    }

    private void DisplayWeaponInventory(InventorySystem weaponInventory)
    {
        if (weaponSlotDisplay != null)
        {
            weaponSlotDisplay.AssignSlot(weaponInventory);
        }
    }
    public void UpdateAmmo(int bulletsLeft, int magazineSize, int bulletsPerTap)
    {
        ammoDisplay.text = $"{bulletsLeft / bulletsPerTap} / {magazineSize / bulletsPerTap}";
    }
}
