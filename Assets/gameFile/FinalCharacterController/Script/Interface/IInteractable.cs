public interface IInteractable
{
    public string interactionPrompt { get; }
    public bool Interact(Interactor interactor);
    InteractionPromptUI GetInteractionPromptUI();
}
