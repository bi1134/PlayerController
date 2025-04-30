using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Enemy Chasing")]
    public float maxTime = 2f;
    public float maxDistance = 1.0f;

    [Header("Enemy Dying")]
    public float ragdollForce = 10;

    [Header("Enemy Idling")]
    public float maxSightDistance = 5.0f;

    [Header("Enemy Attacking range")]
    public float meleeRange = 2f;
    public float gunRange = 10f;
    public float weaponLostRange = 20f;

    [Header("Enemy stats")]
    public float health = 100f;
    public float meleeDamage = 10f;
}
