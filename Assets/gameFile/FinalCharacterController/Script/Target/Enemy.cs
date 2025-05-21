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
    [SerializeField] public Transform firePoint;
    [SerializeField] GameObject tpPreview;
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
    public bool isDead = false;
    [HideInInspector] public bool canMeleeAttack = true;
    [HideInInspector] public bool isInMelee = false;
    [HideInInspector] public EnemySensor sensor;
    [HideInInspector] public EnemyTargetingSystem targeting;
    [HideInInspector] public EnemyStats enemyStats;
    [HideInInspector] public bool isAttackingMelee;
    [HideInInspector] public float teleportCooldownTimer = 0f;

    [Header("Enemy type stuff")]
    public bool IsMeleeOnly => config.enemyType == EnemyType.MeleeOnly;
    public bool IsRangedOnly => config.enemyType == EnemyType.RangedOnly;
    public bool IsFlying => config.enemyType == EnemyType.Flying;
    public bool IsBoss => config.enemyType == EnemyType.Boss;
    public bool CanUseWeapons => config.canUseWeapons;
    public bool CanMelee => config.canMelee;
    public bool UsesIK => config.usesIK;

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

        switch (config.enemyType)
        {
            case EnemyType.MeleeOnly:
                stateMachine.RegisterState(new EnemyIdleState());
                stateMachine.RegisterState(new EnemyFindTargetState());
                stateMachine.RegisterState(new EnemyAttackMeleeState());
                stateMachine.RegisterState(new EnemyDeathState());
                break;

            case EnemyType.RangedOnly:
                stateMachine.RegisterState(new EnemyIdleState());
                stateMachine.RegisterState(new EnemyFindTargetState());
                stateMachine.RegisterState(new EnemyFindWeaponState());
                stateMachine.RegisterState(new EnemyAttackRangedState());
                stateMachine.RegisterState(new EnemyDeathState());
                break;

            case EnemyType.Boss:
                stateMachine.RegisterState(new EnemyIdleState());
                stateMachine.RegisterState(new EnemyFindTargetState());
                stateMachine.RegisterState(new BossTeleportState());
                stateMachine.RegisterState(new EnemyAttackTargetState()); // ranged
                stateMachine.RegisterState(new EnemyAttackMeleeState());  // melee
                stateMachine.RegisterState(new EnemyDeathState());
                break;

            default:
                stateMachine.RegisterState(new EnemyIdleState());
                stateMachine.RegisterState(new EnemyFindTargetState());
                stateMachine.RegisterState(new EnemyFindWeaponState());
                stateMachine.RegisterState(new EnemyAttackTargetState());
                stateMachine.RegisterState(new EnemyDeathState());
                break;
        }
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
        isInMelee = false;
        navMeshAgent.isStopped = false;
        navMeshAgent.ResetPath();
        if (weapons.weaponIK != null)
            weapons.weaponIK.LerpToWeight(0.8f, 0.2f);
        if (weapons != null && weapons.HasWeapon())
        {
            weapons.SetFiring(true);
        }
        if (IsBoss)
            return;
        stateMachine.ChangeState(EnemyStateID.FindTarget);
    }

    public void ShowTpPreview(Vector3 targetPos, float range)
    {
        if (tpPreview == null) return;

        Vector3 start = transform.position;
        Vector3 direction = (targetPos - start).normalized;
        Vector3 midpoint = start + direction * (range * 0.5f);

        tpPreview.transform.position = midpoint;
        tpPreview.transform.rotation = Quaternion.LookRotation(direction);
        tpPreview.transform.localScale = new Vector3(0.25f, 1f, range); // slim width, long length

        tpPreview.SetActive(true);
    }

    public void HideTpPreview()
    {
        if (tpPreview != null)
            tpPreview.SetActive(false);
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

    public void ShootProjectile(GameObject bulletPrefab, float speed, float spread = 0f)
    {
        if (!bulletPrefab || !firePoint) return;

        Quaternion spreadRot = Quaternion.Euler(0, UnityEngine.Random.Range(-spread, spread), 0);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * spreadRot);

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = firePoint.forward * speed;
        }

        // assign shooter so it doesn't hit self and can apply damage
        if (bullet.TryGetComponent<BulletProjectile>(out var projectile))
        {
            projectile.SetShooter(this.gameObject); // or add direct Enemy shooter logic
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
