using UnityEngine;

public class MeshSocket : MonoBehaviour
{
    public MeshSockets.SocketID socketID;
    Transform attachPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        attachPoint = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attach(Transform objectTransform)
    {
        objectTransform.SetParent(attachPoint, false);
    }
}
