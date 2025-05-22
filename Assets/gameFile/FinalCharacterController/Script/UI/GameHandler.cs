using System;
using UnityEngine;


public class GameHandler : MonoBehaviour
{
    public enum GameState
    {
        WaitingToStart,
        GamePlaying,
        GameOver
    }
    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        SetState(GameState.WaitingToStart);
    }
    public GameState state;

    private void Update()
    {
        switch(state)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.GamePlaying:
                break;
            case GameState.GameOver:
                break;
                default:
                break;
        }

    }

    private void SetState(GameState newState)
    {
        state = newState;
        print(state);
        OnGameStateChanged?.Invoke(state);
    }

    public void StartGame()
    {
        SetState(GameState.GamePlaying);
        Debug.Log("Game Started");
    }

    public void TriggerGameOver()
    {
        SetState(GameState.GameOver);
    }
}
