using UnityEngine;

public class CartonBox : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionPromptPanelUI interactionPromptUI;

    [SerializeField] private GameHandler gameHandler;
    [SerializeField] private string prompt;
    [SerializeField] private string sidePromptText;

    public string interactionPrompt => prompt;

    public string sidePrompt => sidePromptText;

    private bool hasStarted = false;
    private float lastInteractTime;
    private float cooldown = 1.0f; // seconds before it can be pressed again

    public bool Interact(Interactor interactor)
    {
        if (hasStarted || Time.time - lastInteractTime < cooldown)
            return false;

        hasStarted = true;
        lastInteractTime = Time.time;

        gameHandler.StartGame();
        return true;
    }

    public bool ShouldDisplayPrompt()
    {
        return !hasStarted;
    }

    public InteractionPromptPanelUI GetInteractionPromptUI()
    {
        return interactionPromptUI;
    }
}
