using UnityEngine;

public class CartonBox : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    [SerializeField] private GameHandler gameHandler;
    [SerializeField] private string prompt;

    public string interactionPrompt => prompt;

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

    public InteractionPromptUI GetInteractionPromptUI()
    {
        return interactionPromptUI;
    }
}
