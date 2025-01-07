using UnityEngine;

public class PlayerState : MonoBehaviour
{
   [field: SerializeField] public PlayerMovementState currentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
   
    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        currentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState()
    {
        return IsStateGroundedState(currentPlayerMovementState);
    }

    public bool IsStateGroundedState(PlayerMovementState movementState)
    {
        return  movementState == PlayerMovementState.Idling ||
                movementState == PlayerMovementState.Walking ||
                movementState == PlayerMovementState.Running ||
                movementState == PlayerMovementState.Sprinting;
    
    }

}
public enum PlayerMovementState
    {
        Idling,
        Walking,
        Running,
        Sprinting,
        Jumping,
        Falling,
        Strafing
    }
