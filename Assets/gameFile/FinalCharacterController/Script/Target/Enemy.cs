using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    #region Variables
    [Header("Compoments")]
    [SerializeField] public EnemyConfig config;
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    [SerializeField] private BoxCollider meleeHitbox;
    public EnemyStateID initialState;


    [NonSerialized]
    //get compoments stuff
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public EnemyStateMachine stateMachine;
    [HideInInspector] public Ragdoll ragdoll;
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public EnemyWeapon weapons;
    [HideInInspector] private Vector3 originalPosition;
    [HideInInspector] private Quaternion originalRotation;
    [HideInInspector] public Animator animator;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool canMeleeAttack = true;
    [HideInInspector] public EnemySensor sensor;

    #endregion

    #region Start Up
    private void Start()
    {
        animator = GetComponent<Animator>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        navMeshAgent = GetComponent<NavMeshAgent>();
        weapons = GetComponent<EnemyWeapon>();
        ragdoll = GetComponent<Ragdoll>();
        sensor = GetComponent<EnemySensor>();

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        stateMachine = new EnemyStateMachine(this);
        stateMachine.RegisterState(new EnemyChasePlayerState());
        stateMachine.RegisterState(new EnemyDeathState());
        stateMachine.RegisterState(new EnemyIdleState());
        stateMachine.RegisterState(new EnemyFindWeaponState());
        stateMachine.RegisterState(new EnemyAttackPlayerState());
        stateMachine.ChangeState(initialState);
    }
    #endregion

    #region Update 
    private void Update()
    {
        stateMachine.Update();
    }

    public void SetAttackTriggerFalse()
    {
        animator.ResetTrigger("isAttacking");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canMeleeAttack || !meleeHitbox.enabled) return;

        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                Vector3 direction = (other.transform.position - transform.position).normalized;
                health.TakeDamage(config.meleeDamage, direction);

                canMeleeAttack = false;
                StartCoroutine(ResetMeleeCooldown());
            }
        }
    }

    private IEnumerator ResetMeleeCooldown()
    {
        yield return Helpers.GetWaitForSecond(meleeAttackCooldown);
        canMeleeAttack = true;
    }

    public void EnableMeleeHitbox()
    {
        if (canMeleeAttack)
        {
            meleeHitbox.enabled = true;
        }
    }

    public void DisableMeleeHitbox()
    {
        meleeHitbox.enabled = false;
    }
    #endregion
}
