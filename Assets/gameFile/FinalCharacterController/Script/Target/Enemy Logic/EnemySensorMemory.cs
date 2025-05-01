using System.Collections.Generic;
using UnityEngine;

public class EnemyMemory
{
    public float Age
    {
        get
        {
            return Time.time - lastSeen;
        }
    }

    public GameObject gameObject;

    public Vector3 position, direction;
    public float distance, angle, lastSeen, score;
}

public class EnemySensorMemory 
{
    public List<EnemyMemory> memories = new List<EnemyMemory>();
    GameObject[] players;

    public EnemySensorMemory(int maxPlayers)
    {
        players = new GameObject[maxPlayers];
    }

    public void UpdateSenses(EnemySensor sensor)
    {
        int targets = sensor.Filter(players, "Player");
        for (int i = 0; i < targets; i++)
        {
            GameObject target = players[i];
            RefreshMemory(sensor.gameObject, target);
        }
    }

    public void RefreshMemory(GameObject enemy, GameObject target)
    {
        EnemyMemory memory = FetchMemory(target);
        memory.gameObject = target;
        memory.position = target.transform.position;
        memory.direction = target.transform.position - enemy.transform.position;
        memory.distance = memory.direction.magnitude;
        memory.angle = Vector3.Angle(enemy.transform.forward, memory.direction);
        memory.lastSeen = Time.time;
    }

    public EnemyMemory FetchMemory(GameObject gameObject)
    {
        EnemyMemory memory = memories.Find(x => x.gameObject == gameObject);

        if (memory == null)
        {
            memory = new EnemyMemory
            {
                gameObject = gameObject  
            };
            memories.Add(memory);
        }

        return memory;
    }

    public void ForgetMemories(float olderThan)
    {
        memories.RemoveAll(m =>
        {
            if (m == null || m.gameObject == null || m.Age > olderThan)
                return true;

            var health = m.gameObject.GetComponent<HealthSystem>();
            return health == null || health.IsDead();
        });
    }
}
