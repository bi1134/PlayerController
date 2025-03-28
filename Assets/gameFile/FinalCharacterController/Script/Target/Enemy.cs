using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    #region Variables
    [Header("Compoments")]
    [SerializeField] public EnemyConfig config;
    public EnemyStateID initialState;


    [NonSerialized]
    //get compoments stuff
    public NavMeshAgent navMeshAgent;
    public EnemyStateMachine stateMachine;
    public Ragdoll ragdoll;
    public Transform playerTransform;
    public EnemyWeapon weapons;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    #endregion

    #region Start Up
    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        navMeshAgent = GetComponent<NavMeshAgent>();
        weapons = GetComponent<EnemyWeapon>();
        stateMachine = new EnemyStateMachine(this);
        stateMachine.RegisterState(new EnemyChasePlayerState());
        stateMachine.RegisterState(new EnemyDeathState());
        stateMachine.RegisterState(new EnemyIdleState());
        stateMachine.RegisterState(new EnemyFindWeaponState());
        stateMachine.RegisterState(new EnemyAttackPlayerState());
        stateMachine.ChangeState(initialState);
        ragdoll = GetComponent<Ragdoll>();

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    #endregion

    #region Update 
    private void Update()
    {
        stateMachine.Update();
    }
    #endregion
}
