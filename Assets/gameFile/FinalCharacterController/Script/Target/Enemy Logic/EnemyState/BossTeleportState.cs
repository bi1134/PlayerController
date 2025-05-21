using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossTeleportState : EnemyState
{
    public EnemyStateID GetID() => EnemyStateID.BossTeleport;

    public void Enter(Enemy enemy)
    {

        enemy.navMeshAgent.isStopped = false;

        PoolRunner.Instance.RunCoroutine(Teleport(enemy));
    }

    public void Exit(Enemy enemy)
    {

        enemy.navMeshAgent.isStopped = false;
    }

    public void Update(Enemy enemy)
    {
    }

    private IEnumerator Teleport(Enemy enemy)
    {
        float prepTime = enemy.config.behaviorProfile.dashPrepareTime;
        float trackTime = prepTime * enemy.config.behaviorProfile.tpTrackPercentage;
        float reactTime = prepTime * enemy.config.behaviorProfile.tpReactPercentage;

        Vector3 trackedPos = enemy.transform.position;

        float elapsed = 0f;
        while (elapsed < trackTime)
        {
            if (enemy.targeting.HasTarget)
            {
                trackedPos = enemy.targeting.TargetPosition;

                // Look at player
                Vector3 lookDir = trackedPos - enemy.transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Quaternion rot = Quaternion.LookRotation(lookDir);
                    enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, rot, Time.deltaTime * 8f);
                }
            }
            enemy.ShowTpPreview(trackedPos, prepTime);

            elapsed += Time.deltaTime;
            yield return null;
        }
        enemy.animator.SetBool("DashPrep", true);
        yield return Helpers.GetWaitForSecond(reactTime); // fixed reaction window

        // Final direction
        Vector3 direction = (trackedPos - enemy.transform.position);
        direction.y = 0f;
        direction = direction.normalized;

        float dashDistance = enemy.config.behaviorProfile.dashDistance;
        Vector3 targetPosition = enemy.transform.position + direction * dashDistance;

        float capsuleRadius = 0.4f;
        float capsuleHeight = 1.8f;

        Vector3 origin = enemy.transform.position + Vector3.up * (capsuleHeight * 0.5f);
        Vector3 end = origin + Vector3.up * (capsuleHeight - capsuleRadius * 2);

        // Wall check
        int blockLayers = LayerMask.GetMask("Default", "Ground");
        if (Physics.CapsuleCast(origin, end, capsuleRadius, direction, out RaycastHit wallHit, dashDistance, blockLayers))
        {
            targetPosition = wallHit.point - direction * 0.5f;
        }
        else
        {
            targetPosition = enemy.transform.position + direction * dashDistance;
        }

        // NavMesh validation
        bool foundNav = NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, 1.5f, NavMesh.AllAreas);
        if (!foundNav || Vector3.Distance(navHit.position, targetPosition) > 2f)
        {
            targetPosition = enemy.transform.position; // cancel teleport
        }
        else
        {
            targetPosition = navHit.position;

            // Ground check only if on navmesh
            if (Physics.Raycast(targetPosition + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit, 5f))
            {
                float slope = Vector3.Angle(groundHit.normal, Vector3.up);
                if (slope < 45f)
                {
                    targetPosition = groundHit.point;
                }
            }
        }

        // Damage player if passed through them
        int playerLayer = LayerMask.GetMask("Player");
        Vector3 castStart = enemy.transform.position + Vector3.up * 0.5f;
        Vector3 castEnd = castStart + Vector3.up;
        RaycastHit[] hits = Physics.CapsuleCastAll(castStart, castEnd, 0.5f, direction, dashDistance, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                var health = hit.collider.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(enemy.enemyStats.baseStats.baseDamage, direction);
                }
            }
        }

        // Visuals
        VFXManager.Instance?.SpawnTrail(enemy.transform.position, targetPosition);

        // Teleport
        if (enemy.navMeshAgent.enabled && enemy.navMeshAgent.isOnNavMesh)
        { 
            enemy.navMeshAgent.Warp(targetPosition);
            enemy.navMeshAgent.ResetPath(); 
        }
        else
            enemy.transform.position = targetPosition;

        yield return new WaitForSeconds(0.3f);

        enemy.animator.SetBool("DashPrep", false);
        enemy.HideTpPreview();

        // Next state
        if (enemy.CanUseWeapons)
            enemy.stateMachine.ChangeState(EnemyStateID.AttackTarget);
        else if (enemy.CanMelee)
            enemy.stateMachine.ChangeState(EnemyStateID.AttackMelee);
        else
            enemy.stateMachine.ChangeState(EnemyStateID.FindTarget);
    }
}
