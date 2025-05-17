using UnityEngine;

public class EnemyStateMachine
{
    #region Variables

    public EnemyState[] states;
    public Enemy enemy;
    public EnemyStateID currentState;

    private float stateChangeCooldown = 0f;

    #endregion

    #region State Logic
    public EnemyStateMachine(Enemy enemy)
    {
        this.enemy = enemy;
        int numStates = System.Enum.GetNames(typeof(EnemyStateID)).Length;

        states = new EnemyState[numStates]; 
    }

    public void RegisterState(EnemyState state)
    {
        int index = (int)state.GetID();
        states[index] = state;
    }

    public EnemyState GetEnemyState(EnemyStateID stateID)
    {
        int index = (int)stateID;
        return states[index]; //return state to that pecific index
    }


    public void ChangeState(EnemyStateID newState)
    {
        bool isForced = newState == EnemyStateID.Death;

        if (!isForced && (stateChangeCooldown > 0f || newState == currentState))
            return;

        var current = GetEnemyState(currentState);
        current?.Exit(enemy);

        currentState = newState;

        var next = GetEnemyState(currentState);
        if (next != null)
        {
            Debug.Log($"[StateMachine] Changing to state: {currentState}");
            next.Enter(enemy);
            stateChangeCooldown = isForced ? 0f : 0.15f;
        }
        else
        {
            Debug.LogError($"[StateMachine] ERROR: Tried to enter unregistered state: {currentState}");
        }
    }
    #endregion

    #region Update
    public void Update()
    {
        if (stateChangeCooldown > 0f)
            stateChangeCooldown -= Time.deltaTime;

        GetEnemyState(currentState)?.Update(enemy);
    }

    #endregion

}
