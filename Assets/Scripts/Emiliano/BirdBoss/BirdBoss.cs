using System.Collections;
using UnityEngine;

public class BirdBoss : MonoBehaviour
{
    [Header("General")]
    private Rigidbody rb;
    public int dmg = 1;
    [SerializeField] private int hp;
    public int maxHp = 60;

    [Header("Dash Configuration")]
    public bool isMoving = false;
    public float dashTime = 1f;
    public int dashSpeed = 10;

    [Header("Falling Attack")]
    public float rangex;
    public float rangez;
    public GameObject boulder;
    public float explosionRange = 3f;
    private bool falling = false;
    private Transform player;

    [Header("Shotgun Attack")]
    [SerializeField] private int bulletCount;
    [SerializeField] private GameObject bulletPrefab;
    public float bulletForce;
    public float dispersion;
    public Transform shootPoint;

    [Header("Referencias 2D y Visuales")]
    public Transform spriteGraphic; // El GameObject hijo que tiene el SpriteRenderer y el Animator
    public SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform mainCamera;
    public GameObject explosionEffect;
    public float effectLifetime = 0.3f;
    public GameObject fallIndicator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        hp = maxHp;
        player = GameObject.FindWithTag("Player").transform;

        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró una Main Camera para el billboarding.");
        }
    }

    
    private void FixedUpdate()
    {
        Vector3 currentDirection = Vector3.zero;

        if (!isMoving)
        {
            transform.LookAt(player.position);
        }

        currentDirection = (player.position - transform.position).normalized;

        // Voltear el sprite dependiendo de si va a la izquierda o derecha
        if (currentDirection.x != 0)
        {
            spriteRenderer.flipX = currentDirection.x < 0;
        }
    }

    void LateUpdate()
    {
        // Billboarding: Hacer que el sprite siempre mire a la cámara
        if (spriteGraphic != null && mainCamera != null)
        {
            // Esto hace que el plano del sprite sea paralelo a la pantalla
            spriteGraphic.forward = mainCamera.forward;
        }
    }
    

    public void FallingAttack()
    {
        transform.Translate(new Vector3(transform.position.x, transform.position.y+30, transform.position.z));

        for(int i = 0; i < 3; i++)
        {
            Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-rangex, rangex), 10, transform.position.z + Random.Range(-rangez, rangez));
            GameObject rock = Instantiate(boulder, spawnPos, Quaternion.identity);
            Destroy(rock, 2f);
        }

        Vector3 fallPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.position = fallPos;
        GameObject indicator = Instantiate(fallIndicator, new Vector3(fallPos.x, 1, fallPos.z), Quaternion.Euler(90, 0, 0));
        Destroy(indicator, 2.5f);
        falling = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.contactCount > 0 && falling)
        {
            falling = false;
            Collider[] coll = Physics.OverlapSphere(transform.position, explosionRange);
            foreach(Collider col in coll)
            {
                if (col.CompareTag("Player"))
                {
                    col.GetComponent<PlayerHeallth>().LoseHealth(dmg);
                    
                }
            }

            if(explosionEffect != null)
            {
                GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effect, effectLifetime);
            }

            animator.SetTrigger("Stop");
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHeallth>().LoseHealth(dmg);
        }
    }

    public void ShotgunAttack()
    {
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

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;

        if (hp <= 0)
        {
            animator.SetInteger("Attack", 0);
            //Death Effect
            //more bs that he can do
            Destroy(gameObject);
        }
    }
}

//subida y caida con daño en area