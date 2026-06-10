using UnityEngine;

[CreateAssetMenu(fileName = "BulletType", menuName = "Scriptable Objects/BulletType")]
public class BulletType : ScriptableObject
{
    public int dmg = 1;
    public float duration;///the flight time of the bullet
    public float abilityDuration;
    //this remains to be decided because it depends on the transformations
    public string[] properties;///introduce the character for a certain property
}
