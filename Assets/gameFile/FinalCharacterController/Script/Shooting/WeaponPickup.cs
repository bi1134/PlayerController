using NUnit.Framework.Interfaces;
using System;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponRaycast weaponPrefab;

    private void OnTriggerEnter(Collider other)
    {
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon)
        {
            WeaponRaycast newWeapon = Instantiate(weaponPrefab);
            activeWeapon.EquipWeapon(newWeapon);
            Destroy(gameObject);
        }

        EnemyWeapon enemyWeapon = other.gameObject.GetComponent<EnemyWeapon>();
        if (enemyWeapon)
        {
            WeaponRaycast newWeapon = Instantiate(weaponPrefab);
            enemyWeapon.Equip(newWeapon);
            Destroy(gameObject);
        }
    }
}
