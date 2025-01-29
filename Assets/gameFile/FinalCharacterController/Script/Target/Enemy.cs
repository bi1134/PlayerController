using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Compoments")]
    [SerializeField] public EnemyConfig config;
    [SerializeField] public EnemyStateID initialState;


    [NonSerialized]
    //get compoments stuff
    public NavMeshAgent navMeshAgent;
    public EnemyStateMachine stateMachine;
    public Ragdoll ragdoll;
    public Transform playerTransform;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stateMachine = new EnemyStateMachine(this);
        stateMachine.RegisterState(new EnemyChasePlayerState());
        stateMachine.RegisterState(new EnemyDeathState());
        stateMachine.RegisterState(new EnemyIdleState());
        stateMachine.ChangeState(initialState);
        ragdoll = GetComponent<Ragdoll>();

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        stateMachine.Update();
    }
}
