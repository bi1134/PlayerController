using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask interactableMask;
    private readonly Collider[] colliders = new Collider[3];
    public int numFound;

    private PlayerActionInput playerActionInput;
    private IInteractable currentInteractable;
    private InteractionPromptUI currentPromptUI;

    private void Start()
    {
        playerActionInput = GetComponent<PlayerActionInput>();
    }

    private void Update()
    {
        numFound = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionPointRadius, colliders, interactableMask);

        if (numFound > 0)
        {
            IInteractable interactable = colliders[0].GetComponent<IInteractable>();

            if (interactable != null)
            {
                // If new interactable is detected, update UI reference
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;

                    // Get the InteractionPromptUI from the interactable object
                    currentPromptUI = interactable.GetInteractionPromptUI();

                    var chest = interactable as Chest;

                    if (currentPromptUI != null && !currentPromptUI.isDisplayed && !chest.hasOpened)
                    {
                        currentPromptUI.SetUp(interactable.interactionPrompt);
                    }
                }

                // Handle interaction
                if (playerActionInput.isInteracting)
                {
                    interactable.Interact(this);
                    if (currentPromptUI != null && currentPromptUI.isDisplayed)
                    {
                        currentPromptUI.Close();
                    }
                }
            }
        }
        else
        {
            // Reset interaction if nothing is found
            if (currentInteractable != null)
            {
                if (currentPromptUI != null && currentPromptUI.isDisplayed)
                {
                    currentPromptUI.Close();
                }
                currentInteractable = null;
                currentPromptUI = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionPointRadius);
    }
}
