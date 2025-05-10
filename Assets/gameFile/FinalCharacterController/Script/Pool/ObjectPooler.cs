using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;              // Unique identifier for the pool
    public GameObject prefab;       // Prefab to be pooled
    public int size;                // Initial pool size
}

public static class ObjectPooler
{
    private static Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private static Dictionary<string, GameObject> poolPrefabs = new Dictionary<string, GameObject>();
    private static Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();

    /// <summary>
    /// Initializes the object pools.
    /// </summary>
    public static void InitializePools(List<Pool> pools, Dictionary<string, Transform> parents)
    {
        poolParents = parents;
        foreach (Pool pool in pools)
        {

            if (poolDictionary.ContainsKey(pool.tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{pool.tag}' already exists!");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Object.Instantiate(pool.prefab);
                obj.SetActive(false);

                if (poolParents.ContainsKey(pool.tag))
                {
                    obj.transform.SetParent(poolParents[pool.tag]);
                }

                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            poolPrefabs.Add(pool.tag, pool.prefab);
        }
    }

    /// <summary>
    /// Spawns an object from the pool at the given position and rotation.
    /// </summary>
    public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            throw new System.Exception($"[ObjectPooler] ERROR: Pool with tag '{tag}' does not exist!");
        }

        Queue<GameObject> objectPool = poolDictionary[tag];

        // Check if there's an available object
        if (objectPool.Count == 0 || objectPool.Peek().activeInHierarchy)
        {
            ExpandPool(tag);
        }

        GameObject objectToSpawn = objectPool.Dequeue();
        if (poolPrefabs[tag].TryGetComponent<UnityEngine.AI.NavMeshAgent>(out _))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(position, out var hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                position = hit.position;
            }
            else
            {
                Debug.LogWarning("Spawn position not on NavMesh. Skipping spawn.");
                poolDictionary[tag].Enqueue(objectToSpawn);
                return null;
            }

            // Make sure NavMeshAgent is off before moving
            var agent = objectToSpawn.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        if (objectToSpawn.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var nav))
        {
            nav.enabled = true; // Enable agent *after* snapping to navmesh
        }

        objectToSpawn.SetActive(true);

        // Call OnObjectSpawn() if the object implements IPooledObject
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        return objectToSpawn;
    }


    /// <summary>
    /// Returns an object back to the pool.
    /// </summary>
    public static void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' does not exist!");
            return;
        }

        // Reset Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset Particle Systems
        foreach (var ps in obj.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Clear();
            ps.Stop();
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    public static bool PoolExists(string tag)
    {
        return poolDictionary.ContainsKey(tag);
    }

    public static int PoolCount(string tag)
    {
        if (!poolDictionary.ContainsKey(tag)) return 0;
        return poolDictionary[tag].Count;
    }


    /// <summary>
    /// Expands the pool dynamically if needed.
    /// </summary>
    private static void ExpandPool(string tag)
    {

        if (!poolPrefabs.ContainsKey(tag))
        {
            throw new System.Exception($"[ObjectPooler] ERROR: Cannot expand. No prefab found for tag '{tag}'!");
        }

        Debug.Log($"[ObjectPooler] Expanding pool '{tag}' (Current count: {poolDictionary[tag].Count})");
        GameObject obj = Object.Instantiate(poolPrefabs[tag]);
        obj.SetActive(false);

        if (poolParents.ContainsKey(tag))
        {
            obj.transform.SetParent(poolParents[tag]);
        }

        poolDictionary[tag].Enqueue(obj);

        Debug.Log($"[ObjectPooler] Pool with tag '{tag}' expanded!");
    }
}
