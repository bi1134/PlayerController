using System.Collections;
using UnityEngine;

public class PooledEffect : MonoBehaviour, IPooledObject
{
    private ParticleSystem effect;
    private string poolTag;

    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }

    public void SetPoolTag(string tag)
    {
        poolTag = tag; // Set the pool tag for this effect
    }

    public void OnObjectSpawn()
    {
        effect.Play(); // Play effect every time it's reused
        StartCoroutine(DisableAfterTime(effect.main.duration)); // Disable after duration
    }

    private IEnumerator DisableAfterTime(float time)
    {
        yield return Helpers.GetWaitForSecond(time);
        ObjectPooler.ReturnToPool(poolTag, gameObject);
    }
}