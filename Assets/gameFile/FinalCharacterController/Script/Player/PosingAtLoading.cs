using UnityEngine;

public class PosingAtLoading : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public bool crouch, sit, lay, happy, bored = false;

    private void Awake()
    {
        animator.SetBool("Crouch", crouch);
        animator.SetBool("Sit", sit);
        animator.SetBool("Lay", lay);
        animator.SetBool("Happy", happy);
        animator.SetBool("Bored", bored);
    }
}
