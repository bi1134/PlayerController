using UnityEditor;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private string sidePromptText;
    [SerializeField] private int moneyCost = 50;
    [SerializeField] private InteractionPromptPanelUI interactionPromptUI;
    [SerializeField] private GameObject lootOrbPrefab;

    [SerializeField] private Transform itemHolder;
    [SerializeField] private LootableSO lootTable;

    private AudioSource audioSource;
    private Animator animator;
    public bool hasOpened;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public string interactionPrompt => hasOpened ? "" : $"${moneyCost}";

    public string sidePrompt => sidePromptText;

    void ShowItem()
    {
        InventoryItemData item = lootTable.GetRandomItem();

        GameObject orb = ObjectPooler.SpawnFromPool("ItemOrb", itemHolder.position, Quaternion.identity);
        var orbHandler = orb.GetComponent<ItemDropHandler>();
        if (orbHandler != null)
        {
            orbHandler.Initialize(item);
        }

        Rigidbody rb = orb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float upwardForce = 5f; // Adjust for desired height
            float sideForceRange = 2f;

            Vector3 force = Vector3.up * upwardForce;

            // Random horizontal direction (XZ plane)
            Vector3 side = new Vector3(
                Random.Range(-sideForceRange, sideForceRange),
                0f,
                Random.Range(-sideForceRange, sideForceRange)
            );

            force += side;

            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (hasOpened)
        {
            return false;
        }

        PlayerStats stats = interactor.GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogWarning("No PlayerStats found on interactor.");
            return false;
        }

        if (stats.baseStats.money < moneyCost)
        {
            Debug.Log("Not enough money to open chest.");
            // Optional: Add UI popup feedback here
            return false;
        }

        stats.baseStats.money -= moneyCost;
        stats.moneyCount.text = stats.baseStats.money.ToString();

        hasOpened = true;
        OpenChest();
        return true;
    }

    private void OpenChest()
    {
        ShowItem();
        animator.SetTrigger("Open");

        if (audioSource)
        {
            audioSource.pitch = Random.Range(1f, 1.2f);
            audioSource.Play();
        }

        Debug.Log("Chest opened!");
    }

    public InteractionPromptPanelUI GetInteractionPromptUI() => interactionPromptUI;
    public bool ShouldDisplayPrompt() => !hasOpened;
}
