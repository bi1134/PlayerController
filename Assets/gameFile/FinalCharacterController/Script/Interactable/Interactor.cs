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
    private float closestDistance = float.MaxValue;
    private IInteractable closestInteractable = null;
    private InteractionPromptUI closestPromptUI = null;

    private void Start()
    {
        playerActionInput = GetComponent<PlayerActionInput>();
    }

    private void Update()
    {
        closestDistance = float.MaxValue;
        closestInteractable = null;
        closestPromptUI = null;

        numFound = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionPointRadius, colliders, interactableMask);

        for (int i = 0; i < numFound; i++)
        {
            IInteractable interactable = colliders[i].GetComponent<IInteractable>();
            if (interactable != null && interactable.ShouldDisplayPrompt())
            {
                float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                    closestPromptUI = interactable.GetInteractionPromptUI();
                }
            }
        }

        if (closestInteractable != null)
        {
            if (currentInteractable != closestInteractable)
            {
                // Close previous
                if (currentPromptUI != null && currentPromptUI.isDisplayed)
                    currentPromptUI.Close();

                currentInteractable = closestInteractable;
                currentPromptUI = closestPromptUI;

                if (currentPromptUI != null && !currentPromptUI.isDisplayed)
                    currentPromptUI.SetUp(currentInteractable.interactionPrompt);
            }

            if (playerActionInput.isInteracting)
            {
                currentInteractable.Interact(this);
                currentPromptUI?.Close();
            }
        }
        else
        {
            if (currentPromptUI != null && currentPromptUI.isDisplayed)
                currentPromptUI.Close();

            currentInteractable = null;
            currentPromptUI = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionPointRadius);
    }
}
