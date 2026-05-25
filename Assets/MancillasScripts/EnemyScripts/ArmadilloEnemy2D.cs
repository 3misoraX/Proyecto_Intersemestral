/*
 * ArmadilloEnemy2D controla todo el comportamiento del enemigo armadillo en 3D con logica de juego 2D.
 * Flujo general:
 * 1. En Start busca referencias importantes: Rigidbody, camara principal y jugador.
 * 2. El estado inicial es Chasing, donde persigue al jugador a pie usando FixedUpdate.
 * 3. Cada frame en Update se revisan los temporizadores para decidir si debe iniciar o terminar el ataque especial.
 * 4. Cuando entra en Rolling, calcula una direccion de embestida, aumenta velocidad segun rebotes y reproduce audio/animaciones.
 * 5. Si colisiona con el jugador aplica dano; si choca con una pared durante la embestida, refleja la direccion y acelera.
 * 6. LateUpdate mantiene el sprite orientado hacia la camara para simular un enemigo 2.5D.
 * 7. Die bloquea el comportamiento, detiene audio, desactiva colision y reproduce la animacion de muerte.
 */
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArmadilloEnemy2D : MonoBehaviour
{
    public enum EnemyState { Chasing, Rolling, Dead }
    [Header("Salud")]
    public int maxHealth = 7;
    private int currentHealth;

    [Header("Estados Alterados")]
    private bool isStunned = false;
    [Header("Estado Actual")]
    public EnemyState currentState = EnemyState.Chasing;

    [Header("Configuración de Movimiento")]
    public float walkSpeed = 3f;
    public float rollBaseSpeed = 10f;
    public float rollSpeedIncrement = 2f;
    public float maxRollSpeed = 20f;
    private float currentRollSpeed;

    [Header("Configuración de Ataque")]
    public int normalDamage = 1;
    public int rollDamage = 2;
    public float specialAttackCooldown = 5f;
    public float rollDuration = 3f;
    
    private float cooldownTimer;
    private float rollTimer;

    [Header("Referencias 2D y Visuales")]
    public Transform spriteGraphic; // El GameObject hijo que tiene el SpriteRenderer y el Animator
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;

    [Header("Referencias de Entorno")]
    public Transform player;
    public string playerTag = "Player";
    public string wallTag = "Wall";

    [Header("Configuración de Audio")]
    public AudioSource loopingAudioSource;
    public AudioSource sfxAudioSource;
    
    public AudioClip walkSound;
    public AudioClip rollSound;
    public AudioClip specialAttackStartSound;
    public AudioClip dealDamageSound;

    private Rigidbody rb;
    private Vector3 rollDirection;

    // Hashes de animación
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isRollingHash = Animator.StringToHash("IsRolling");
    private readonly int dieHash = Animator.StringToHash("Die");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Para que la cápsula/esfera de colisión no ruede

        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró una Main Camera para el billboarding.");
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) player = playerObj.transform;
        }

        cooldownTimer = specialAttackCooldown;
        currentHealth = maxHealth;
        StartChasing();
    }

    void Update()
    {
        if (currentState == EnemyState.Dead || player == null) return;

        switch (currentState)
        {
            case EnemyState.Chasing:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    StartSpecialAttack();
                }
                break;

            case EnemyState.Rolling:
                rollTimer -= Time.deltaTime;
                if (rollTimer <= 0f)
                {
                    EndSpecialAttack();
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || player == null) return;

        Vector3 currentDirection = Vector3.zero;

        if (currentState == EnemyState.Chasing)
        {
            currentDirection = (player.position - transform.position).normalized;
            currentDirection.y = 0; 
            
            rb.MovePosition(transform.position + currentDirection * walkSpeed * Time.fixedDeltaTime);
        }
        else if (currentState == EnemyState.Rolling)
        {
            currentDirection = rollDirection;
            rb.linearVelocity = rollDirection * currentRollSpeed;
        }

        // Voltear el sprite dependiendo de si va a la izquierda o derecha
        if (currentDirection.x != 0)
        {
            // Asume que el sprite original mira hacia la derecha.
            // Si mira a la izquierda por defecto, invierte el '<' por '>'
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

    // --- FUNCIÓN DE DAÑO ---
    public void TakeDamage(int damageAmount)
    {
        // Ignorar si ya está muerto
        // Si lo pones en el Jugador, cambia "EnemyState.Dead" por la variable de estado de tu jugador
        if (currentState == EnemyState.Dead) return; 

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {currentHealth}");

        // Opcional: Aquí puedes poner un sonido de recibir daño o cambiar el color del Sprite a rojo por un instante

        if (currentHealth <= 0)
        {
            Die(); // Llama a la función de muerte que ya tienes
        }
    }

    // --- FUNCIÓN DE ATURDIMIENTO ---
    public void ApplyStun(float duration)
    {
        // No aturdir si ya está muerto o si ya está aturdido
        if (currentState == EnemyState.Dead || isStunned) return;

        StartCoroutine(StunRoutine(duration));
    }

    // --- CORRUTINA DE TIEMPO DE ATURDIMIENTO ---
    private System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        
        // Detener el movimiento físico de golpe
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Pausar las animaciones para que parezca congelado
        if (animator != null) animator.speed = 0f;

        // Cambiar el color del sprite (opcional, para dar feedback visual)
        spriteRenderer.color = Color.cyan;

        // Esperar el tiempo indicado en la bala
        yield return new WaitForSeconds(duration);

        // Restaurar la velocidad de la animación y el estado normal
        if (animator != null) animator.speed = 1f;
        spriteRenderer.color = Color.white; // Volver al color original
        
        isStunned = false;
    }

    private void StartChasing()
    {
        currentState = EnemyState.Chasing;
        
        animator.SetBool(isWalkingHash, true);
        animator.SetBool(isRollingHash, false);

        PlayLoopingSound(walkSound);
    }

    private void StartSpecialAttack()
    {
        currentState = EnemyState.Rolling;
        rollTimer = rollDuration;
        currentRollSpeed = rollBaseSpeed;

        rollDirection = (player.position - transform.position).normalized;
        rollDirection.y = 0;

        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isRollingHash, true);

        sfxAudioSource.PlayOneShot(specialAttackStartSound);
        PlayLoopingSound(rollSound);
    }

    private void EndSpecialAttack()
    {
        cooldownTimer = specialAttackCooldown;
        rb.linearVelocity = Vector3.zero; 
        StartChasing();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState == EnemyState.Dead) return;

        if (collision.gameObject.CompareTag(playerTag))
        {
            sfxAudioSource.PlayOneShot(dealDamageSound);
            int damageToDeal = (currentState == EnemyState.Rolling) ? rollDamage : normalDamage;
            Debug.Log($"Hizo {damageToDeal} de daño al jugador.");

            if (currentState == EnemyState.Rolling)
            {
                EndSpecialAttack();
            }
        }
        else if (currentState == EnemyState.Rolling && collision.gameObject.CompareTag(wallTag))
        {
            ContactPoint contact = collision.contacts[0];
            rollDirection = Vector3.Reflect(rollDirection, contact.normal);
            rollDirection.y = 0; 
            rollDirection.Normalize();

            currentRollSpeed += rollSpeedIncrement;
            currentRollSpeed = Mathf.Clamp(currentRollSpeed, rollBaseSpeed, maxRollSpeed);
        }
    }

    public void Die()
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Dead;
        rb.linearVelocity = Vector3.zero;
        
        animator.SetTrigger(dieHash);
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isRollingHash, false);

        loopingAudioSource.Stop();
        
        GetComponent<Collider>().enabled = false;
        rb.isKinematic = true;
        EnemyDeathNotifier notifier = GetComponent<EnemyDeathNotifier>();
        if (notifier != null) notifier.NotifyDeath();
    }

    private void PlayLoopingSound(AudioClip clip)
    {
        if (loopingAudioSource == null || clip == null) return;
        
        loopingAudioSource.Stop();
        loopingAudioSource.clip = clip;
        loopingAudioSource.loop = true;
        loopingAudioSource.Play();
    }
}