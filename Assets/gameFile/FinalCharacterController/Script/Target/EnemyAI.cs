using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform target;

    public float maxTime = 2f;
    public float maxDistance = 1.0f;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private float timer = 0.1f;

    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }


    private void Update()
    {
        timer -= Time.deltaTime;
        if (target != null)
        {
            if (timer <= 0f)
            { 
                float sqrtDistance = (target.position - navMeshAgent.destination).sqrMagnitude;
                if(sqrtDistance > maxDistance * maxDistance)
                {
                    navMeshAgent.destination = target.position;
                }
                timer = maxTime;
            }
        }
        animator.SetFloat(inputYHash, navMeshAgent.velocity.y);
        animator.SetFloat(inputMagnitudeHash, navMeshAgent.velocity.magnitude);
    }
    private void LateUpdate()
    {
        
    }
}
