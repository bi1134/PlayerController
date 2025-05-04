using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Enemy Finding and Chasing")]
    public float maxTime = 2f;
    public float maxDistance = 1.0f;
    public float findWeaponSpeed = 5.0f;
    public float findTargetSpeed = 5.0f;
    public float chaseTargetSpeed = 3.0f;
    public float wanderRadius = 20f;

    [Header("Enemy Dying")]
    public float ragdollForce = 10;

    [Header("Enemy Idling")]
    public float maxSightDistance = 5.0f;

    [Header("Enemy Attacking range")]
    public float meleeRange = 2f;
    public float gunRange = 10f;
    public float weaponLostRange = 20f;
}
