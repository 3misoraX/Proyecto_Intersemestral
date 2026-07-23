using UnityEngine;
using System.Collections;


public class ErmineEnemy2D : MonoBehaviour
{
    public enum ErmineState { Walking, Attacking, Jumping, Dead }
    public ErmineState currentState = ErmineState.Walking;

    [Header("Salud")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Movimiento")]
    public float walkSpeed = 3f;
    public float walkRadius = 6f;
    public float waitBeforeAttack = 1f;

    [Header("Combate")]
    public GameObject basicFreezeProjectile;
    public Transform shootPoint;
    public float jumpHeight = 4f;
    public int jumpDamage = 3;
    public float jumpAoERadius = 1.5f;
    [Range(0, 100)] public float superProbability = 40f;

    [Header("Referencias 2D")]
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;
    public Transform player;
    public LayerMask playerLayer;

    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int dieHash = Animator.StringToHash("Die");

    private Vector3 targetPosition;
    private Collider col;

    void Start()
    {
        currentHealth = maxHealth;
        col = GetComponent<Collider>();
        if (Camera.main != null) mainCamera = Camera.main.transform;
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        StartCoroutine(BehaviorRoutine());
    }

    void LateUpdate()
    {
        if (spriteGraphic != null && mainCamera != null && currentState != ErmineState.Dead)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private IEnumerator BehaviorRoutine()
    {
        while (currentState != ErmineState.Dead)
        {
            // 1. Caminar aleatoriamente
            targetPosition = transform.position + (Random.insideUnitSphere * walkRadius);
            targetPosition.y = transform.position.y; 

            currentState = ErmineState.Walking;
            if (animator != null) animator.SetBool(isWalkingHash, true);
            spriteRenderer.flipX = targetPosition.x < transform.position.x;

            float stuckTimer = 0f;
            Vector3 lastPosition = transform.position;

            while (Vector3.Distance(transform.position, targetPosition) > 0.5f && currentState != ErmineState.Dead)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer >= 1f) break; // Seguro anti-atascos
                }
                else stuckTimer = 0f;

                lastPosition = transform.position;
                yield return null;
            }

            if (animator != null) animator.SetBool(isWalkingHash, false);
            yield return new WaitForSeconds(waitBeforeAttack);

            // 2. Decidir Ataque
            bool useSuper = Random.Range(0f, 100f) <= superProbability;
            
            if (useSuper)
            {
                yield return StartCoroutine(JumpAttackRoutine());
            }
            else
            {
                currentState = ErmineState.Attacking;
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                dirToPlayer.y = 0;
                spriteRenderer.flipX = dirToPlayer.x < 0;
                Instantiate(basicFreezeProjectile, shootPoint.position, Quaternion.LookRotation(dirToPlayer));
                yield return new WaitForSeconds(1.5f);
            }
        }
    }

    private IEnumerator JumpAttackRoutine()
    {
        currentState = ErmineState.Jumping;
        if (col != null) col.enabled = false; // Se vuelve invulnerable mientras salta

        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position; // Guarda la posición actual del jugador
        targetPos.y = startPos.y; 

        spriteRenderer.flipX = targetPos.x < startPos.x;

        float duration = 2f; // 1s subiendo, 1s bajando
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Interpolar X y Z
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            // Matemáticas de parábola perfecta para el eje Y
            currentPos.y += jumpHeight * 4f * t * (1f - t); 
            
            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPos; // Asegurar que aterrice exactamente en el suelo
        if (col != null) col.enabled = true;

        // Daño en área al aterrizar
        Collider[] hits = Physics.OverlapSphere(transform.position, jumpAoERadius, playerLayer);
        foreach (var hit in hits)
        {
            hit.SendMessage("TakeDamage", jumpDamage, SendMessageOptions.DontRequireReceiver);
            hit.SendMessage("LoseHealth", jumpDamage, SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(1f); // Pausa de recuperación
    }

    public void TakeDamage(int damage)
    {
        if (currentState == ErmineState.Dead || currentState == ErmineState.Jumping) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        currentState = ErmineState.Dead;
        StopAllCoroutines();
        if (animator != null) animator.SetTrigger(dieHash);
        if (col != null) col.enabled = false;
        
        if (UnlockManager.Instance != null) UnlockManager.Instance.RegisterKill("Ermine");
        Destroy(gameObject, 2f);
    }
}