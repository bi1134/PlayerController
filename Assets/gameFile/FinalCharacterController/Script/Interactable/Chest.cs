using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt;
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public string interactionPrompt => prompt;

    public bool Interact(Interactor interactor)
    {
        Debug.Log("Chest opened");
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }
        return true;
    }

    public InteractionPromptUI GetInteractionPromptUI()
    {
        return interactionPromptUI;
    }
}
