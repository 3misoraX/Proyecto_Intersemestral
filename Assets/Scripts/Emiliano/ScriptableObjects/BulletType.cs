using UnityEngine;

[CreateAssetMenu(fileName = "BulletType", menuName = "Scriptable Objects/BulletType")]
public class BulletType : ScriptableObject
{
    public int dmg = 1;
    public float duration;///the flight time of the bullet
    //this remains to be decided because it depends on the transformations
    public char[] properties;///introduce the character for a certain property
}
