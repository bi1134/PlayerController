using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPropertiesSO", menuName = "Scriptable Objects/WeaponPropertiesSO")]
public class WeaponPropertiesSO : ScriptableObject
{
    public string weaponName;
    public float damage;
    public int fireRate;
    public int bulletSpeed;
    public WeaponSlot weaponSlot;
    public WeaponDamageType damageType;
}
