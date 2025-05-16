using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    public TextMeshProUGUI interactionText;
    [SerializeField] private Color keyColor = new Color(1f, 0.85f, 0f); // bright yellow

    public void Show(string verb, string target)
    {
        string keyHex = ColorUtility.ToHtmlStringRGB(keyColor);
        string targetHex = keyHex;

        string formatted = $"Press <color=#{keyHex}>E</color> to {verb} <color=#{targetHex}>{target}</color>";
        interactionText.text = formatted;
        gameObject.SetActive(true);
    }

    public void ShowPickup(string verb, InventoryItemData itemData)
    {
        string keyHex = ColorUtility.ToHtmlStringRGB(keyColor);
        string rarityHex = RarityColorManager.Instance.GetColorHex(itemData.rarity);
        string formatted = $"Press <color=#{keyHex}>E</color> to {verb} <color=#{rarityHex}>{itemData.ItemName}</color>";

        interactionText.text = formatted;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
