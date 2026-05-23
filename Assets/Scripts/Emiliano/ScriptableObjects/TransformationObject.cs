using UnityEngine;

[CreateAssetMenu(fileName = "Transformation", menuName = "Scriptable Objects/Transformation")]
public class TransformationObject : ScriptableObject
{
    public char weaponType; ///s for single-prefab, a for auto-prefab, r for single-raycast, l for auto-raycast
    public float cadency; /// the higher the number the lower the time between bullets, only avaliable for auto modes
    public char specialAbility;///introduce the initial of the transformation that grants it
    public char superAbility;///introduce the initial of the transformation that grants it
    public int superCharges;
    private int currentCharge;
    public BulletType bulletType;///for prefab bullets and their abilities
}
