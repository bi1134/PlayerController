using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBehaviorProfile", menuName = "Scriptable Objects/EnemyBehaviorProfile")]
public class EnemyBehaviorProfile : ScriptableObject
{
    public bool canDash;
    public bool canSummonMinions;
    public float dashSpeed;
    public float dashCooldown;
    public bool enragesAtHalfHP;
}
