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
                var obj = new GameObject("PoolRunner");
                _instance = obj.AddComponent<PoolRunner>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}