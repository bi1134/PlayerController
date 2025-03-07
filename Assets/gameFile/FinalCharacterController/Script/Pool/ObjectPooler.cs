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
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

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

        obj.SetActive(false);

        // Don't reparent to the pool parent, just keep it inside
        poolDictionary[tag].Enqueue(obj);
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
