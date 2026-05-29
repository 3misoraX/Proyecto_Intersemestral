using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    //inputs
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference switchAction;
    public InputActionReference specialAction;
    public InputActionReference superAction;
    //shooting and changing weapons
    [SerializeField] private Transform shootPoint;
    public List<TransformationObject> transformations = new List<TransformationObject>();
    private TransformationObject activeTransformation;
    public GameObject bulletPrefab;
    public float bulletForce;
    private float cooldown = 0;
    [SerializeField] private int current = 0;
    //specials
    [SerializeField] private RoomController currentRoom;
    public int superCharges;
    public int superMaxCharges;
    public float specialCooldown;

    //Checks for default weapon
    void Start()
    {
        activeTransformation = transformations[current];
    }

    void Update()
    {
        //function for shooting
        ShootingManager();

        //switches transformation if the switch button is pressed
        if (switchAction.action.triggered)
        {
            ChangeWeapons();
        }

        //Calling the current special or super, never both
        if (specialAction.action.triggered && specialCooldown >= 5)
        {
            CallSpecial(this.gameObject, false);
            cooldown = 0;
        }
        else if (specialAction.action.triggered && superCharges == superMaxCharges)
        {
            CallSpecial(this.gameObject,true);
            superCharges = 0;
        }

        if(specialCooldown < 5)
        {
            specialCooldown += Time.deltaTime;
        }
    }

    //Function that manages shooting
    //s for a single shot
    //a for an auto prefab shot
    //r for a single raycast shot
    //l for an auto raycast shot
    void ShootingManager()
    {
        switch (activeTransformation.weaponType)
        {
            case 's':
                //single prefab
                if (shootAction.action.triggered)
                {
                    //instantiates a bullet
                    GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
                    //changes the bullet type acording to its transformation
                    projectile.GetComponent<BulletScript>().bulletType = activeTransformation.bulletType;
                    //bullet goes pium pium
                    projectile.GetComponent<Rigidbody>().AddForce(shootPoint.forward * bulletForce, ForceMode.VelocityChange);
                }
                break;
            case 'a':
                //auto prefab
                //same bs but automatic (i think)
                if (shootAction.action.IsPressed() == true && cooldown <= 0)
                {
                    GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
                    projectile.GetComponent<BulletScript>().bulletType = activeTransformation.bulletType;
                    projectile.GetComponent<Rigidbody>().AddForce(shootPoint.forward *bulletForce, ForceMode.VelocityChange);
                    cooldown = 1 / activeTransformation.cadency;
                }

                //auto weapon cooldown
                if(cooldown > 0)
                cooldown -= Time.deltaTime;
                break;
            case 'r':
                //single raycast
                break;
            case 'l':
                //auto raycast
                break;
        }
    }

    //Will cycle through the weapons pressing the switch button
    void ChangeWeapons()
    {
        current++;
        if(current >= transformations.Count)
        {
            current = 0;
        }
        activeTransformation = transformations[current];
    }

    void CallSpecial(GameObject target, bool isSuper)
    {
        //when triggering the special button, this will call the corresponding special
        target.SendMessage(activeTransformation.transformationName, isSuper, SendMessageOptions.DontRequireReceiver);
    }

    public void AddCharge()
    {
        //when completing a room this should add a charge
        if (superCharges < superMaxCharges)
        {
            //adds a charge to the current special
            superCharges++;
        }
        //animation for adding a charges
    }
}