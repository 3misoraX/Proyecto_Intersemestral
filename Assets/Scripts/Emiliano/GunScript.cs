using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    //inputs
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference switchAction;
    //shooting and changing weapons
    [SerializeField] private Transform shootPoint;
    public List<TransformationObject> transformations = new List<TransformationObject>();
    private TransformationObject activeTransformation;
    public GameObject bulletPrefab;
    public float bulletForce;
    private float cooldown = 0;
    [SerializeField] private int current = 0;

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
    }

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

    void ChangeWeapons()
    {
        current++;
        if(current >= transformations.Count)
        {
            current = 0;
        }
        else
        {
            activeTransformation = transformations[current];
        }
    }
}