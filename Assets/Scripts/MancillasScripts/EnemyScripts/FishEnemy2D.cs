using UnityEngine;
using System.Collections;

public class FishEnemy2D : MonoBehaviour
{
    public enum FishState { Hidden, Attacking, Dead }
    public FishState currentState = FishState.Hidden;

    [Header("Salud")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Combate y Tiempos")]
    public int basicDamage = 2;
    public int beamDamagePerTick = 1;
    public float attackDuration = 4f;
    public float hideDuration = 2f;
    [Range(0, 100)] public float beamProbability = 50f;

    [Header("Referencias 2D")]
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;

    [Header("Referencias de Entorno")]
    public Transform player;
    public Transform[] waterPoints; // Puntos donde puede aparecer
    public LayerMask obstacleLayer; // Capa de las paredes
    public LayerMask playerLayer;   // Capa del jugador

    [Header("Rayo de Agua (Beam)")]
    public LineRenderer beamLine;
    public Transform shootPoint;
    public GameObject basicProjectilePrefab;

    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int dieHash = Animator.StringToHash("Die");

    void Start()
    {
        currentHealth = maxHealth;
        if (Camera.main != null) mainCamera = Camera.main.transform;
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (beamLine != null) beamLine.enabled = false;
        spriteGraphic.gameObject.SetActive(false); // Empieza escondido
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
            // 1. Esconderse y teletransportarse
            currentState = FishState.Hidden;
            spriteGraphic.gameObject.SetActive(false);
            if (beamLine != null) beamLine.enabled = false;
            
            yield return new WaitForSeconds(hideDuration);

            // Teletransportar a un punto de agua aleatorio
            if (waterPoints.Length > 0)
            {
                Transform randomWater = waterPoints[Random.Range(0, waterPoints.Length)];
                transform.position = randomWater.position;
            }

            // 2. Aparecer y atacar
            currentState = FishState.Attacking;
            spriteGraphic.gameObject.SetActive(true);
            if (animator != null) animator.SetBool(isAttackingHash, true);

            // Mirar hacia el jugador (voltear sprite)
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            spriteRenderer.flipX = dirToPlayer.x < 0;

            bool useBeam = Random.Range(0f, 100f) <= beamProbability;

            if (useBeam) yield return StartCoroutine(FireBeamRoutine(dirToPlayer));
            else yield return StartCoroutine(FireBasicRoutine(dirToPlayer));

            if (animator != null) animator.SetBool(isAttackingHash, false);
        }
    }

    private IEnumerator FireBasicRoutine(Vector3 direction)
    {
        // Disparo único con mucho daño
        if (basicProjectilePrefab != null)
        {
            GameObject bullet = Instantiate(basicProjectilePrefab, shootPoint.position, Quaternion.LookRotation(direction));
            // Asegúrate de que este proyectil tenga tu script de daño asignado
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
            // Raycast para encontrar la pared más cercana
            float maxDistance = 20f;
            if (Physics.Raycast(shootPoint.position, direction, out RaycastHit wallHit, 20f, obstacleLayer))
            {
                maxDistance = wallHit.distance;
            }

            // Dibujar la línea
            beamLine.SetPosition(0, shootPoint.position);
            beamLine.SetPosition(1, shootPoint.position + (direction * maxDistance));

            // Raycast para hacer daño al jugador si toca el rayo (con cooldown de daño)
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
                        damageTickTimer = 0.5f; // Hace daño cada medio segundo
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
        if (currentState == FishState.Dead) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        currentState = FishState.Dead;
        if (beamLine != null) beamLine.enabled = false;
        if (animator != null) animator.SetTrigger(dieHash);
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (UnlockManager.Instance != null) UnlockManager.Instance.RegisterKill("Fish");
        Destroy(gameObject, 2f);
    }
}