using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{

    [SerializeField] private string prompt;
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    private Animator animator;
    public bool hasOpened;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public string interactionPrompt => prompt;

    public bool Interact(Interactor interactor)
    {
        if (hasOpened)
        {
            return false;
        }
        hasOpened = true;
        Debug.Log("Chest opened");
        animator.SetTrigger("Open");
        return true;
    }

    public InteractionPromptUI GetInteractionPromptUI()
    {
        return interactionPromptUI;
    }
}
