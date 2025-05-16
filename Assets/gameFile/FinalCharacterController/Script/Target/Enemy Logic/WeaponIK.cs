using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}

public class WeaponIK : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private float weightLerpSpeed = 5f;

    [Range(0,1)]
    public float weight = 1.0f;
    private float targetWeight = 1.0f;

    public int iterations = 10;
    public float angleLimit = 90.0f;
    public float distanceLimit = 1.5f;
    public Vector3 targetOffset;

    public HumanBone[] humanBones;
    private Transform[] boneTransforms;

    private void Start()
    {
        Animator animator = GetComponent<Animator>();
        boneTransforms = new Transform[humanBones.Length];
        for (int i = 0; i < boneTransforms.Length; i++ )
        {
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);
        }
    }
    #region Late Update
    private void LateUpdate()
    {
        if (aimTransform == null || targetTransform == null)
            return;

        if (TryGetComponent<Enemy>(out var enemy) && enemy.isDead)
            return;

        Vector3 directionToTarget = targetTransform.position - transform.position;
        directionToTarget.y = 0f; // Prevent looking up/down
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth turning
        }

        Vector3 targetPosition = GetTargetPosition();
        for (int i = 0; i < iterations; i++)
        {
            for (int b = 0; b < boneTransforms.Length; b++)
            {
                Transform bone = boneTransforms[b];
                weight = Mathf.Lerp(weight, targetWeight, Time.deltaTime * weightLerpSpeed);
                float boneWeight = humanBones[b].weight * weight;
                AimAtTarget(bone, targetPosition, boneWeight);
            }
        }
    }

    #endregion

    #region Functions

    private void AimAtTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        Quaternion blendedRotation = Quaternion.Lerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetDirection = (targetTransform.position + targetOffset) - aimTransform.position;
        Vector3 aimDirection = aimTransform.forward;
        float blendOut = 0.0f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if(targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50.0f;
        }

        float targetDistance = targetDirection.magnitude;
        if(targetDistance < distanceLimit)
        {
            blendOut += distanceLimit - targetDistance;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return aimTransform.position + direction;
    }

    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
    }

    public void SetAimTransform(Transform target)
    {
        aimTransform = target;
    }

    public void SetWeight(float value)
    {
        targetWeight = Mathf.Clamp01(value);
    }

    public void LerpToWeight(float target, float duration)
    {
        PoolRunner.Instance.RunCoroutine(LerpIKWeight(target, duration));
    }

    private IEnumerator LerpIKWeight(float target, float duration)
    {
        float start = weight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetWeight(Mathf.Lerp(start, target, t));
            yield return null;
        }

        SetWeight(target);
    }
    #endregion
}
