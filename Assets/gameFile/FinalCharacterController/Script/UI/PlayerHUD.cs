using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoDisplay;

    public void UpdateAmmo(int bulletsLeft, int magazineSize, int bulletsPerTap)
    {
        ammoDisplay.text = $"{bulletsLeft / bulletsPerTap} / {magazineSize / bulletsPerTap}";
    }
}
