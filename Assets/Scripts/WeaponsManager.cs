using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    public Transform primaryWeaponsContainer;
    public Transform secondaryWeaponsContainer;

    private int currentWeaponIndex = 0;
    private int currentSecondaryWeaponIndex = 0;

    private GunController currentGunController;
    private GunController currentSecondaryGunController;

    public GunController CurrentWeapon
    {
        get { return currentGunController; }
    }

    public GunController CurrentSecondaryWeapon
    {
        get { return currentSecondaryGunController; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (primaryWeaponsContainer != null)
        {
            for (int i = 0; i < primaryWeaponsContainer.childCount; i++)
            {
                primaryWeaponsContainer.GetChild(i).gameObject.SetActive(false);
            }
            if (primaryWeaponsContainer.childCount > 0)
            {
                primaryWeaponsContainer.GetChild(currentWeaponIndex).gameObject.SetActive(true);
                currentGunController = primaryWeaponsContainer.GetChild(currentWeaponIndex).GetComponent<GunController>();
            }
        }

        if (secondaryWeaponsContainer != null)
        {
            for (int i = 0; i < secondaryWeaponsContainer.childCount; i++)
            {
                secondaryWeaponsContainer.GetChild(i).gameObject.SetActive(false);
            }
            if (secondaryWeaponsContainer.childCount > 0)
            {
                secondaryWeaponsContainer.GetChild(currentSecondaryWeaponIndex).gameObject.SetActive(true);
                currentSecondaryGunController = secondaryWeaponsContainer.GetChild(currentSecondaryWeaponIndex).GetComponent<GunController>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Next()
    {
        if (primaryWeaponsContainer == null || primaryWeaponsContainer.childCount <= 1) return;

        int prevIndex = currentWeaponIndex;
        currentWeaponIndex = (currentWeaponIndex + 1) % primaryWeaponsContainer.childCount;
        primaryWeaponsContainer.GetChild(prevIndex).gameObject.SetActive(false);
        primaryWeaponsContainer.GetChild(currentWeaponIndex).gameObject.SetActive(true);
        currentGunController = primaryWeaponsContainer.GetChild(currentWeaponIndex).GetComponent<GunController>();
    }

    public void NextSecondary()
    {
        if (secondaryWeaponsContainer == null || secondaryWeaponsContainer.childCount <= 1) return;

        int prevIndex = currentSecondaryWeaponIndex;
        currentSecondaryWeaponIndex = (currentSecondaryWeaponIndex + 1) % secondaryWeaponsContainer.childCount;
        secondaryWeaponsContainer.GetChild(prevIndex).gameObject.SetActive(false);
        secondaryWeaponsContainer.GetChild(currentSecondaryWeaponIndex).gameObject.SetActive(true);
        currentSecondaryGunController = secondaryWeaponsContainer.GetChild(currentSecondaryWeaponIndex).GetComponent<GunController>();
    }
}
