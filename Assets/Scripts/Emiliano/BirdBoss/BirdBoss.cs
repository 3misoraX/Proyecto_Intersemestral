using NUnit.Framework;
using System.Collections;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BirdBoss : MonoBehaviour
{
    private Rigidbody rb;
    public Vector3 playerPos;
    private Animator animator;

    [Header("Dash Configuration")]
    public bool isMoving = false;
    public float dashTime = 1f;
    public int dashSpeed = 10;

    [Header("Shotgun Attack")]
    [SerializeField] private int bulletCount;
    [SerializeField] private GameObject bulletPrefab;
    public float bulletForce;
    public float dispersion;
    public Transform shootPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    
    public void FallingAttack()
    {
        animator.SetTrigger("Stop");
    }

    public void ShotgunAttack()
    {
        transform.LookAt(GameObject.FindWithTag("Player").transform.position);
        for(int i = 0; i < bulletCount; i++)
        {
            float angleX = Random.Range(-dispersion, dispersion);
            float angleZ = Random.Range(-dispersion, dispersion);

            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            Vector3 disperseDir = new Vector3(angleX, 0, angleZ).normalized;
            Vector3 finalDir = (disperseDir + 2*shootPoint.forward).normalized;

            bullet.gameObject.GetComponent<Rigidbody>().AddForce(finalDir * bulletForce, ForceMode.VelocityChange);
        }
    }
    
    //Moves by dashing towards the player
    public IEnumerator Dash(Vector3 moveDir)
    {
        isMoving = true;
        rb.linearVelocity = new Vector3(moveDir.x * dashSpeed, 0, moveDir.z * dashSpeed);
        yield return new WaitForSeconds(dashTime);
        isMoving = false;
    }
}

//subida y caida con daño en area