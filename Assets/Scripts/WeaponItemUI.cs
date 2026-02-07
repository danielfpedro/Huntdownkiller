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
                ammoText.text = $"{gunController.CurrentAmmo}/{gunController.TotalAmmo}";
            }
        }
    }
}
