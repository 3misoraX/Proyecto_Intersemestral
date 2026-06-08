using UnityEngine;
using System.Collections;

public class BirdBoss : MonoBehaviour
{
    private Rigidbody rb;
    public Vector3 playerPos;
    [Header("Dash Configuration")]
    public bool isMoving = false;
    public float dashTime = 1f;
    public int dashSpeed = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Siempre debera estar a cierta distancia del jugador
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
//escopeta como la de monstro