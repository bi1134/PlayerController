using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseStatsSO", menuName = "Scriptable Objects/BaseStatsSO")]
public class BaseStatsSO : ScriptableObject
{
    public float maxHealth = 100;
    public float baseDamage = 10;
    public float moveSpeed = 5;
    public float critChance = 0.1f;
    public int level = 1;
    public float exp = 0;
    public int money = 0;
}
