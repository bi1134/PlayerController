using System.Collections;
using UnityEngine;

public class EnemyPooled : MonoBehaviour, IPooledObject
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public void OnObjectSpawn()
    {
        StopAllCoroutines();
        StartCoroutine(DeferredReset());
    }

    private IEnumerator DeferredReset()
    {
        yield return null; // wait one frame to allow disable/setup
        enemy.ResetState();
    }

    public void OnDeath()
    {
        // Delay only for ragdoll visual effect
        PoolRunner.Instance.RunCoroutine(DelayedReturnToPool());
    }

    private IEnumerator DelayedReturnToPool()
    {
        yield return Helpers.GetWaitForSecond(60f); // Wait before forcibly pooling

        // NOTE: Don't check killed count. Rely on EnemyCorpseTracker to handle limit.
        if (!gameObject.activeInHierarchy) yield break;

        ObjectPooler.ReturnToPool("Enemy", gameObject);
    }
}
