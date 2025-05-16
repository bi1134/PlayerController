using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RarityColor
{
    public Rarity rarity;
    public Color color;
}

public class RarityColorManager : MonoBehaviour
{
    public static RarityColorManager Instance;

    [Header("Rarity Colors")]
    public List<RarityColor> rarityColors;

    private Dictionary<Rarity, Color> rarityColorDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rarityColorDict = new Dictionary<Rarity, Color>();
        foreach (var entry in rarityColors)
        {
            if (!rarityColorDict.ContainsKey(entry.rarity))
                rarityColorDict.Add(entry.rarity, entry.color);
        }
    }

    public Color GetColor(Rarity rarity)
    {
        return rarityColorDict.TryGetValue(rarity, out var color) ? color : Color.white;
    }

    public string GetColorHex(Rarity rarity)
    {
        Color color = GetColor(rarity);
        return ColorUtility.ToHtmlStringRGB(color);
    }
}
