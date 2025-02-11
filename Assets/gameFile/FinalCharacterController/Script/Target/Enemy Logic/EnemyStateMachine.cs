using UnityEngine;

public class EnemyStateMachine
{
    #region Variables

    public EnemyState[] states;
    public Enemy enemy;
    public EnemyStateID currentState;


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
        GetEnemyState(currentState).Exit(enemy);
        currentState = newState;
        GetEnemyState(currentState)?.Enter(enemy);
    }
    #endregion

    #region Update
    public void Update()
    {
        GetEnemyState(currentState)?.Update(enemy);
    }
    #endregion

}
