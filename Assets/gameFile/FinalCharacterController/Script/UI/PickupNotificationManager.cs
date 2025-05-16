using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupNotificationManager : MonoBehaviour
{
    public static PickupNotificationManager Instance;

    [Header("References")]
    public PickupNotificationUI[] pooledNotifications; // Assign manually in editor
    public float delayBetween = 0.2f;

    private Queue<InventoryItemData> queue = new Queue<InventoryItemData>();
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        foreach (var ui in pooledNotifications)
            ui.gameObject.SetActive(false);
    }

    public void EnqueuePickup(InventoryItemData item)
    {
        queue.Enqueue(item);
        if (!isShowing)
        {
            PoolRunner.Instance.RunCoroutine(ShowNext());
        }
    }

    private IEnumerator ShowNext()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            InventoryItemData item = queue.Dequeue();

            // Find an inactive one
            PickupNotificationUI next = System.Array.Find(pooledNotifications, ui => !ui.gameObject.activeSelf);
            if (next == null)
            {
                Debug.LogWarning("All pickup UIs are busy!");
                yield return Helpers.GetWaitForSecond(delayBetween);
                continue;
            }

            next.Show(item); // Fill in sprite/text/animation
            yield return Helpers.GetWaitForSecond(next.DisplayDuration); // Wait for current to end
        }

        isShowing = false;
    }
}
