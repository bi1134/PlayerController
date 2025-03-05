using UnityEngine;

public class PlayerState : MonoBehaviour
{
   [field: SerializeField] public PlayerMovementState currentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
   [field: SerializeField] public PlayerCombatState currentPlayerCombatState { get; private set; } = PlayerCombatState.notInCombat;
   [field: SerializeField] public PlayerDashState currentPlayerDashingState { get; private set; } = PlayerDashState.notDashing;
   
    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        currentPlayerMovementState = playerMovementState;
    }

    public void SetPlayerCombatState(PlayerCombatState playerCombatState)
    {
        currentPlayerCombatState = playerCombatState;
    }

    public void SetPlayerDashingState(PlayerDashState playerDashingState)
    {
        currentPlayerDashingState = playerDashingState;
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
public enum PlayerCombatState
    {
        InCombat,
        notInCombat
    }

public enum PlayerDashState
{
    Dashing,
    notDashing
}
