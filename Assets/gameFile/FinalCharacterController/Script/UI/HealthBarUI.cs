using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    #region Variables
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI healthText;
    public bool canShowHealthBar = false;

    #endregion

    #region StartUp
    private void Start()
    {
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
        barImage.fillAmount = 1;

        if (healthText)
        {
            healthText.text = healthSystem.currentHealth.ToString() + "/" + healthSystem.maxHealth.ToString();
        }


        this.gameObject.SetActive(canShowHealthBar);
    }

    #endregion

    #region Signal
    private void HealthSystem_OnHealthChanged(object sender, HealthSystem.OnHealthChangedEventArgs e)
    {
        this.gameObject.SetActive(true);
        barImage.fillAmount = e.healthNormalized;
        if (healthText)
        {
            healthText.text = healthSystem.currentHealth.ToString() + "/" + healthSystem.maxHealth.ToString();
        }
        if (barImage.fillAmount <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }
    #endregion
}
