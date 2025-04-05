using System;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponBase weaponPrefab;

    private void OnTriggerEnter(Collider other)
    {
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon)
        {
            WeaponBase newWeapon = Instantiate(weaponPrefab);
            activeWeapon.EquipWeapon(newWeapon);
            Destroy(gameObject);
        }

        EnemyWeapon enemyWeapon = other.gameObject.GetComponent<EnemyWeapon>();
        if (enemyWeapon)
        {
            WeaponBase newWeapon = Instantiate(weaponPrefab);
            enemyWeapon.Equip(newWeapon);
            Destroy(gameObject);
        }
    }
}
