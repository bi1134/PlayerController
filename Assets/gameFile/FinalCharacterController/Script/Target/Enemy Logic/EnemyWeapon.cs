using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    WeaponRaycast currentWeapon;
    Animator animator;
    MeshSockets sockets;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sockets = GetComponent<MeshSockets>();
    }

    public void Equip(WeaponRaycast weapon)
    {
        currentWeapon = weapon;
        sockets.Attach(currentWeapon.transform, MeshSockets.SocketID.Spine);
    }

    public void ActivateWeapon()
    {
        animator.SetBool("Equip", true);
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

}
