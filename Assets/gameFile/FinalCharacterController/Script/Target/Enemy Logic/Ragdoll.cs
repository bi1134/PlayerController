using System.Collections;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    private Rigidbody[] rigidbodies;
    private Animator animator;
    #endregion


    #region Start Up
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();

        DeActivateRagdoll();
    }

    #endregion

    #region Ragdoll Physics
    public void ActivateRagdoll()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
        
        animator.enabled = false;
    }
    
    public void DeActivateRagdoll()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        animator.enabled = true;
    }

    public void DisableCollider()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            Collider col = rb.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }
    }

    public void EnableCollider()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            Collider col = rb.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;
        }
    }

    public void DisableCollidersDelayed(float delay)
    {
        StartCoroutine(DisableAfterDelay(delay));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return Helpers.GetWaitForSecond(delay);
        DisableCollider();
    }

    public void ApplyForce(Vector3 force)
    {
        var rigidbody = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        rigidbody.AddForce(force, ForceMode.VelocityChange);
    }
    #endregion
}
