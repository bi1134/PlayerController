using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    #region Variables
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image barImage;

    #endregion

    #region StartUp
    private void Start()
    {
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
        barImage.fillAmount = 1;
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Signal
    private void HealthSystem_OnHealthChanged(object sender, HealthSystem.OnHealthChangedEventArgs e)
    {
        this.gameObject.SetActive(true);
        barImage.fillAmount = e.healthNormalized;
        if (barImage.fillAmount <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }
    #endregion
}
