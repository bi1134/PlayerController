using UnityEngine;

public enum EnemyType
{
    Default,
    MeleeOnly,
    RangedOnly,
    Flying,
    Quadruped,
    Elite,
    Boss
}


[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("General Settings")]
    public string poolTag; 
    public EnemyType enemyType = EnemyType.Default;
    public EnemyBehaviorProfile behaviorProfile;

    [Tooltip("Whether this enemy can pick up and use weapons.")]
    public bool canUseWeapons = true;

    [Tooltip("Whether this enemy has melee capabilities.")]
    public bool canMelee = true;

    [Tooltip("Whether this enemy uses weapon IK aiming.")]
    public bool usesIK = true;

    [Header("Movement Speeds")]
    public float findWeaponSpeed = 5.0f;
    public float findTargetSpeed = 5.0f;
    public float chaseTargetSpeed = 3.0f;
    public float chaseAttackTargetSpeed = 2.0f;

    [Header("Combat Ranges")]
    [Tooltip("Range at which melee attack becomes active.")]
    public float meleeRange = 2f;

    [Tooltip("Range at which ranged attack becomes active.")]
    public float gunRange = 10f;

    [Tooltip("If weapon is beyond this range, enemy considers it lost.")]
    public float weaponLostRange = 20f;

    [Header("AI Vision and Awareness")]
    public float maxSightDistance = 5.0f;
    public float wanderRadius = 20f;

    [Header("Death Settings")]
    [Tooltip("Force applied to ragdoll parts on death.")]
    public float ragdollForce = 10f;

    [Tooltip("Time to remember last seen target.")]
    public float maxTime = 2f;

    [Tooltip("Distance to forget last seen target.")]
    public float maxDistance = 1.0f;
}
