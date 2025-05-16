using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    #region Variables
    [Header("Compoments")]
    [SerializeField] public EnemyConfig config;
    [SerializeField] private float meleeAttackCooldown = 0.8f;
    [SerializeField] private BoxCollider meleeHitbox;

    [SerializeField] private Transform lookRoot; // usually hips or full body root
    [SerializeField] private Transform player;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float maxAngle = 120f; // prevent rotating completely backward
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
    [HideInInspector] public bool isInMelee = false;
    [HideInInspector] public EnemySensor sensor;
    [HideInInspector] public EnemyTargetingSystem targeting;
    [HideInInspector] public EnemyStats enemyStats;
    [HideInInspector] public bool isAttackingMelee;



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
        targeting = GetComponent<EnemyTargetingSystem>();
        enemyStats = GetComponent<EnemyStats>();

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        stateMachine = new EnemyStateMachine(this);
        stateMachine.RegisterState(new EnemyChasePlayerState());
        stateMachine.RegisterState(new EnemyDeathState());
        stateMachine.RegisterState(new EnemyIdleState());
        stateMachine.RegisterState(new EnemyFindWeaponState());
        stateMachine.RegisterState(new EnemyAttackTargetState());
        stateMachine.RegisterState(new EnemyFindTargetState());
        stateMachine.ChangeState(initialState);
    }
    #endregion

    #region Update 
    private void Update()
    {
        stateMachine.Update();
    }

    private void LateUpdate()
    {
        if (player == null || lookRoot == null)
            return;

        Vector3 direction = player.position - lookRoot.position;
        direction.y = 0f; // ignore vertical tilt

        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        float angle = Quaternion.Angle(lookRoot.rotation, targetRotation);

        // clamp rotation angle to avoid twisting too far
        if (angle <= maxAngle)
        {
            lookRoot.rotation = Quaternion.Lerp(lookRoot.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }

    public void StartMeleeAnimation()
    {
        if (weapons.weaponIK != null)
            weapons.weaponIK.LerpToWeight(0f, 0.2f);
    }

    public void FinishMeleeAnimation()
    {
        animator.ResetTrigger("isAttacking");
        stateMachine.ChangeState(EnemyStateID.FindTarget);
        isInMelee = false;
        if (weapons.weaponIK != null)
            weapons.weaponIK.LerpToWeight(0.8f, 0.2f);
        if (weapons != null && weapons.HasWeapon())
        {
            weapons.SetFiring(true);
        }
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
                health.TakeDamage(enemyStats.baseStats.baseDamage, direction);

                canMeleeAttack = false;
                PoolRunner.Instance.RunCoroutine(ResetMeleeCooldown());
            }
        }
    }

    public void ResetState()
    {
        StartCoroutine(DeferredReset());
    }

    private IEnumerator DeferredReset()
    {
        ragdoll.DeActivateRagdoll();

        yield return null; // Wait one frame to avoid immediate collision
        ragdoll.EnableCollider();

        navMeshAgent.enabled = true;
        animator.enabled = true;
        GetComponent<EnemySensor>().enabled = true;
        GetComponent<EnemyAI>().enabled = true;
        enemyStats.ResetStats();
        isDead = false;
        stateMachine.ChangeState(initialState);
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
            PoolRunner.Instance.RunCoroutine(DisableMeleeHitbox());
        }
    }

    public IEnumerator DisableMeleeHitbox()
    {
        yield return Helpers.GetWaitForSecond(0.1f);

        meleeHitbox.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(playerTransform.position, config.wanderRadius);
    }
    #endregion
}
