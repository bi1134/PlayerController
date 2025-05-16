using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PickupNotificationUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image icon;
    public float DisplayDuration = 2f;

    public Animator animator; // Optional for fade in/out

    public void Show(InventoryItemData data)
    {
        string rarityHex = RarityColorManager.Instance.GetColorHex(data.rarity);
        nameText.text = $"<color=#{rarityHex}>{data.ItemName}</color>";

        string coloredDescription = KeywordColorizer.Instance.ColorizeText(data.Description);
        descriptionText.text = coloredDescription;

        icon.sprite = data.Icon;
        gameObject.SetActive(true);

        if (animator != null)
            animator.SetTrigger("Show");

        PoolRunner.Instance.RunCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return Helpers.GetWaitForSecond(DisplayDuration);

        if (animator != null)
            animator.SetTrigger("Hide");

        yield return Helpers.GetWaitForSecond(0.5f); // if fade out is animated
        gameObject.SetActive(false);
    }
}
