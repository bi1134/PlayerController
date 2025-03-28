using UnityEngine;

public class MeshSocket : MonoBehaviour
{
    public MeshSockets.SocketID socketID;
    public HumanBodyBones bone;

    public Vector3 offSet;
    public Vector3 rotation;

    Transform attachPoint;

    private void Start()
    {
        Animator animator = GetComponentInParent<Animator>();
        attachPoint = new GameObject("socket" + socketID).transform;
        attachPoint.SetParent(animator.GetBoneTransform(bone));
        attachPoint.localPosition = offSet;
        attachPoint.localRotation = Quaternion.Euler(rotation);
    }

    void Update()
    {
        
    }

    public void Attach(Transform objectTransform)
    {
        objectTransform.SetParent(attachPoint, false);
    }
}
