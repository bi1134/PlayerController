using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image barImage;

    private void Start()
    {
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
        barImage.fillAmount = 1;
        this.gameObject.SetActive(false);
    }

    private void HealthSystem_OnHealthChanged(object sender, HealthSystem.OnHealthChangedEventArgs e)
    {
        this.gameObject.SetActive(true);
        barImage.fillAmount = e.healthNormalized;
        if (barImage.fillAmount <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }
}
