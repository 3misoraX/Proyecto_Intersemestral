using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GenericRangedEnemy2D : MonoBehaviour
{
    public enum EnemyState { Moving, Attacking, Dead }
    [Header("Estado Actual")]
    public EnemyState currentState = EnemyState.Moving;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 3f;
    [Tooltip("Distancia mínima: Si el jugador se acerca más de esto, el enemigo retrocede.")]
    public float minDistance = 4f;
    [Tooltip("Distancia máxima: Si el jugador se aleja más de esto, el enemigo se acerca.")]
    public float maxDistance = 7f;

    [Header("Configuración de Combate")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public float attackCooldown = 2f;
    private float attackTimer;

    [Header("Salud y Estados")]
    public int maxHealth = 4;
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
    public AudioClip shootSound;
    public AudioClip deathSound;

    private Rigidbody rb;
    private Vector3 currentMoveDirection;

    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int dieHash = Animator.StringToHash("Die");

    private readonly Vector3[] directions8Way = new Vector3[]
    {
        new Vector3(0, 0, 1), new Vector3(1, 0, 1).normalized, new Vector3(1, 0, 0), new Vector3(1, 0, -1).normalized,
        new Vector3(0, 0, -1), new Vector3(-1, 0, -1).normalized, new Vector3(-1, 0, 0), new Vector3(-1, 0, 1).normalized
    };

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

        if (currentState == EnemyState.Moving)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Calcular si debe atacar
            if (distanceToPlayer >= minDistance && distanceToPlayer <= maxDistance && attackTimer <= 0f)
            {
                PerformAttack();
            }
        }
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || player == null || isStunned) return;

        if (currentState == EnemyState.Moving)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer.y = 0;

            currentMoveDirection = Vector3.zero;

            // Decidir si acercarse o alejarse
            if (distanceToPlayer > maxDistance)
            {
                currentMoveDirection = dirToPlayer; // Acercarse
            }
            else if (distanceToPlayer < minDistance)
            {
                currentMoveDirection = -dirToPlayer; // Retroceder
            }

            if (currentMoveDirection != Vector3.zero)
            {
                rb.MovePosition(transform.position + currentMoveDirection * moveSpeed * Time.fixedDeltaTime);
                
                if (spriteRenderer != null) spriteRenderer.flipX = currentMoveDirection.x < 0;
                if (animator != null) animator.SetBool(isMovingHash, true);
            }
            else
            {
                // Si está en el rango ideal, se detiene y mira al jugador
                if (spriteRenderer != null && dirToPlayer.x != 0) spriteRenderer.flipX = dirToPlayer.x < 0;
                if (animator != null) animator.SetBool(isMovingHash, false);
            }
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

        if (firePoint == null || projectilePrefab == null)
        {
            Debug.LogError("Falta FirePoint o ProjectilePrefab en GenericRangedEnemy.");
            Invoke(nameof(ResumeMovement), 0.5f);
            return;
        }

        Vector3 dirToPlayer = (player.position - firePoint.position).normalized;
        dirToPlayer.y = 0;
        Vector3 snapDirection = GetClosest8WayDirection(dirToPlayer);

        if (snapDirection.x != 0 && spriteRenderer != null) spriteRenderer.flipX = snapDirection.x < 0;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(snapDirection));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null) bulletRb.linearVelocity = snapDirection * projectileSpeed;

        if (sfxAudioSource != null && shootSound != null) sfxAudioSource.PlayOneShot(shootSound);

        Invoke(nameof(ResumeMovement), 0.5f);
    }

    private Vector3 GetClosest8WayDirection(Vector3 targetDirection)
    {
        float maxDot = -Mathf.Infinity;
        Vector3 bestDirection = directions8Way[0];

        foreach (Vector3 dir in directions8Way)
        {
            float dotProduct = Vector3.Dot(targetDirection, dir);
            if (dotProduct > maxDot)
            {
                maxDot = dotProduct;
                bestDirection = dir;
            }
        }
        return bestDirection;
    }

    private void ResumeMovement()
    {
        if (currentState == EnemyState.Dead || isStunned) return;
        currentState = EnemyState.Moving;
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