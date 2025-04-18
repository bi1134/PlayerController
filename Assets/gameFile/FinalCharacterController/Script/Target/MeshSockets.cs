using System.Collections.Generic;
using UnityEngine;

public class MeshSockets : MonoBehaviour
{
    public enum SocketID
    {
        Spine,
        RightHand
    }

    Dictionary<SocketID, MeshSocket> socketMap = new Dictionary<SocketID, MeshSocket>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshSocket[] sockets = GetComponentsInChildren<MeshSocket>();
        foreach (var socket in sockets)
        {
            socketMap[socket.socketID] = socket;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Attach(Transform objectTransform, SocketID socketId, WeaponName weaponName)
    {
        socketMap[socketId].Attach(objectTransform, weaponName);
    }
}
