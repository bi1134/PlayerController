using System.Collections.Generic;
using UnityEngine;

public class MeshSocket : MonoBehaviour
{
    public MeshSockets.SocketID socketID;
    public HumanBodyBones bone;

    public List<WeaponOffsetProfile> offsetProfiles;

    [SerializeField] private Transform attachPoint;

    private void Start()
    {
        if (attachPoint != null) return;
        Animator animator = GetComponentInParent<Animator>();
        attachPoint = new GameObject("socket" + socketID).transform;
        attachPoint.SetParent(animator.GetBoneTransform(bone));
        attachPoint.localPosition = Vector3.zero;
        attachPoint.localRotation = Quaternion.identity;
    }

    void Update()
    {
        
    }

    public void Attach(Transform objectTransform, WeaponName weaponName)
    {
        WeaponOffsetProfile profile = offsetProfiles.Find(p => p.weaponName == weaponName);
        if (profile == null)
        {
            Debug.LogWarning($"No offset profile found for {weaponName}, defaulting to zero.");
            profile = new WeaponOffsetProfile();
        }

        objectTransform.SetParent(attachPoint);
        objectTransform.localPosition = profile.positionOffset;
        objectTransform.localRotation = Quaternion.Euler(profile.rotationOffset);
    }
}
