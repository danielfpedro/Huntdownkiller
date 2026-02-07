using UnityEngine;
using UnityEngine.UI;

public class WeaponsUI : MonoBehaviour
{
    public Transform uiContainer;
    public GameObject weaponItemPrefab;
    public Vector2 currentWeaponSize;
    public Vector2 otherWeaponsSize;
    public GameObject target;

    private WeaponsManager weaponsManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target != null)
        {
            weaponsManager = target.GetComponent<WeaponsManager>();
            if (weaponsManager != null)
            {
                weaponsManager.onWeaponChanged.AddListener(PopulateWeaponIcons);
            }
        }
        PopulateWeaponIcons();
    }

    private void OnDestroy()
    {
        if (weaponsManager != null)
        {
            weaponsManager.onWeaponChanged.RemoveListener(PopulateWeaponIcons);
        }
    }

    public void PopulateWeaponIcons()
    {
        if (weaponsManager == null || uiContainer == null || weaponItemPrefab == null) return;

        // Clear existing children
        foreach (Transform child in uiContainer)
        {
            Destroy(child.gameObject);
        }

        // Primary weapons
        if (weaponsManager.primaryWeaponsContainer != null)
        {
            for (int i = 0; i < weaponsManager.primaryWeaponsContainer.childCount; i++)
            {
                GameObject weaponGO = weaponsManager.primaryWeaponsContainer.GetChild(i).gameObject;
                GunController gunController = weaponGO.GetComponent<GunController>();

                if (gunController != null)
                {
                    GameObject itemGO = Instantiate(weaponItemPrefab, uiContainer);
                    WeaponItemUI itemUI = itemGO.GetComponent<WeaponItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.gunController = gunController;
                    }

                    RectTransform itemRT = itemGO.GetComponent<RectTransform>();
                    if (itemRT != null)
                    {
                        itemRT.sizeDelta = (i == weaponsManager.GetCurrentWeaponIndex()) ? currentWeaponSize : otherWeaponsSize;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
