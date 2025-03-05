using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private List<Pool> pools; // Assign pools in the Inspector

    void Start()
    {
        ObjectPooler.InitializePools(pools);
    }
}
