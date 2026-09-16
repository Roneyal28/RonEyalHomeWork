using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Armory/Scriptable Objects/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon ScriptableObject")]
    public string displayName = "Pistol";
    public WeaponTypeData weaponType;
    public BulletData bullet;
    [Min(0.05f)] public float shotDelay = 0.3f;

    [Header("3D Gun Prefab")]
    [Tooltip("Drag a gun prefab or an imported FBX model here.")]
    public GameObject gunPrefab;
    public Vector3 gunLocalRotation = Vector3.zero;
    public Vector3 gunLocalScale = Vector3.one;
    public Vector3 muzzleLocalPosition = new Vector3(0f, 0f, 1f);
}
