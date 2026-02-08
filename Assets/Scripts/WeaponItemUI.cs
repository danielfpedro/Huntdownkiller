using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponItemUI : MonoBehaviour
{
    public GunController gunController;
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (gunController != null)
        {

            if (weaponIcon != null)
            {
                weaponIcon.sprite = gunController.icon;
            }
            if (ammoText != null)
            {
                if (gunController.TotalMagazines == -1)
                {
                    ammoText.text = $"{gunController.CurrentAmmo}/∞";
                }
                else
                {
                    int totalReserveAmmo = gunController.TotalMagazines * gunController.magazineSize;
                    ammoText.text = $"{gunController.CurrentAmmo}/{totalReserveAmmo}";
                }
            }
        }
    }
}
