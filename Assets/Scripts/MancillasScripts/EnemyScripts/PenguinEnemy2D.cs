using UnityEngine;
using System.Collections;

public class PenguinEnemy2D : MonoBehaviour
{
    public enum PenguinState { Sliding, Attacking, Dead }
    public PenguinState currentState = PenguinState.Sliding;

    [Header("Salud")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Movimiento")]
    public float slideSpeed = 4f;
    public float slideRadius = 6f;
    public float waitBeforeAttack = 1f;

    [Header("Combate")]
    public GameObject basicSlowProjectile;
    public GameObject superSnowballProjectile;
    public Transform shootPoint;
    [Range(0, 100)] public float superProbability = 30f;

    [Header("Referencias 2D")]
    public Transform spriteGraphic;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;
    public Transform player;

    private readonly int isSlidingHash = Animator.StringToHash("IsSliding");
    private readonly int dieHash = Animator.StringToHash("Die");

    private Vector3 targetPosition;

    void Start()
    {
        currentHealth = maxHealth;
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
        if (spriteGraphic != null && mainCamera != null && currentState != PenguinState.Dead)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    private IEnumerator BehaviorRoutine()
    {
        while (currentState != PenguinState.Dead)
        {
            // 1. Encontrar un punto aleatorio para deslizarse
            targetPosition = transform.position + (Random.insideUnitSphere * slideRadius);
            targetPosition.y = transform.position.y; 

            currentState = PenguinState.Sliding;
            if (animator != null) animator.SetBool(isSlidingHash, true);

            spriteRenderer.flipX = targetPosition.x < transform.position.x;

            // Variables para detectar si se queda atascado
            float stuckTimer = 0f;
            Vector3 lastPosition = transform.position;

            // 2. Deslizarse
            while (Vector3.Distance(transform.position, targetPosition) > 0.5f && currentState != PenguinState.Dead)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, slideSpeed * Time.deltaTime);

                // Comprobar si se ha movido muy poco (chocando contra un muro)
                if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer >= 1f) // Si lleva 1 segundo atascado
                    {
                        break; // Rompe el ciclo y avanza directamente a atacar
                    }
                }
                else
                {
                    stuckTimer = 0f; // Reinicia el reloj porque se sigue moviendo libremente
                }

                lastPosition = transform.position;
                yield return null;
            }

            if (animator != null) animator.SetBool(isSlidingHash, false);

            // 3. Pausa antes de atacar
            yield return new WaitForSeconds(waitBeforeAttack);

            // 4. Atacar
            currentState = PenguinState.Attacking;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer.y = 0;
            spriteRenderer.flipX = dirToPlayer.x < 0;

            bool useSuper = Random.Range(0f, 100f) <= superProbability;
            
            if (useSuper)
            {
                Instantiate(superSnowballProjectile, shootPoint.position, Quaternion.LookRotation(dirToPlayer));
            }
            else
            {
                Instantiate(basicSlowProjectile, shootPoint.position, Quaternion.LookRotation(dirToPlayer));
            }

            yield return new WaitForSeconds(1.5f); 
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == PenguinState.Dead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        currentState = PenguinState.Dead;
        StopAllCoroutines();
        if (animator != null) animator.SetTrigger(dieHash);
        GetComponent<Collider>().enabled = false;
        
        if (UnlockManager.Instance != null) UnlockManager.Instance.RegisterKill("Penguin");
        Destroy(gameObject, 2f);
    }
}