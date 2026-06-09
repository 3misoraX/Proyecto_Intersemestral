using System.Collections;
using UnityEngine;

public class Specials : MonoBehaviour
{
    [Header("Referencias Generales")]
    public GameObject player;
    private CharacterController charControl;
    private PlayerController playerScript; // Referencia a tu script de movimiento normal

    [Header("Configuración Armadillo")]
    public float specialDashDuration = 0.3f; // Tiempo del dash corto
    public float specialDashSpeed = 20f;
    
    public float superDashDuration = 4f;     // Tiempo de la bola rebotadora
    public float superDashBaseSpeed = 15f;
    public float superDashSpeedIncrement = 3f;
    public float maxSuperDashSpeed = 30f;
    
    [Tooltip("Variable para leer desde tu script de daño si el jugador es invencible")]
    public bool isInvincible = false; 
    private bool isDashing = false;

    [Header("Configuración Araña")]
    public Transform shootPoint;
    public GameObject normalProjectilePrefab;
    public GameObject stunProjectilePrefab; // El proyectil universal que configuraste antes
    public float projectileSpeed = 15f;
    
    [Range(0, 100)] public float shotgunStunChance = 25f;
    public int shotgunPellets = 5;
    public float shotgunSpreadAngle = 45f; // Ángulo del cono de la escopeta

    private readonly Vector3[] directions8Way = new Vector3[]
    {
        new Vector3(0, 0, 1), new Vector3(1, 0, 1).normalized, new Vector3(1, 0, 0), new Vector3(1, 0, -1).normalized,
        new Vector3(0, 0, -1), new Vector3(-1, 0, -1).normalized, new Vector3(-1, 0, 0), new Vector3(-1, 0, 1).normalized
    };

    void Start()
    {
        if (player != null)
        {
            charControl = player.GetComponent<CharacterController>();
            playerScript = player.GetComponent<PlayerController>(); // Tu script PlayerController.cs
        }
    }

    // --- HABILIDADES DE ARMADILLO ---
    public void Armadillo(bool super)
    {
        // Evitar que active la habilidad si ya está rodando
        if (isDashing) return;

        if (!super)
        {
            // Dash invencible corto (no rebota)
            StartCoroutine(ArmadilloDashRoutine(specialDashDuration, specialDashSpeed, false));
        }
        else
        {
            // Dash invencible largo (rebota en paredes)
            StartCoroutine(ArmadilloDashRoutine(superDashDuration, superDashBaseSpeed, true));
        }
    }

    private IEnumerator ArmadilloDashRoutine(float duration, float startSpeed, bool canBounce)
    {
        isDashing = true;
        isInvincible = true;

        // Desactivar el control normal del jugador para que no interfiera
        if (playerScript != null) playerScript.enabled = false;

        float timer = 0f;
        float currentSpeed = startSpeed;
        float bounceCooldown = 0f; // Evita quedarse atascado en esquinas
        
        // Obtener la dirección actual
        Vector3 dashDirection = playerScript.GetMoveDirection();
        if (dashDirection == Vector3.zero)
        {
            dashDirection = player.transform.forward;
        }

        while (timer < duration)
        {
            bounceCooldown -= Time.deltaTime;

            if (canBounce && bounceCooldown <= 0f)
            {
                // Calculamos el centro del jugador
                Vector3 center = player.transform.position + charControl.center;
                
                // Distancia que recorrerá en este frame más un pequeño margen de predicción (0.2f)
                float predictDistance = (currentSpeed * Time.deltaTime) + 0.2f;

                // Lanzamos la esfera ANTES de movernos.
                // Usamos un radio ligeramente menor (0.8f) para no detectar el suelo accidentalmente.
                if (Physics.SphereCast(center, charControl.radius * 0.8f, dashDirection, out RaycastHit hit, predictDistance))
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        // 1. Reflejar la dirección matemáticamente usando la normal de la pared
                        dashDirection = Vector3.Reflect(dashDirection, hit.normal);
                        dashDirection.y = 0; // Mantenerlo en el plano horizontal
                        dashDirection.Normalize();

                        // 2. Aumentar la velocidad
                        currentSpeed = Mathf.Clamp(currentSpeed + superDashSpeedIncrement, startSpeed, maxSuperDashSpeed);
                        
                        // 3. Rotar al jugador visualmente
                        player.transform.rotation = Quaternion.LookRotation(dashDirection);

                        // 4. Activar el cooldown para no rebotar múltiples veces en la misma esquina
                        bounceCooldown = 0.1f; 
                    }
                }
            }

            // Mover al jugador con la dirección final (ya sea la original o la rebotada)
            charControl.Move(dashDirection * currentSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }

        // Restaurar el control al jugador
        if (playerScript != null) playerScript.enabled = true;
        isDashing = false;
        isInvincible = false;
    }


    // --- HABILIDADES DE ARAÑA ---
    public void Spider(bool super)
    {
        if (shootPoint == null)
        {
            Debug.LogWarning("Falta asignar el ShootPoint en Specials.");
            return;
        }

        if (!super)
        {
            // Disparo tipo escopeta, probabilidad de stun
            ShootShotgun();
        }
        else
        {
            // Disparo en 8 direcciones, todos stunean
            Shoot8Way();
        }
    }

    private void ShootShotgun()
    {
        // El signo negativo invierte el vector, apuntando exactamente hacia atrás
        Vector3 baseDirection = -player.transform.forward;
        
        float angleStep = shotgunSpreadAngle / (shotgunPellets - 1);
        float startAngle = -shotgunSpreadAngle / 2f;

        for (int i = 0; i < shotgunPellets; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 pelletDirection = Quaternion.Euler(0, currentAngle, 0) * baseDirection;

            // Decidir qué PREFAB usar basado en la probabilidad
            bool isStun = Random.Range(0f, 100f) <= shotgunStunChance;
            GameObject prefabToUse = isStun ? stunProjectilePrefab : normalProjectilePrefab;

            SpawnProjectile(prefabToUse, pelletDirection);
        }
    }

    private void Shoot8Way()
    {
        for (int i = 0; i < 8; i++)
        {
            // En el ataque super, forzamos que todas usen la bala paralizante
            SpawnProjectile(stunProjectilePrefab, directions8Way[i]);
        }
    }

    private void SpawnProjectile(GameObject prefab, Vector3 direction)
    {
        if (prefab == null) return;

        GameObject bullet = Instantiate(prefab, shootPoint.position, Quaternion.LookRotation(direction));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * projectileSpeed;
        }
    }
}