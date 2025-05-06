using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseItemData : MonoBehaviour
{
    [SerializeField] public Image ItemSprite;
    [SerializeField] public TextMeshProUGUI ItemCount;

    private void Awake()
    {
        ItemSprite.color = Color.clear;
        ItemCount.text = string.Empty;
    }
}
