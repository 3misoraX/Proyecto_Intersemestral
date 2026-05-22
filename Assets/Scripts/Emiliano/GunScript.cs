using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    [SerializeField] private InputActionReference shootAction;
    //uncomment this function if you end up adding the UseSpecial() function
    //[SerializeField] private InputActionReference specialAction;
    [SerializeField] private InputActionReference switchAction;
    [SerializeField] private Transform shootPoint;
    public GameObject activeAbility;
    public GameObject bulletPrefab;
    public float bulletForce;
    public float bulletDuration;


    void Update()
    {
        //prefab shooting
        if (shootAction.action.triggered)
        {
            GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce, ForceMode.VelocityChange);
            Destroy(projectile, bulletDuration);
        }
        //could add raycast shooting with certain weapons
    }

    void ChangeWeapons()
    {
        //This function should change the active object, which should be a scriptable object
    }

    /*  not sure if this will be added
    void UseSpecial()
    {
        This function should use the special marked in a scriptable object
    }
    */
}