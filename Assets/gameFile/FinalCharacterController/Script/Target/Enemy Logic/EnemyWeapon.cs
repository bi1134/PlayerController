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
    private float fireCooldown = 0f;
    public float enemyFireRate = 1f; // shots per second

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
        if (currentWeapon && weaponActive && currentWeapon.IsAmmoEmpty() && !currentWeapon.reloading)
        {
            currentWeapon.Reload();
        }

        if(currentTarget && currentWeapon && weaponActive && !currentWeapon.reloading)
        {
            fireCooldown -= Time.deltaTime;

            if (isFiring && fireCooldown <= 0f)
            {
                target = currentTarget.position + weaponIK.targetOffset;
                target += Random.insideUnitSphere * inaccuracy;

                currentWeapon.StartFiring(target);
                currentWeapon.UpdateFiring(Time.deltaTime, target, true);
                fireCooldown = 1f / enemyFireRate;
            }
        }
    }
    #endregion

    #region Fire Logic
    public void SetFiring(bool enabled)
    {
        isFiring = enabled;

        if (!currentWeapon)
            return;

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
        if (currentWeapon != null)
        {
            currentWeapon.OnReloadStarted -= HandleEnemyReloadStart;
        }

        currentWeapon = weapon;
        sockets.Attach(currentWeapon.transform, MeshSockets.SocketID.Spine, currentWeapon.weaponProperties.weaponName);

        currentWeapon.OnReloadStarted += HandleEnemyReloadStart;
    }

    public void ActivateWeapon()
    {
        if (!currentWeapon || weaponActive)
            return;

        StartCoroutine(EquipWeapon());
    }

    IEnumerator EquipWeapon()
    {
        if (!currentWeapon)
        {
            Debug.LogError("EquipWeapon: currentWeapon is null!");
            yield break;
        }

        if (!weaponIK)
        {
            Debug.LogError("EquipWeapon: weaponIK is null!");
            yield break;
        }

        if (!currentWeapon.bulletSpawnPosition)
        {
            Debug.LogError("EquipWeapon: bulletSpawnPosition is null in " + currentWeapon.name);
            yield break;
        }

        animator.runtimeAnimatorController = currentWeapon.animator;
        animator.SetBool("Equip", true);

        yield return Helpers.GetWaitForSecond(0.5f);

        while (animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
        {
            yield return null;
        };

        weaponIK.SetAimTransform(currentWeapon.bulletSpawnPosition);
        StartCoroutine(LerpIKWeight(1.0f, 0.25f));
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

        if (animator == null)
        {
            Debug.LogWarning("Animator is missing on EnemyWeapon.");
            yield break;
        }

        animator.SetBool("Equip", false);
        yield return Helpers.GetWaitForSecond(0.5f);

        if (animator.layerCount <= 1)
        {
            Debug.LogWarning("Animator does not have layer 1.");
            yield break;
        }

        while (animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
        {
            yield return null;
        }

        StartCoroutine(LerpIKWeight(0.0f, 0.25f));
        weaponIK.SetAimTransform(currentWeapon?.bulletSpawnPosition);
    }

    public void DropWeapon()
    {
        if(currentWeapon)
        {
            currentWeapon.OnReloadStarted -= HandleEnemyReloadStart;
            weaponIK.weight = 0.0f;
            weaponIK.SetWeight(0.0f);
            StartCoroutine(LerpIKWeight(0.0f, 0.25f));
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
            sockets.Attach(currentWeapon.transform, MeshSockets.SocketID.RightHand, currentWeapon.weaponProperties.weaponName);
        }
    }

    private void HandleEnemyReloadStart()
    {
        animator.SetTrigger("isReloading"); 
    }

    #endregion

    #region Set Target
    public void SetTarget(Transform target)
    {
        weaponIK.SetTargetTransform(target);
        currentTarget = target;
    }

    private IEnumerator LerpIKWeight(float target, float duration)
    {
        float start = weaponIK.weight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weaponIK.SetWeight(Mathf.Lerp(start, target, t));
            yield return null; // Keep this as `null` for per-frame update
        }

        weaponIK.SetWeight(target);
    }


    #endregion
}
