using System.Collections.Generic;
using UnityEngine;

public class EnemyCorpseTracker
{
    public static readonly Queue<GameObject> ActiveCorpses = new Queue<GameObject>();
    public static int MaxAllowedCorpses = 35;

    public static void Register(GameObject corpse)
    {
        ActiveCorpses.Enqueue(corpse);
        if (ActiveCorpses.Count > MaxAllowedCorpses)
        {
            var oldest = ActiveCorpses.Dequeue();
            if (oldest != null && oldest.activeInHierarchy)
            {
                ObjectPooler.ReturnToPool("Enemy", oldest);
            }
        }
    }

    public static void Clear()
    {
        ActiveCorpses.Clear();
    }
}