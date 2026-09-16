using UnityEngine;

public enum FireMode { Single, Automatic, Spread }

[CreateAssetMenu(fileName = "NewWeaponType", menuName = "Armory/Scriptable Objects/Weapon Type")]
public class WeaponTypeData : ScriptableObject
{
    [Header("Weapon Type ScriptableObject")]
    public FireMode fireMode;
    [Min(1)] public int projectilesPerShot = 1;
    [Range(0f, 90f)] public float spreadAngle;
}
