using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Task 2/Character Data")]
public class CharacterData : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public Sprite icon;
}
