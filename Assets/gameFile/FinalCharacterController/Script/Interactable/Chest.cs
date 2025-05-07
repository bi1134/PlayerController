using UnityEditor;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{

    [SerializeField] private string prompt;
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    [SerializeField] private GameObject lootOrbPrefab;

    [SerializeField] private Transform itemHolder;
    [SerializeField] private LootableSO lootTable;


    private Animator animator;
    public bool hasOpened;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public string interactionPrompt => prompt;

    void ShowItem()
    {
        InventoryItemData item = lootTable.GetRandomItem();

        GameObject orb = Instantiate(lootOrbPrefab, itemHolder.position, Quaternion.identity);
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
        ShowItem();
        hasOpened = true;
        Debug.Log("Chest opened");
        animator.SetTrigger("Open");
        return true;
    
    }
    public InteractionPromptUI GetInteractionPromptUI()
    {
        return interactionPromptUI;
    }

    public bool ShouldDisplayPrompt()
    {
        return !hasOpened;
    }
}
