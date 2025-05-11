using System.Collections;
using UnityEngine;

public class PoolRunner : MonoBehaviour
{
    private static PoolRunner _instance;
    public static PoolRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance in the scene
                _instance = FindFirstObjectByType<PoolRunner>();

                if (_instance == null)
                {
                    var obj = new GameObject("PoolRunner");
                    _instance = obj.AddComponent<PoolRunner>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure singleton and destroy duplicates
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}