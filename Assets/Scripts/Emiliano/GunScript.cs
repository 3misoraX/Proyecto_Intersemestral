using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference switchAction;
    [SerializeField] private Transform shootPoint;
    public List<TransformationObject> transformations = new List<TransformationObject>();
    [SerializeField] private TransformationObject activeTransformation;
    public GameObject bulletPrefab;
    public float bulletForce;
    private float cooldown = 0;

    void Start()
    {
        activeTransformation = transformations[0];
    }

    void Update()
    {
        ShootingManager();
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
                    GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
                    projectile.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce, ForceMode.VelocityChange);
                }
                break;
            case 'a':
                //auto prefab
                if (shootAction.action.IsPressed() == true && cooldown <= 0)
                {
                    GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
                    projectile.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce, ForceMode.VelocityChange);
                    cooldown = 1 / activeTransformation.cadency;
                }
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
        //This function should change the active object, which should be a scriptable object
    }
}