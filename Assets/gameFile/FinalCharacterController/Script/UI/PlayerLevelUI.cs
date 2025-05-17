using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelUI : MonoBehaviour
{
    [Header("Experience")]
    [SerializeField] AnimationCurve experienceCurve;

    int currentLevel, totalExperience, previousExperience, nextLevelExperience;

    [Header("Reference")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image exprerienceBar;

    private void Start()
    {
        UpdateLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddExperence(5);
        }
    }

    public void AddExperence(int amount)
    {
        totalExperience += amount;
        CheckForLevelUp();
        UpdateInterface();
    }

    private void CheckForLevelUp()
    {
        if (totalExperience >= nextLevelExperience)
        {
            currentLevel++;
            previousExperience = nextLevelExperience;
            UpdateLevel();
        }

        exprerienceBar.fillAmount = (float)(totalExperience - previousExperience) / (nextLevelExperience - previousExperience);
    }

    private void UpdateLevel()
    {
        previousExperience = (int)experienceCurve.Evaluate(currentLevel);
        nextLevelExperience = (int)experienceCurve.Evaluate(currentLevel + 1);
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        int start = totalExperience - previousExperience;
        int end = nextLevelExperience - previousExperience;
        levelText.text = "LVL " + currentLevel.ToString();
        exprerienceBar.fillAmount = (float)(start) / (end);
    }
}
