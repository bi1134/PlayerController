public interface IInteractable
{
    public string interactionPrompt { get; }
    string sidePrompt { get; }
    public bool Interact(Interactor interactor);
    InteractionPromptPanelUI GetInteractionPromptUI();
    bool ShouldDisplayPrompt();
}
