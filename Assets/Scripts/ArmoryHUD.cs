using UnityEngine;
using UnityEngine.UI;

public class ArmoryHUD : MonoBehaviour
{
    public WeaponController weapon;
    public Text weaponLabel;
    public Button[] weaponButtons;
    private int lastWeapon = -1;

    private void Start()
    {
        for (int i = 0; i < weaponButtons.Length; i++)
        {
            int index = i;
            weaponButtons[i].GetComponentInChildren<Text>().text = weapon.weapons[i].displayName;
            weaponButtons[i].onClick.AddListener(() => weapon.SelectWeapon(index));
        }
    }

    private void Update()
    {
        if (lastWeapon != weapon.SelectedIndex)
        {
            lastWeapon = weapon.SelectedIndex;
            var data = weapon.CurrentWeapon;
            weaponLabel.text = data.displayName;
            for (int i = 0; i < weaponButtons.Length; i++)
                weaponButtons[i].image.color = i == lastWeapon ? new Color(0.1f, 0.65f, 0.65f) : new Color(0.19f, 0.25f, 0.31f);
        }
    }
}
