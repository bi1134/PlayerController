using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }


    private void Update()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(navMeshAgent.velocity);
        float forwardSpeed = localVelocity.z;
        animator.SetFloat(inputYHash, forwardSpeed);
        animator.SetFloat(inputMagnitudeHash, navMeshAgent.velocity.magnitude);
    }
}
