using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    public GameObject trailPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SpawnTrail(Vector3 from, Vector3 to)
    {
        if (!trailPrefab) return;

        GameObject trail = Instantiate(trailPrefab, from, Quaternion.LookRotation(to - from));
        TrailRenderer tr = trail.GetComponent<TrailRenderer>();
        if (tr)
        {
            tr.Clear();
            tr.emitting = true;
        }

        PoolRunner.Instance.RunCoroutine(MoveTrail(trail, from, to));
        Destroy(trail, 2f);
    }

    private IEnumerator MoveTrail(GameObject obj, Vector3 from, Vector3 to)
    {
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = to;
    }
}
