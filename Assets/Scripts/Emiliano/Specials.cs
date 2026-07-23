using System.Collections;
using System.Collections.Generic;
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
    public int normalDashDamage = 1;
    public int superDashDamage = 3;
    
    [Tooltip("Variable para leer desde tu script de daño si el jugador es invencible")]
    public bool isInvincible = false; 
    private bool isDashing = false;

    [Header("Configuración Araña")]
    public Transform shootPoint;
    public GameObject normalProjectilePrefab;
    public GameObject stunProjectilePrefab; // El proyectil universal que configuraste antes
    public float projectileSpeed = 15f;

    [Header("Cooldowns de Habilidades")]
    public float armadilloNormalCooldown = 1.5f;
    public float armadilloSuperCooldown = 5f;
    public float spiderNormalCooldown = 1f;
    public float spiderSuperCooldown = 8f;

    [Header("Configuración Pez")]
    public GameObject fishHeavyBulletPrefab;
    public LineRenderer playerBeamLine;
    public LayerMask environmentLayer; // Para que el rayo choque con muros
    public LayerMask enemiesLayer;     // Para que el rayo atraviese y dañe enemigos
    public Transform playerShootPoint;
    public int playerBeamDamage = 1;
    private bool isFiringBeam = false;

    [Header("Configuración Pinguino")]
    public GameObject playerBasicSlowProjectile;
    public GameObject playerSuperSnowballProjectile;

    // Relojes internos para llevar la cuenta
    private float nextArmadilloTime = 0f;
    private float nextSpiderTime = 0f;

    [Header("Configuración Armiño")]
    public GameObject playerFreezeProjectile;
    public float playerJumpHeight = 4f;
    public int playerJumpDamage = 4;
    public float playerJumpAoERadius = 2.5f;
    public LayerMask EnemyLayer; // Para detectar a quién golpear al caer
    public MonoBehaviour PlayerControllerScript; // Referencia a tu script de control del jugador para desactivarlo durante el salto
    
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
        // 1. Verificamos si la habilidad aún se está recargando
        if (Time.time < nextArmadilloTime)
        {
            Debug.Log("El Armadillo está en cooldown. Falta: " + (nextArmadilloTime - Time.time).ToString("F1") + "s");
            return; 
        }

        // Evitar que active la habilidad si ya está rodando
        if (isDashing) return;

        if (!super)
        {
            // Registramos el tiempo para el próximo uso (Tiempo actual + lo que dura el dash + el cooldown)
            nextArmadilloTime = Time.time + specialDashDuration + armadilloNormalCooldown;
            StartCoroutine(ArmadilloDashRoutine(specialDashDuration, specialDashSpeed, false));
        }
        else
        {
            nextArmadilloTime = Time.time + superDashDuration + armadilloSuperCooldown;
            StartCoroutine(ArmadilloDashRoutine(superDashDuration, superDashBaseSpeed, true));
        }
    }

    private IEnumerator ArmadilloDashRoutine(float duration, float startSpeed, bool canBounce)
    {
        gameObject.GetComponent<PlayerHeallth>().canTakeDamage = false; 
        isDashing = true;
        isInvincible = true;

        // Obtenemos los IDs de las capas de Unity
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        // Desactivamos la colisión física para pasar a través de los enemigos
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        // Desactivar el control normal del jugador para que no interfiera
        if (playerScript != null) {
            playerScript.enabled = false;
            playerScript.SetRollingAnimation(true);
        }

        float timer = 0f;
        float currentSpeed = startSpeed;
        float bounceCooldown = 0f; // Evita quedarse atascado en esquinas
        
        // Obtener la dirección actual
        Vector3 dashDirection = playerScript.GetMoveDirection();
        if (dashDirection == Vector3.zero)
        {
            dashDirection = player.transform.forward;
        }

        // Registro de enemigos golpeados en este dash
        HashSet<GameObject> enemiesDamagedThisDash = new HashSet<GameObject>();

        while (timer < duration)
        {
            bounceCooldown -= Time.deltaTime;
            Vector3 center = player.transform.position + charControl.center;

            if (canBounce && bounceCooldown <= 0f)
            {
                
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
                        enemiesDamagedThisDash.Clear();

                        // 2. Aumentar la velocidad
                        currentSpeed = Mathf.Clamp(currentSpeed + superDashSpeedIncrement, startSpeed, maxSuperDashSpeed);
                        
                        // 3. Rotar al jugador visualmente
                        player.transform.rotation = Quaternion.LookRotation(dashDirection);

                        // 4. Activar el cooldown para no rebotar múltiples veces en la misma esquina
                        bounceCooldown = 0.1f; 
                    }
                }
            }

            // Detección de daño a enemigos
            Collider[] hitColliders = Physics.OverlapSphere(center, charControl.radius * 1.2f);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Enemy") && !enemiesDamagedThisDash.Contains(col.gameObject))
                {
                    int damageAmount = canBounce ? superDashDamage : normalDashDamage;
                    col.gameObject.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
                    enemiesDamagedThisDash.Add(col.gameObject);
                }
            }
            // Mover al jugador con la dirección final (ya sea la original o la rebotada)
            charControl.Move(dashDirection * currentSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }

        // Restaurar el control al jugador
        if (playerScript != null) {
            playerScript.enabled = true;
            playerScript.SetRollingAnimation(false);
        }
        isDashing = false;
        isInvincible = false;
        gameObject.GetComponent<PlayerHeallth>().canTakeDamage = true;
    }


    // --- HABILIDADES DE ARAÑA ---
    public void Spider(bool super)
    {
        // 1. Verificamos si la habilidad aún se está recargando
        if (Time.time < nextSpiderTime)
        {
            Debug.Log("La Araña está en cooldown. Falta: " + (nextSpiderTime - Time.time).ToString("F1") + "s");
            return;
        }

        if (shootPoint == null)
        {
            Debug.LogWarning("Falta asignar el ShootPoint en Specials.");
            return;
        }
        if (playerScript != null) playerScript.ForceShootAnimation(0.3f);
        if (!super)
        {
            // Registramos el tiempo para el próximo uso normal
            nextSpiderTime = Time.time + spiderNormalCooldown;
            ShootShotgun();
        }
        else
        {
            // Registramos el tiempo para el próximo uso especial
            nextSpiderTime = Time.time + spiderSuperCooldown;
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

    public void Fish(bool isSuper)
    {
        if (isFiringBeam) return; // No disparar si ya está lanzando el rayo

        if (!isSuper)
        {
            // Disparo Básico del Pez (Poca cadencia, mucho daño)
            Vector3 aimDir = playerScript.GetMoveDirection(); // O tu vector de apuntado del joystick
            if (aimDir == Vector3.zero) aimDir = player.transform.forward;
            
            Instantiate(fishHeavyBulletPrefab, playerShootPoint.position, Quaternion.LookRotation(aimDir));
        }
        else
        {
            // Super del Pez (Rayo de 4 segundos)
            StartCoroutine(PlayerFishBeamRoutine(4f));
        }
    }

    private IEnumerator PlayerFishBeamRoutine(float duration)
    {
        isFiringBeam = true;
        if (playerBeamLine != null) playerBeamLine.enabled = true;

        // 1. Bloquear la dirección a una de las 4 direcciones cardinales iniciales
        Vector3 rawAim = playerScript.GetMoveDirection();
        if (rawAim == Vector3.zero) rawAim = player.transform.forward;
        
        Vector3 lockedDirection = Vector3.forward;
        if (Mathf.Abs(rawAim.x) > Mathf.Abs(rawAim.z))
            lockedDirection = rawAim.x > 0 ? Vector3.right : Vector3.left;
        else
            lockedDirection = rawAim.z > 0 ? Vector3.forward : Vector3.back;

        float timer = 0f;
        float damageTickTimer = 0f;

        while (timer < duration)
        {
            // El jugador puede moverse, por lo que el rayo debe actualizar su posición inicial siempre
            Vector3 startPos = playerShootPoint.position;
            float maxDistance = 15f; // Rango máximo del rayo

            // Chocar contra el entorno (paredes)
            if (Physics.Raycast(startPos, lockedDirection, out RaycastHit wallHit, maxDistance, environmentLayer))
            {
                maxDistance = wallHit.distance;
            }

            playerBeamLine.SetPosition(0, startPos);
            playerBeamLine.SetPosition(1, startPos + (lockedDirection * maxDistance));

            // Dañar a todos los enemigos en el camino
            damageTickTimer -= Time.deltaTime;
            if (damageTickTimer <= 0f)
            {
                RaycastHit[] hits = Physics.RaycastAll(startPos, lockedDirection, maxDistance, enemiesLayer);
                foreach (var hit in hits)
                {
                    // Llama al script de daño genérico de tus enemigos
                    hit.collider.SendMessage("TakeDamage", playerBeamDamage, SendMessageOptions.DontRequireReceiver);
                }
                damageTickTimer = 0.3f; // Hace daño cada 0.3 segundos
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (playerBeamLine != null) playerBeamLine.enabled = false;
        isFiringBeam = false;
    }

    public void Penguin(bool isSuper)
    {
        Vector3 aimDir = playerScript.GetMoveDirection();
        if (aimDir == Vector3.zero) aimDir = player.transform.forward;

        if (!isSuper)
        {
            // Disparo Básico del Pingüino (Ralentiza)
            Instantiate(playerBasicSlowProjectile, shootPoint.position, Quaternion.LookRotation(aimDir));
        }
        else
        {
            // Súper del Pingüino (Bola de nieve creciente)
            Instantiate(playerSuperSnowballProjectile, shootPoint.position, Quaternion.LookRotation(aimDir));
        }
    }

    public void Ermine(bool isSuper)
    {
        if (!isSuper)
        {
            // Disparo Básico del Armiño (Congela)
            Vector3 aimDir = playerScript.GetMoveDirection();
            if (aimDir == Vector3.zero) aimDir = player.transform.forward;
            
            Instantiate(playerFreezeProjectile, playerShootPoint.position, Quaternion.LookRotation(aimDir));
        }
        else
        {
            // Súper del Armiño (Salto en el sitio)
            StartCoroutine(PlayerErmineJumpRoutine());
        }
    }

    private IEnumerator PlayerErmineJumpRoutine()
    {
        // Bloquear movimiento (asumiendo que tu script de movimiento se puede apagar)
        if (PlayerControllerScript != null) PlayerControllerScript.enabled = false;
        
        // Opcional: Apagar el collider del jugador para que sea invencible al saltar
        Collider pCol = player.GetComponent<Collider>();
        if (pCol != null) pCol.enabled = false;

        Vector3 startPos = player.transform.position;
        float duration = 1.5f; // Un salto un poco más rápido para el jugador
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            Vector3 currentPos = startPos;
            currentPos.y += playerJumpHeight * 4f * t * (1f - t); 
            player.transform.position = currentPos;
            
            yield return null;
        }

        player.transform.position = startPos;
        if (pCol != null) pCol.enabled = true;
        if (PlayerControllerScript != null) PlayerControllerScript.enabled = true;

        // Daño en área al caer
        Collider[] hits = Physics.OverlapSphere(player.transform.position, playerJumpAoERadius, enemiesLayer);
        foreach (var hit in hits)
        {
            hit.SendMessage("TakeDamage", playerJumpDamage, SendMessageOptions.DontRequireReceiver);
        }
    }
}