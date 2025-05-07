using System.Threading;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoDisplay;
    [SerializeField] private TextMeshProUGUI timerDisplay;
    public DynamicInventoryDisplay inventoryPanel;
    public WeaponDisplay weaponSlotDisplay;
    [SerializeField] private PlayerActionInput playerInput;
    [SerializeField] private InventoryHolder inventoryHolder;

    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject gameOverUI;

    private bool inventoryWasOpen = false;
    private bool isGameOver = false;
    private float timer;

    private void Awake()
    {
        GameHandler.OnGameStateChanged += HandleGameStateChanged;
        isGameOver = false;
        SetAllInactive();
    }

    private void HandleGameStateChanged(GameHandler.GameState newState)
    {
        switch (newState)
        {
            case GameHandler.GameState.WaitingToStart:
                SetAllInactive();
                break;

            case GameHandler.GameState.GamePlaying:
                playerUI.SetActive(true);
                gameOverUI.SetActive(false);
                break;

            case GameHandler.GameState.GameOver:
                playerUI.SetActive(false);
                gameOverUI.SetActive(true);
                isGameOver = true;
                inventoryWasOpen = true;
                inventoryPanel.gameObject.SetActive(true);
                inventoryPanel.RefreshDynamicInventory(inventoryHolder.PassiveItemInventory);

                if (weaponSlotDisplay != null)
                    weaponSlotDisplay.AssignSlot(inventoryHolder.WeaponInventory);
                break;
        }
    }


    private void Update()
    {
        if (!isGameOver)
        {
            timer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (playerInput == null || inventoryHolder == null || isGameOver) return;

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

    private void SetAllInactive()
    {
        playerUI?.SetActive(false);
        gameOverUI?.SetActive(false);
    }

    private void OnDestroy()
    {
        GameHandler.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void OnEnable()
    {
        InventoryHolder.OnWeaponInventoryChanged += DisplayWeaponInventory;
        Debug.Log("Player Canvas ENABLED", gameObject);
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
