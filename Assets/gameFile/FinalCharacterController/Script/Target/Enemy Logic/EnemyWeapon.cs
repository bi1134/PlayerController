using System.Collections;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{

    //Components
    private Animator animator;
    private MeshSockets sockets;
    private WeaponIK weaponIK;

    private WeaponBase currentWeapon;
    private Transform currentTarget;
    private Vector3 target;

    //bool
    private bool weaponActive = false;
    private bool isFiring = false;

    //floats
    public float inaccuracy = 0.0f;

    #region Start Up
    private void Start()
    {
        animator = GetComponent<Animator>();
        sockets = GetComponent<MeshSockets>();
        weaponIK = GetComponent<WeaponIK>();
    }
    #endregion

    #region Update
    private void Update()
    {
        if(currentTarget && currentWeapon && weaponActive)
        {
            target = currentTarget.position + weaponIK.targetOffset;
            target += Random.insideUnitSphere * inaccuracy;
            currentWeapon.UpdateFiring(Time.deltaTime, target, isFiring);
        }
    }
    #endregion

    #region Fire Logic
    public void SetFiring(bool enabled)
    {
        isFiring = enabled;
        if(enabled)
        {
            currentWeapon.StartFiring(target);
        }
        else
        {
            currentWeapon.StopFiring();
        }
    }

    #endregion

    #region Equip and Drop Weapon Logic
    public void Equip(WeaponBase weapon)
    {
        currentWeapon = weapon;
        sockets.Attach(currentWeapon.transform, MeshSockets.SocketID.Spine);
    }

    public void ActivateWeapon()
    {
        StartCoroutine(EquipWeapon());
    }

    IEnumerator EquipWeapon()
    {
        animator.SetBool("Equip", true);
        yield return new WaitForSeconds(0.5f);
        while(animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
        {
            yield return null;
        }

        weaponIK.SetAimTransform(currentWeapon.bulletSpawnPosition);
        weaponActive = true;
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
    #endregion

    #region Set Target
    public void SetTarget(Transform target)
    {
        weaponIK.SetTargetTransform(target);
        currentTarget = target;
    }
    #endregion
}
