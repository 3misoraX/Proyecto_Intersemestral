using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GenericMeleeEnemy2D : MonoBehaviour
{
    public enum EnemyState { Chasing, Attacking, Dead }
    [Header("Estado Actual")]
    public EnemyState currentState = EnemyState.Chasing;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 4f;
    public float attackRange = 1.5f;

    [Header("Configuración de Combate")]
    public int damage = 1;
    public float attackCooldown = 1.5f;
    private float attackTimer;

    [Header("Salud y Estados")]
    public int maxHealth = 5;
    private int currentHealth;
    private bool isStunned = false;

    [Header("Referencias 2D y Visuales")]
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;

    [Header("Referencias de Entorno")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Audio")]
    public AudioSource loopingAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip moveSound;
    public AudioClip attackSound;
    public AudioClip deathSound;

    private Rigidbody rb;

    // Hashes de animación
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int dieHash = Animator.StringToHash("Die");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentHealth = maxHealth;

        if (Camera.main != null) mainCamera = Camera.main.transform;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) player = playerObj.transform;
        }

        PlayLoopingSound(moveSound);
    }

    void Update()
    {
        if (currentState == EnemyState.Dead || player == null || isStunned) return;

        attackTimer -= Time.deltaTime;

        if (currentState == EnemyState.Chasing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange && attackTimer <= 0f)
            {
                PerformAttack();
            }
        }
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || player == null || isStunned) return;

        if (currentState == EnemyState.Chasing)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);

            if (direction.x != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }

            if (animator != null) animator.SetBool(isMovingHash, true);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if (spriteGraphic != null && mainCamera != null)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private void PerformAttack()
    {
        currentState = EnemyState.Attacking;
        attackTimer = attackCooldown;

        if (animator != null)
        {
            animator.SetBool(isMovingHash, false);
            animator.SetTrigger(attackHash);
        }

        if (loopingAudioSource != null) loopingAudioSource.Stop();
        if (sfxAudioSource != null && attackSound != null) sfxAudioSource.PlayOneShot(attackSound);

        // Aplicar daño al jugador (asumiendo que está en rango)
        player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        // Volver a perseguir después de un pequeño retraso
        Invoke(nameof(ResumeChasing), 0.5f);
    }

    private void ResumeChasing()
    {
        if (currentState == EnemyState.Dead || isStunned) return;
        currentState = EnemyState.Chasing;
        PlayLoopingSound(moveSound);
    }

    // --- MÉTODOS DE VIDA Y ESTADO ---
    public void TakeDamage(int damageAmount)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

    public void ApplyStun(float duration)
    {
        if (currentState == EnemyState.Dead || isStunned) return;
        StartCoroutine(StunRoutine(duration));
    }

    private System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        if (rb != null) rb.linearVelocity = Vector3.zero;
        if (animator != null) animator.speed = 0f;

        yield return new WaitForSeconds(duration);

        if (animator != null) animator.speed = 1f;
        isStunned = false;
    }

    public void Die()
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Dead;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        if (animator != null)
        {
            animator.SetTrigger(dieHash);
            animator.SetBool(isMovingHash, false);
        }

        if (loopingAudioSource != null) loopingAudioSource.Stop();
        if (sfxAudioSource != null && deathSound != null) sfxAudioSource.PlayOneShot(deathSound);
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        EnemyDeathNotifier notifier = GetComponent<EnemyDeathNotifier>();
        if (notifier != null) notifier.NotifyDeath();
    }

    private void PlayLoopingSound(AudioClip clip)
    {
        if (loopingAudioSource == null || clip == null) return;
        if (loopingAudioSource.isPlaying && loopingAudioSource.clip == clip) return;

        loopingAudioSource.Stop();
        loopingAudioSource.clip = clip;
        loopingAudioSource.loop = true;
        loopingAudioSource.Play();
    }
}