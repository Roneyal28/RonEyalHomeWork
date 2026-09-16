using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("ScriptableObject Weapon Assets")]
    [Tooltip("Each entry is a WeaponData ScriptableObject asset.")]
    public WeaponData[] weapons;
    public PoolManager pool;
    public Transform muzzle;
    public Camera aimCamera;
    public int SelectedIndex { get; private set; }
    public WeaponData CurrentWeapon => weapons[SelectedIndex];
    public int ShotsFired { get; private set; }
    private float nextShotTime;
    private bool mouseWasHeld;
    private GameObject currentGunModel;

    private void Start()
    {
        Application.targetFrameRate = 60;
        var oldSprite = transform.Find("Gun Image");
        if (oldSprite != null) oldSprite.gameObject.SetActive(false);

        var preparedPrefabs = new System.Collections.Generic.HashSet<Bullet>();
        foreach (var weapon in weapons)
        {
            if (preparedPrefabs.Add(weapon.bullet.prefab))
                pool.Prewarm(weapon.bullet.prefab, pool.PreMadeBulletCount);
        }
        SelectWeapon(0);
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        SelectedIndex = index;
        var weapon = CurrentWeapon;

        if (currentGunModel != null)
        {
            currentGunModel.SetActive(false);
            Destroy(currentGunModel);
            currentGunModel = null;
        }

        if (weapon.gunPrefab != null)
        {
            currentGunModel = Instantiate(weapon.gunPrefab, transform);
            currentGunModel.name = weapon.gunPrefab.name;
            currentGunModel.transform.localPosition = Vector3.zero;
            currentGunModel.transform.localEulerAngles = weapon.gunLocalRotation;
            currentGunModel.transform.localScale = weapon.gunLocalScale;
            muzzle.localPosition = weapon.muzzleLocalPosition;
        }
    }

    public void AimAtScreen(Vector2 screenPosition)
    {
        Ray ray = aimCamera.ScreenPointToRay(screenPosition);
        // First-person aim: the gun shoots through the mouse cursor.
        transform.rotation = Quaternion.LookRotation(ray.direction, Vector3.up);
    }

    private void LateUpdate()
    {
        // Run after the UI's Update so its mouse-over state is current.
        var mouse = Mouse.current;
        if (mouse == null) return;
        bool held = mouse.leftButton.isPressed;
        bool pressed = held && !mouseWasHeld;
        mouseWasHeld = held;
        // Clicking a weapon button must not also shoot a bullet.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        AimAtScreen(mouse.position.ReadValue());
        bool wantsToFire = CurrentWeapon.weaponType.fireMode == FireMode.Automatic
            ? held : pressed;
        if (wantsToFire) TryFire();
    }

    public bool TryFire()
    {
        if (Time.time < nextShotTime) return false;
        var weapon = CurrentWeapon;
        var type = weapon.weaponType;
        nextShotTime = Time.time + weapon.shotDelay;
        int count = type.fireMode == FireMode.Spread ? Mathf.Max(1, type.projectilesPerShot) : 1;
        for (int i = 0; i < count; i++)
        {
            float angle = count == 1 ? 0f : Mathf.Lerp(-type.spreadAngle / 2f, type.spreadAngle / 2f, i / (float)(count - 1));
            var rotation = transform.rotation * Quaternion.Euler(0f, angle, 0f);
            var bullet = (Bullet)pool.Get(weapon.bullet.prefab, muzzle.position, rotation);
            bullet.Launch(weapon.bullet);
        }
        ShotsFired++;
        return true;
    }
}
