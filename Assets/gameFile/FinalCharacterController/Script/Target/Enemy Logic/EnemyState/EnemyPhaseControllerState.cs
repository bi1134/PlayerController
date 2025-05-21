using System.Collections;
using UnityEngine;

public class EnemyPhaseControllerState : EnemyState
{
    private float dashCooldownTimer;
    private float laserCooldownTimer;
    private int dashCount;
    private bool enraged;

    private const int maxDashCount = 2;
    private const float dashWindupDelay = 0.5f;
    private const float postDashStun = 0.3f;

    public EnemyStateID GetID() => EnemyStateID.PhaseController;

    public void Enter(Enemy enemy)
    {
        dashCooldownTimer = 0f;
        laserCooldownTimer = 0f;
        dashCount = 0;
        enraged = false;
        Debug.Log("[Boss] Entered phase controller");
    }

    public void Exit(Enemy enemy) { }

    public void Update(Enemy enemy)
    {
        if (!enemy.targeting.HasTarget) return;

        Vector3 targetPos = enemy.targeting.TargetPosition;
        float distance = Vector3.Distance(enemy.transform.position, targetPos);

        // Enrage
        if (!enraged && enemy.enemyStats.GetHealthPercent() < 0.5f && enemy.config.behaviorProfile.enragesAtHalfHP)
        {
            enraged = true;
            enemy.animator.SetTrigger("Enrage");
            Debug.Log("[Boss] Enraged!");
        }

        dashCooldownTimer -= Time.deltaTime;
        laserCooldownTimer -= Time.deltaTime;

        // Melee if close
        if (enemy.CanMelee && distance <= enemy.config.meleeRange && enemy.canMeleeAttack)
        {
            enemy.animator.SetTrigger("isAttacking");
            return;
        }

        // Dash if allowed and cooldown
        if (enemy.config.behaviorProfile.canDash && dashCooldownTimer <= 0f && dashCount < maxDashCount)
        {
            dashCount++;
            PoolRunner.Instance.RunCoroutine(DashTowardPlayer(enemy, targetPos));
            return;
        }

        // Reset dash count after cooldown
        if (dashCooldownTimer <= 0 && dashCount >= maxDashCount)
        {
            dashCount = 0;
            dashCooldownTimer = enemy.config.behaviorProfile.dashCooldown;
        }

        // Laser if far and cooldown
        if (enemy.config.behaviorProfile.hasLaserAttack && distance > enemy.config.meleeRange + 2 && laserCooldownTimer <= 0f)
        {
            laserCooldownTimer = enemy.config.behaviorProfile.laserCooldown;
            enemy.animator.SetTrigger("LaserAttack");
            PoolRunner.Instance.RunCoroutine(FireLaser(enemy));
            return;
        }

        // Default chase
        if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
        {
            enemy.navMeshAgent.isStopped = false;
            enemy.navMeshAgent.destination = targetPos;
        }
    }

    private IEnumerator DashTowardPlayer(Enemy enemy, Vector3 target)
    {
        Debug.Log("[Boss] preparing teleport...");

        // freeze movement
        if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
            enemy.navMeshAgent.isStopped = true;

        float prepareTime = enemy.config.behaviorProfile.dashPrepareTime;
        float trackDuration = prepareTime * 0.5f;
        float waitAfterTracking = prepareTime - trackDuration;

        // track position for half of the delay
        Vector3 trackedTargetPos = enemy.transform.position;
        float timer = 0f;

        while (timer < trackDuration)
        {
            if (enemy.targeting.HasTarget)
            {
                trackedTargetPos = enemy.targeting.TargetPosition;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        yield return Helpers.GetWaitForSecond(waitAfterTracking); // pause before teleporting

        // calculate destination
        Vector3 direction = (trackedTargetPos - enemy.transform.position).normalized;
        float dashDistance = 6f;
        Vector3 desiredPosition = enemy.transform.position + direction * dashDistance;

        // disable collision with player
        Collider[] bossCols = enemy.GetComponentsInChildren<Collider>();
        Collider[] playerCols = GameObject.FindWithTag("Player").GetComponentsInChildren<Collider>();
        foreach (var b in bossCols)
            foreach (var p in playerCols)
                Physics.IgnoreCollision(b, p, true);


        // teleport
        enemy.animator.SetTrigger("DashPrep");
        if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
            enemy.navMeshAgent.Warp(desiredPosition);
        else
            enemy.transform.position = desiredPosition;

        yield return Helpers.GetWaitForSecond(postDashStun);

        // re-enable collision
        foreach (var b in bossCols)
            foreach (var p in playerCols)
                Physics.IgnoreCollision(b, p, false);

        if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
            enemy.navMeshAgent.isStopped = false;
    }

    private IEnumerator FireLaser(Enemy enemy)
    {
        Debug.Log("[Boss] Preparing laser...");
        enemy.animator.SetTrigger("LaserPrep");

        yield return Helpers.GetWaitForSecond(enemy.config.behaviorProfile.laserPrepareTime);

        Debug.Log("[Boss] Firing bullets...");
        var bulletPrefab = enemy.config.behaviorProfile.bulletPrefab;
        float speed = enemy.config.behaviorProfile.bulletSpeed;
        int count = enemy.config.behaviorProfile.bulletCount;
        float spread = enemy.config.behaviorProfile.bulletSpreadAngle;

        for (int i = 0; i < count; i++)
        {
            enemy.ShootProjectile(bulletPrefab, speed, spread);
            yield return new WaitForSeconds(0.05f); // rapid fire delay
        }

        yield return Helpers.GetWaitForSecond(0.2f);
    }
}

