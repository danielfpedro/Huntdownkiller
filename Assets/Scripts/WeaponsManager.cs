using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    public Transform primaryWeaponsContainer;
    private int currentWeaponIndex = 0;
    private GunController currentGunController;

    public GunController CurrentWeapon
    {
        get { return currentGunController; }
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
}
