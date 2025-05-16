using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class KeywordColor
{
    public string keyword;
    public Color color;
}

public class KeywordColorizer : MonoBehaviour
{
    public static KeywordColorizer Instance;

    [Header("Keyword Colors")]
    public List<KeywordColor> keywordColors;

    private Dictionary<string, string> colorTagLookup = new Dictionary<string, string>();
    private string positivePattern = @"(?<!\w)\+[\d%\.]+(?!\w)";
    private string negativePattern = @"(?<![\w\-])-[\d%\.]+(?!\w)";
    private string colorHexPositive, colorHexNegative;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var entry in keywordColors)
        {
            string hex = ColorUtility.ToHtmlStringRGB(entry.color);
            colorTagLookup[entry.keyword.ToLower()] = $"<color=#{hex}>{entry.keyword}</color>";
        }

        colorHexPositive = ColorUtility.ToHtmlStringRGB(GetColor("positive"));
        colorHexNegative = ColorUtility.ToHtmlStringRGB(GetColor("negative"));
    }

    private Color GetColor(string key)
    {
        if (key == "positive" && colorTagLookup.TryGetValue("+", out var colorTag))
            ColorUtility.TryParseHtmlString(colorTag, out var cPos);
        if (key == "negative" && colorTagLookup.TryGetValue("-", out var colorTag2))
            ColorUtility.TryParseHtmlString(colorTag2, out var cNeg);

        // Fallbacks
        return key == "positive" ? Color.green : Color.red;
    }
    public string ColorizeText(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Protect text in (...) blocks
        string protectedInput = Regex.Replace(input, @"\((.*?)\)", match =>
        {
            return $"<noparse>{match.Value}</noparse>";
        });

        // Color +number or -number
        protectedInput = Regex.Replace(protectedInput, positivePattern,
            match => $"<color=#{colorHexPositive}>{match.Value}</color>");

        protectedInput = Regex.Replace(protectedInput, negativePattern,
            match => $"<color=#{colorHexNegative}>{match.Value}</color>");

        // Keyword replacements (avoids hyphenated words)
        foreach (var pair in colorTagLookup)
        {
            string wordPattern = $@"(?<![\w\-]){Regex.Escape(pair.Key)}(?![\w\-])";
            protectedInput = Regex.Replace(protectedInput, wordPattern, pair.Value, RegexOptions.IgnoreCase);
        }

        // Restore ( ... ) blocks and remove <noparse> markers
        protectedInput = Regex.Replace(protectedInput, @"<noparse>\((.*?)\)</noparse>", match =>
        {
            return match.Groups[1].Value; // just put back as-is with no styling
        });

        return protectedInput;
    }
}
