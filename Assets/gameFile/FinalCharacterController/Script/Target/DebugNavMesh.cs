using UnityEngine;
using UnityEngine.AI;

public class DebugNavMesh : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    
    public bool velocity;
    public bool desiredVelocity;
    public bool path;


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnDrawGizmos()
    {
        if(velocity)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + navMeshAgent.velocity);
        }

        if(desiredVelocity)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + navMeshAgent.desiredVelocity);
        }

        if (path)
        {
            Gizmos.color = Color.black;
            var agentPath = navMeshAgent.path;
            Vector3 prevCorner = transform.position;

            foreach (var corner in agentPath.corners)
            {
                Gizmos.DrawLine(prevCorner, corner);
                Gizmos.DrawSphere(corner, 0.1f);
                prevCorner = corner;
            }
        }
    }
}
