using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBehaviorProfile", menuName = "Scriptable Objects/EnemyBehaviorProfile")]
public class EnemyBehaviorProfile : ScriptableObject
{
    public bool canDash;
    public bool dashIsTeleport;
    public float dashDistance = 6f;
    public float dashSpeed = 20f;
    public float dashCooldown = 5f;
    public float dashPrepareTime = 0.5f;
    [Range(0f, 1f)]
    public float tpTrackPercentage = 0.75f; // time to track player
    [Range(0f, 1f)]
    public float tpReactPercentage = 0.25f;

    public bool canSummonMinions;
    public bool enragesAtHalfHP;

    public bool hasLaserAttack;
    public float laserCooldown = 8f;
    public float laserDuration = 2f;
    public float laserPrepareTime = 0.4f;

    [Header("Projectile")]
    public GameObject bulletPrefab; 
    public float bulletSpeed = 10f;
    public int bulletCount = 5;
    public float bulletSpreadAngle = 15f;
}
