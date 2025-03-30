using System.Collections;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    #region Variables
    public float inacuracy = 0f;

    private WeaponRaycast currentWeapon;
    private Animator animator;
    private MeshSockets sockets;
    private WeaponIK weaponIK;
    private Transform currentTarget;
    private bool weaponActive = false;
    private Vector3 target;

    #endregion

    #region Start Up
    private void Awake()
    {
        animator = GetComponent<Animator>();
        sockets = GetComponent<MeshSockets>();
        weaponIK = GetComponent<WeaponIK>();
    }

    #endregion

    #region Updates

    private void Update()
    {
        if(currentTarget && currentWeapon && weaponActive) //if all 3 of these are true then start blasting
        {
            target = currentTarget.position + weaponIK.targetOffset;
            target += Random.insideUnitSphere * inacuracy;
            currentWeapon.UpdateFiring(Time.deltaTime, target);
        }
    }

    #endregion

    #region Functions

    public void SetFiring(bool enabled)
    {
        if (enabled)
        {
            currentWeapon.StartFiring(target);
        }
        else
        {
            currentWeapon.StopFiring();
        }
    }
    public void Equip(WeaponRaycast weapon)
    {
        currentWeapon = weapon;
        sockets.Attach(currentWeapon.transform, MeshSockets.SocketID.Spine);
    }

    public void DeactivateWeapon()
    {
        SetTarget(null);
        SetFiring(false);
        StartCoroutine(HolsterWeapon());
    }

    IEnumerator HolsterWeapon()
    {
        weaponActive = false;
        animator.SetBool("Equip", false);
        yield return new WaitForSeconds(0.5f);
        while (animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
        {
            yield return null;
        }

        weaponIK.SetAimTransform(currentWeapon.bulletSpawnPosition);
    }


    public void ActivateWeapon()
    {
        StartCoroutine(EquipWeapon());
    }

    IEnumerator EquipWeapon()
    {
        weaponActive = true;
        animator.SetBool("Equip", true);
        yield return new WaitForSeconds(0.5f);
        while(animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
        {
            yield return null;
        }

        weaponIK.SetAimTransform(currentWeapon.bulletSpawnPosition);
    }

    public void DropWeapon()
    {
        if(currentWeapon)
        {
            currentWeapon.transform.SetParent(null);
            currentWeapon.gameObject.GetComponent<BoxCollider>().enabled = true;
            currentWeapon.gameObject.AddComponent<Rigidbody>();
            currentWeapon = null;
        }
    }

    public bool HasWeapon()
    {
        return currentWeapon != null;
    }

    public void OnAnimationEvent(string eventName)
    {
        if(eventName == "equipWeapon")
        {
            sockets.Attach(currentWeapon.transform, MeshSockets.SocketID.RightHand);
        }
    }

    public void SetTarget(Transform target)
    {
        weaponIK.SetTargetTransform(target);
        currentTarget = target;
    }
    #endregion
}
