using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference specialAction;
    [SerializeField] private InputActionReference switchAction;
    [SerializeField] private Transform shootPoint;
    public GameObject bulletPrefab;
    public float bulletForce;
    public float bulletDuration;
    
    // Update is called once per frame
    void Update()
    {
        if (shootAction.action.triggered)
        {
            GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce, ForceMode.VelocityChange);
            Destroy(projectile, bulletDuration);
        }
    }
}
