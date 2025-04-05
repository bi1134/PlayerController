using System;
using UnityEngine;

public enum WeaponType { Gun, Melee, Bow, Magic }
public enum WeaponName { Pistol, Rifle, Shotgun, Sniper, Sword, BowAndArrow, Staff }
[CreateAssetMenu(fileName = "WeaponPropertiesSO", menuName = "Scriptable Objects/WeaponPropertiesSO")]
public class WeaponPropertiesSO : ScriptableObject
{
    public WeaponName weaponName;

    [Header("Gun Stats")]
    public float damage;
    public float fireRate;
    public float reloadTime;
    public int magazineSize;
    public int bulletsPerTap;

    [Header("Projectile Stats (Only for Projectile Weapons)")]
    public float bulletSpeed;
    public float bulletLifetime;
    public float spread;
    public float upwardForce;
}
