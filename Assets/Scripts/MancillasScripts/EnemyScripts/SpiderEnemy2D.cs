using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderEnemy2D : MonoBehaviour
{
    public enum EnemyState { Moving, Attacking, SpecialAttacking, Dead }
    
    [Header("Salud")]
    public int maxHealth = 7;
    private int currentHealth;

    [Header("Estados Alterados")]
    private bool isStunned = false;
    [Header("Estado Actual")]
    public EnemyState currentState = EnemyState.Moving;

    [Header("Configuración de Movimiento Errático")]
    public float moveSpeed = 4f;
    public float changeDirectionInterval = 1.5f;
    private Vector3 randomMoveDirection;
    private float moveTimer;

    [Header("Configuración de Combate")]
    public float projectileSpeed = 8f;
    public float attackCooldown = 2f;
    public float specialAttackCooldown = 10f;
    [Range(0f, 100f)] public float stunChance = 25f; // Probabilidad de disparar bala paralizante
    
    private float attackTimer;
    private float specialAttackTimer;

    [Header("Referencias de Proyectiles")]
    public GameObject normalProjectilePrefab;
    public GameObject stunProjectilePrefab; // La bala que paraliza al jugador
    public Transform firePoint; // Desde dónde salen los disparos

    [Header("Referencias 2D y Visuales")]
    public Transform spriteGraphic; 
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;

    [Header("Referencias de Entorno")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Configuración de Audio")]
    public AudioSource loopingAudioSource; // Movimiento
    public AudioSource sfxAudioSource;     // Disparos/Muerte
    
    public AudioClip moveSound;
    public AudioClip shootSound;
    public AudioClip specialShootSound;
    public AudioClip deathSound;

    private Rigidbody rb;

    // Hashes de animación
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int specialAttackHash = Animator.StringToHash("SpecialAttack");
    private readonly int dieHash = Animator.StringToHash("Die");

    // Las 8 direcciones posibles
    private readonly Vector3[] directions8Way = new Vector3[]
    {
        new Vector3(0, 0, 1),   // Arriba (Adelante en 3D)
        new Vector3(1, 0, 1).normalized, // Arriba-Derecha
        new Vector3(1, 0, 0),   // Derecha
        new Vector3(1, 0, -1).normalized, // Abajo-Derecha
        new Vector3(0, 0, -1),  // Abajo (Atrás en 3D)
        new Vector3(-1, 0, -1).normalized, // Abajo-Izquierda
        new Vector3(-1, 0, 0),  // Izquierda
        new Vector3(-1, 0, 1).normalized // Arriba-Izquierda
    };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (Camera.main != null) mainCamera = Camera.main.transform;
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) player = playerObj.transform;
        }

        attackTimer = attackCooldown;
        specialAttackTimer = specialAttackCooldown;
        PickNewRandomDirection();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentState == EnemyState.Dead || player == null) return;

        // Timers
        attackTimer -= Time.deltaTime;
        specialAttackTimer -= Time.deltaTime;

        if (currentState == EnemyState.Moving)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                PickNewRandomDirection();
            }

            // Transiciones a ataques
            if (specialAttackTimer <= 0f)
            {
                PerformSpecialAttack();
            }
            else if (attackTimer <= 0f)
            {
                PerformNormalAttack();
            }
        }
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead) return;

        if (currentState == EnemyState.Moving)
        {
            // Movimiento errático
            rb.MovePosition(transform.position + randomMoveDirection * moveSpeed * Time.fixedDeltaTime);
            
            // Voltear sprite según el movimiento
            if (randomMoveDirection.x != 0)
            {
                spriteRenderer.flipX = randomMoveDirection.x < 0;
            }
        }
        else
        {
            // Detenerse al atacar
            rb.linearVelocity = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        // Billboarding
        if (spriteGraphic != null && mainCamera != null)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    // --- FUNCIÓN DE DAÑO ---
    public void TakeDamage(int damageAmount)
    {
        // Ignorar si ya está muerto
        if (currentState == EnemyState.Dead) return; 

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(); // Llama a la función de muerte
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
    private void PickNewRandomDirection()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        randomMoveDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
        moveTimer = changeDirectionInterval + Random.Range(-0.5f, 0.5f); 

        if (animator != null) 
        {
            animator.SetBool(isMovingHash, true);
        }
        
        PlayLoopingSound(moveSound);
    }

    private void PerformNormalAttack()
    {
        currentState = EnemyState.Attacking;
        
        if (animator != null)
        {
            animator.SetBool(isMovingHash, false);
            animator.SetTrigger(attackHash);
        }
        
        attackTimer = attackCooldown;
        if (loopingAudioSource != null) loopingAudioSource.Stop();

        // Evitar error si no hay firePoint
        if (firePoint == null)
        {
            Debug.LogError("Falta el FirePoint en RangedEnemy2D.");
            Invoke(nameof(ResumeMovement), 0.5f);
            return;
        }

        Vector3 dirToPlayer = (player.position - firePoint.position).normalized;
        dirToPlayer.y = 0;
        Vector3 snapDirection = GetClosest8WayDirection(dirToPlayer);

        if (snapDirection.x != 0 && spriteRenderer != null) 
        {
            spriteRenderer.flipX = snapDirection.x < 0;
        }

        bool isStun = Random.Range(0f, 100f) <= stunChance;
        GameObject prefabToUse = isStun ? stunProjectilePrefab : normalProjectilePrefab;

        ShootProjectile(prefabToUse, snapDirection);
        
        if (sfxAudioSource != null && shootSound != null)
        {
            sfxAudioSource.PlayOneShot(shootSound);
        }

        Invoke(nameof(ResumeMovement), 0.5f); 
    }

    private void PerformSpecialAttack()
    {
        currentState = EnemyState.SpecialAttacking;
        
        if (animator != null)
        {
            animator.SetBool(isMovingHash, false);
            animator.SetTrigger(specialAttackHash);
        }

        specialAttackTimer = specialAttackCooldown;
        if (loopingAudioSource != null) loopingAudioSource.Stop();
        
        if (sfxAudioSource != null && specialShootSound != null)
        {
            sfxAudioSource.PlayOneShot(specialShootSound);
        }

        List<bool> bulletTypes = new List<bool> { true, true, true, true, false, false, false, false };
        
        for (int i = 0; i < bulletTypes.Count; i++)
        {
            bool temp = bulletTypes[i];
            int randomIndex = Random.Range(i, bulletTypes.Count);
            bulletTypes[i] = bulletTypes[randomIndex];
            bulletTypes[randomIndex] = temp;
        }

        for (int i = 0; i < 8; i++)
        {
            GameObject prefabToUse = bulletTypes[i] ? stunProjectilePrefab : normalProjectilePrefab;
            ShootProjectile(prefabToUse, directions8Way[i]);
        }

        Invoke(nameof(ResumeMovement), 1f); 
    }

    public void Die()
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Dead;
        if (rb != null) rb.linearVelocity = Vector3.zero;
        
        if (animator != null)
        {
            animator.SetTrigger(dieHash);
            animator.SetBool(isMovingHash, false);
        }

        if (loopingAudioSource != null) loopingAudioSource.Stop();
        if (sfxAudioSource != null && deathSound != null) sfxAudioSource.PlayOneShot(deathSound);
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        if (rb != null) rb.isKinematic = true;
        EnemyDeathNotifier notifier = GetComponent<EnemyDeathNotifier>();
        if (notifier != null) notifier.NotifyDeath();
    }

    private void ShootProjectile(GameObject prefab, Vector3 direction)
    {
        if (prefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(prefab, firePoint.position, Quaternion.LookRotation(direction));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * projectileSpeed;
        }
    }

    private Vector3 GetClosest8WayDirection(Vector3 targetDirection)
    {
        float maxDot = -Mathf.Infinity;
        Vector3 bestDirection = directions8Way[0];

        // Compara el vector del jugador con los 8 posibles y elige el que sea más parecido (producto punto más alto)
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
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Moving;
        PickNewRandomDirection();
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