using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InteractionPrompt interactionUI;

    private readonly Collider[] colliders = new Collider[3];
    private PlayerActionInput playerActionInput;

    private IInteractable currentInteractable;
    private InteractionPromptPanelUI currentPromptUI;
    private InteractableOutline currentOutline;

    private void Start()
    {
        playerActionInput = GetComponent<PlayerActionInput>();
    }

    private void Update()
    {
        IInteractable closest = FindClosestInteractable(out InteractionPromptPanelUI promptUI);

        // If interactable becomes invalid
        if (currentInteractable != null && !currentInteractable.ShouldDisplayPrompt())
        {
            ClearCurrentInteraction();
            return;
        }

        if (closest != null)
        {
            if (currentInteractable != closest)
            {
                // Clean up previous
                ClearCurrentInteraction();

                // Assign new
                currentInteractable = closest;
                currentPromptUI = promptUI;
                currentOutline = (currentInteractable as MonoBehaviour)?.GetComponent<InteractableOutline>();
                currentOutline?.EnableOutline();

                // Set prompts
                if (currentPromptUI != null && !currentPromptUI.isDisplayed)
                    currentPromptUI.SetUp(currentInteractable.interactionPrompt);

                if (currentInteractable is ItemPickup pickup && pickup.ItemData != null)
                {
                    interactionUI.ShowPickup("pick up", pickup.ItemData);
                }
                else
                {
                    interactionUI.Show("interact", currentInteractable.sidePrompt ?? currentInteractable.interactionPrompt);
                }
            }

            if (playerActionInput.isInteracting)
            {
                if (currentInteractable.Interact(this))
                {
                    ClearCurrentInteraction();
                }
            }
        }
        else
        {
            ClearCurrentInteraction();
        }
    }

    private IInteractable FindClosestInteractable(out InteractionPromptPanelUI promptUI)
    {
        float closestDistance = float.MaxValue;
        IInteractable closest = null;
        promptUI = null;

        int numFound = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionPointRadius, colliders, interactableMask);

        for (int i = 0; i < numFound; i++)
        {
            IInteractable interactable = colliders[i].GetComponent<IInteractable>();
            if (interactable != null && interactable.ShouldDisplayPrompt())
            {
                float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                    promptUI = interactable.GetInteractionPromptUI();
                }
            }
        }

        return closest;
    }

    private void ClearCurrentInteraction()
    {
        interactionUI?.Hide();
        if (currentPromptUI != null && currentPromptUI.isDisplayed)
            currentPromptUI.Close();

        currentOutline?.DisableOutline();

        currentInteractable = null;
        currentPromptUI = null;
        currentOutline = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionPointRadius);
    }
}
