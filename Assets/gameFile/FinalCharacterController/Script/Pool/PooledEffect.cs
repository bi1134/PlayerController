using System.Collections;
using UnityEngine;

public class PooledEffect : MonoBehaviour, IPooledObject
{
    private ParticleSystem effect;

    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }

    public void OnObjectSpawn()
    {
        effect.Play(); // Play effect every time it's reused
        StartCoroutine(DisableAfterTime(effect.main.duration)); // Disable after duration
    }

    private IEnumerator DisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}