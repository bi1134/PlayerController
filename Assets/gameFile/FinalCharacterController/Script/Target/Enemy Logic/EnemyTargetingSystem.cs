using UnityEngine;

[ExecuteInEditMode]
public class EnemyTargetingSystem : MonoBehaviour
{
    public float memorySpan, distanceWeight, angleWeight, ageWeight = 3.0f;


    public bool HasTarget 
    {
        get 
        { 
            return bestMemory != null; 
        } 
    }

    public GameObject Target => bestMemory != null ? bestMemory.gameObject : null;
    public Vector3 TargetPosition => bestMemory != null ? bestMemory.position : Vector3.zero;

    public bool IsTargetInSight
    {
        get 
        {
            return bestMemory.Age < 0.5f; //seconds
        }
    }

    public float TargetDistance
    {
        get 
        {
            return bestMemory.distance;
        }
    }

    EnemySensorMemory memory = new EnemySensorMemory(10);
    EnemySensor sensor;
    EnemyMemory bestMemory;

    private void Start()
    { 
        sensor = GetComponent<EnemySensor>();
    }

    // Update is called once per frame
    private void Update()
    {
        memory.UpdateSenses(sensor);
        memory.ForgetMemories(memorySpan);
        EvaluateScores();
    }

    private void EvaluateScores()
    {
        bestMemory = null;

        foreach (var memory in memory.memories)
        {
            memory.score = CalculateScore(memory);
            if(bestMemory == null || memory.score > bestMemory.score)
            {
                bestMemory = memory;
            }
        }
    }

    private float Normalize(float value, float maxValue)
    {
        return 1.0f - (value / maxValue);
    }

    private float CalculateScore(EnemyMemory memory)
    {
        //rough 0 to 1 range for distance score
        float distanceScore = Normalize(memory.distance, sensor.distance) * distanceWeight;
        float angleScore = Normalize(memory.angle, sensor.angle) * angleWeight;
        float ageScore = Normalize(memory.Age, memorySpan) * ageWeight;

        return distanceScore + angleScore;
    }

    private void OnDrawGizmos()
    {
        float maxScore = float.MinValue;
        foreach(var memory in memory.memories)
        {
            maxScore = Mathf.Max(maxScore, memory.score);
            if (maxScore <= 0.0001f) maxScore = 1f;
        }

        foreach(var memory in memory.memories)
        {
            Color color = Color.red;
            if(memory == bestMemory)
            {
                color = Color.yellow;
            }
            color.a = memory.score / maxScore;
            Gizmos.color = color;
            Gizmos.DrawSphere(memory.position, 0.2f);
        }
    }

    public void InjectMemory(GameObject target)
    {
        memory.RefreshMemory(gameObject, target);
    }
}
