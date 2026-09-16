using UnityEngine;

[CreateAssetMenu(fileName = "NewBullet", menuName = "Armory/Scriptable Objects/Bullet")]
public class BulletData : ScriptableObject
{
    [Header("Bullet ScriptableObject")]
    public Bullet prefab;
    [Min(1)] public int damage = 25;
    [Min(0.1f)] public float speed = 14f;
    [Min(0.1f)] public float range = 18f;
    public Sprite sprite;
    public Color color = Color.white;
    public Vector2 size = new Vector2(0.32f, 0.12f);
}
