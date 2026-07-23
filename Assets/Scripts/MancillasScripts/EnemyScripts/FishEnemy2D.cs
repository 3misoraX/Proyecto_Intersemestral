using UnityEngine;
using System.Collections;

public class FishEnemy2D : MonoBehaviour
{
    public enum FishState { Hidden, Emerging, Attacking, Hiding, Dead }
    public FishState currentState = FishState.Hidden;

    [Header("Salud")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Combate y Tiempos")]
    public int basicDamage = 2;
    public int beamDamagePerTick = 1;
    public float attackDuration = 4f;
    [Range(0, 100)] public float beamProbability = 50f;

    [Header("Movimiento (Entrar/Salir del Agua)")]
    public float depthOffset = 2f; 
    public float emergeDuration = 0.5f; 
    public float hideDuration = 0.5f; 
    public float waitUnderwaterDuration = 1.5f; 

    [Header("Referencias 2D")]
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;

    [Header("Referencias de Entorno")]
    public Transform player;
    public Transform[] waterPoints; 
    public LayerMask obstacleLayer; 
    public LayerMask playerLayer;   

    [Header("Rayo de Agua (Beam)")]
    public LineRenderer beamLine;
    public Transform shootPoint;
    public GameObject basicProjectilePrefab;

    // Hashes de Animación
    private readonly int emergeHash = Animator.StringToHash("Emerge");
    private readonly int hideHash = Animator.StringToHash("Hide");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int dieHash = Animator.StringToHash("Die");

    // NUEVAS VARIABLES PARA CONTROLAR LA FÍSICA
    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Obtenemos las referencias de física al inicio
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (Camera.main != null) mainCamera = Camera.main.transform;
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (beamLine != null) beamLine.enabled = false;
        spriteGraphic.gameObject.SetActive(false); 
        StartCoroutine(FishBehaviorRoutine());
    }

    void LateUpdate()
    {
        if (spriteGraphic != null && mainCamera != null && currentState != FishState.Dead)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private IEnumerator FishBehaviorRoutine()
    {
        while (currentState != FishState.Dead)
        {
            // 1. PREPARACIÓN INVISIBLE (Bajo el agua)
            currentState = FishState.Hidden;
            
            // APAGAMOS LAS FÍSICAS Y COLISIONES PARA QUE NO CAIGA AL VACÍO
            if (rb != null) 
            {
                rb.isKinematic = true; // Lo vuelve inmune a la gravedad
                rb.linearVelocity = Vector3.zero; // Frena cualquier caída anterior
            }
            if (col != null) col.enabled = false; 

            if (waterPoints.Length > 0)
            {
                Transform randomWater = waterPoints[Random.Range(0, waterPoints.Length)];
                transform.position = randomWater.position + (Vector3.down * depthOffset);
            }
            
            yield return new WaitForSeconds(waitUnderwaterDuration);

            // 2. EMERGER (Subir a la superficie)
            currentState = FishState.Emerging;
            spriteGraphic.gameObject.SetActive(true);
            if (animator != null) animator.SetTrigger(emergeHash);
            
            Vector3 targetSurfacePos = transform.position + (Vector3.up * depthOffset);
            yield return StartCoroutine(MoveVerticalRoutine(targetSurfacePos, emergeDuration));

            // 3. ATACAR
            currentState = FishState.Attacking;
            
            // ENCENDEMOS EL COLLIDER AHORA QUE ESTÁ ARRIBA PARA QUE RECIBA DAÑO
            if (col != null) col.enabled = true; 

            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer.y = 0; 
            spriteRenderer.flipX = dirToPlayer.x < 0;

            if (animator != null) animator.SetBool(isAttackingHash, true);

            bool useBeam = Random.Range(0f, 100f) <= beamProbability;
            if (useBeam) yield return StartCoroutine(FireBeamRoutine(dirToPlayer));
            else yield return StartCoroutine(FireBasicRoutine(dirToPlayer));

            if (animator != null) animator.SetBool(isAttackingHash, false);

            // 4. ESCONDERSE (Bajar al fondo)
            currentState = FishState.Hiding;
            if (animator != null) animator.SetTrigger(hideHash);
            
            // APAGAMOS EL COLLIDER ANTES DE BAJAR PARA QUE NO CHOQUE CON EL PISO
            if (col != null) col.enabled = false; 

            Vector3 targetDepthPos = transform.position + (Vector3.down * depthOffset);
            yield return StartCoroutine(MoveVerticalRoutine(targetDepthPos, hideDuration));

            // 5. APAGAR GRÁFICOS
            spriteGraphic.gameObject.SetActive(false);
            if (beamLine != null) beamLine.enabled = false;
        }
    }

    private IEnumerator MoveVerticalRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition; 
    }

    private IEnumerator FireBasicRoutine(Vector3 direction)
    {
        if (basicProjectilePrefab != null)
        {
            Instantiate(basicProjectilePrefab, shootPoint.position, Quaternion.LookRotation(direction));
        }
        yield return new WaitForSeconds(attackDuration);
    }

    private IEnumerator FireBeamRoutine(Vector3 direction)
    {
        if (beamLine == null) yield break;
        
        beamLine.enabled = true;
        float timer = 0f;
        float damageTickTimer = 0f;

        while (timer < attackDuration && currentState != FishState.Dead)
        {
            float maxDistance = 20f;
            if (Physics.Raycast(shootPoint.position, direction, out RaycastHit wallHit, 20f, obstacleLayer))
            {
                maxDistance = wallHit.distance;
            }

            beamLine.SetPosition(0, shootPoint.position);
            beamLine.SetPosition(1, shootPoint.position + (direction * maxDistance));

            damageTickTimer -= Time.deltaTime;
            if (damageTickTimer <= 0f)
            {
                RaycastHit[] hits = Physics.RaycastAll(shootPoint.position, direction, maxDistance, playerLayer);
                foreach (var hit in hits)
                {
                    PlayerHeallth pHealth = hit.collider.GetComponent<PlayerHeallth>();
                    if (pHealth != null)
                    {
                        pHealth.LoseHealth(beamDamagePerTick);
                        damageTickTimer = 0.5f; 
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
        beamLine.enabled = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == FishState.Dead || currentState == FishState.Hidden || currentState == FishState.Hiding) return; 
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        if (currentState == FishState.Dead) return;
        currentState = FishState.Dead;
        
        StopAllCoroutines(); 

        if (beamLine != null) beamLine.enabled = false;
        if (animator != null) animator.SetTrigger(dieHash);
        
        // Desactivamos el collider usando la variable que ya habíamos guardado
        if (col != null) col.enabled = false;

        if (UnlockManager.Instance != null) UnlockManager.Instance.RegisterKill("Fish");
        Destroy(gameObject, 2f);
    }
}