using UnityEngine;
using UnityEngine.Events;

public class HolsterEventRelay : MonoBehaviour
{
    // Reference to the ActiveWeapon script on the parent
    private ActiveWeapon activeWeapon;

    // UnityEvents to relay animation events
    public UnityEvent onHolsterComplete;
    public UnityEvent onEquipComplete;

    private void Start()
    {
        // Get the ActiveWeapon component from the parent
        activeWeapon = GetComponentInParent<ActiveWeapon>();

        if (activeWeapon == null)
        {
            Debug.LogError("[HolsterEventRelay] ActiveWeapon not found on parent!");
        }
    }

    // Called by Animation Event
    public void TriggerHolsterComplete()
    {
        onHolsterComplete?.Invoke();
    }

    // Called by Animation Event
    public void TriggerEquipComplete()
    {
        onEquipComplete?.Invoke();
    }
}
