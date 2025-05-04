using UnityEngine;

[CreateAssetMenu(fileName = "ItemsEffect", menuName = "Scriptable Objects/ItemsEffect")]
public class ItemEffectConfig : ScriptableObject
{
    [Header("Basic Info")]
    public string effectName;

    [Tooltip("What event triggers this effect?")]
    public EffectTrigger trigger;

    [Tooltip("What action does the item take when triggered?")]
    public EffectAction action;

    [Header("Effect Settings")]
    [Tooltip("Main value of the effect. E.g. damage, healing amount, or stat multiplier.")]
    public float value = 0f;

    [Tooltip("Cooldown in seconds before this effect can activate again.")]
    public float cooldown = 0f;

    [Tooltip("Prefab to spawn if action is SpawnPrefab.")]
    public GameObject spawnPrefab;

    [Tooltip("Chance (0-1) this effect will trigger when conditions are met.")]
    [Range(0f, 1f)]
    public float chanceToProc = 1f;

    [Tooltip("Which stat this affects (e.g. MoveSpeed, AttackDamage). Leave 'None' if irrelevant.")]
    public StatType targetStat = StatType.None;

    [Tooltip("For effects with a duration (like DoT, buffs, shields).")]
    public float duration = 0f;

    [Tooltip("For execute-type effects: the enemy must be below this % HP.")]
    [Range(0f, 1f)]
    public float maxTriggerHealthThreshold = 1f;
}

public enum EffectTrigger
{
    OnPickup,
    OnHitEnemy,
    OnTakeDamage,
    OnKillEnemy,
    OnJump,
    PassiveTick
}

public enum StatType { None, MoveSpeed, BonusDamage, CritChance, MaxHealth }

public enum EffectAction
{
    DealDamage,
    Heal,
    SpawnPrefab,
    ModifyStat,
    ExecuteEnemy
}

