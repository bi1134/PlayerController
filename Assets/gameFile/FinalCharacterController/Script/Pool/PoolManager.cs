using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private List<Pool> pools; // Assign pools in the Inspector
    private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();


    void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (Pool pool in pools)
        {
            // Create a parent object to hold pooled objects
            GameObject parentObject = new GameObject(pool.tag + " Pool");
            parentObject.transform.SetParent(transform);
            poolParents[pool.tag] = parentObject.transform;
        }

        ObjectPooler.InitializePools(pools, poolParents);
    }
}
