using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] private AnimationCurve experienceCurve;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image experienceBar;

    private int currentLevel;
    private int lastLevel;
    private float currentExp;
    private float nextLevelExp;

    private void Start()
    {
        currentLevel = playerStats.baseStats.level;
        currentExp = playerStats.baseStats.exp;
        UpdateExpThreshold();
        UpdateUI();
    }

    public void AddExperience(float amount)
    {
        currentExp += amount;
        playerStats.baseStats.exp = currentExp;

        while (currentExp >= nextLevelExp)
        {
            currentExp -= nextLevelExp;
            lastLevel = currentLevel;
            currentLevel++;
            playerStats.baseStats.level = currentLevel;

            playerStats.OnLevelUp(currentLevel, lastLevel); // Stats scale here
            UpdateExpThreshold();
        }

        UpdateUI();
    }

    private void UpdateExpThreshold()
    {
        nextLevelExp = experienceCurve.Evaluate(currentLevel + 1);
    }

    private void UpdateUI()
    {
        levelText.text = $"LVL {currentLevel}";
        experienceBar.fillAmount = currentExp / nextLevelExp;
    }
}