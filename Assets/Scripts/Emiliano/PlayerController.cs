using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Variables
    //Inputs
    [SerializeField]private InputActionReference moveActions;
    [SerializeField] private InputActionReference shootActions;
    private CharacterController charControl;
    //movement
    private Vector2 movement;
    private Vector2 shootDir;
    public float speed = 6f;
    //jump
    private bool isGrounded;
    private float verticalVelocity = 0f;
    public float gravity = -12f;
    public float iFallVelocity = -2f;
    public float FallTimer = 5f;
    [Header("2.5D Visuals")]
    public Transform spriteGraphic; 
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isShootingHash = Animator.StringToHash("IsShooting");
    private readonly int isRollingHash = Animator.StringToHash("IsRolling");
    private readonly int damageHash = Animator.StringToHash("Damage");
    private float forceShootTimer = 0f; // Para forzar la animación con la habilidad de la araña

    void Awake()
    {
        //gets the character controller at the beginning
        charControl = GetComponent<CharacterController>();
        if (Camera.main != null) mainCamera = Camera.main.transform;
    }

    //input detection, didnt understand why but i'm sure its for connecting it with the new input system easily
    private void OnEnable()
    {
        moveActions.action.performed += storeInput;
        moveActions.action.canceled += storeInput;
        shootActions.action.performed += storeShootInput;
        shootActions.action.canceled += storeShootInput;
    }

    private void OnDisable()
    {
        moveActions.action.performed -= storeInput;
        moveActions.action.canceled -= storeInput;
        shootActions.action.performed -= storeShootInput;
        shootActions.action.canceled -= storeShootInput;
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        //gravity
        isGrounded = charControl.isGrounded;
        HandleGravity();
        //movement method
        Move();
        UpdateAnimations();
    }

    void LateUpdate()
    {
        if (spriteGraphic != null && mainCamera != null)
        {
            spriteGraphic.forward = mainCamera.forward;
        }
    }

    //detects the player input and stores it on a vector2
    private void storeInput(InputAction.CallbackContext call)
    {
        movement = call.ReadValue<Vector2>();
    }

    //detects the player input regarding shooting for direction purpouses
    private void storeShootInput(InputAction.CallbackContext call)
    {
        shootDir = call.ReadValue<Vector2>();
    }

    //Movement, makes the camera direction "forward" and moves the character according to where they are facing
    void Move()
    {
        var mover = new Vector3(movement.x, 0, movement.y).normalized;
        var fMove = mover * speed;
        fMove.y = verticalVelocity;
        charControl.Move(fMove * Time.deltaTime);
    }

    // Función pública para que Specials.cs pueda leer la dirección de WASD
    public Vector3 GetMoveDirection()
    {
        return new Vector3(movement.x, 0, movement.y).normalized;
    }

    private void HandleRotation()
    {
        var moveDir = new Vector3(shootDir.x, 0, shootDir.y).normalized;
        //rotates the player depending on the input
        if (moveDir.z > 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (moveDir.z < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (moveDir.x > 0)
        {
            if(moveDir.z == 0)
            {
                transform.rotation = Quaternion.Euler(0, 270, 0);
            }
        }
        else if (moveDir.x < 0)
        {
            if(moveDir.z == 0)
            {
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }

        if (spriteRenderer != null)
        {
            var aimDir = new Vector3(shootDir.x, 0, shootDir.y).normalized;
            
            if (aimDir.x != 0) // Si está apuntando, mira hacia el disparo
            {
                spriteRenderer.flipX = aimDir.x < 0; 
            }
            else if (movement.x != 0) // Si no apunta, mira hacia donde camina
            {
                spriteRenderer.flipX = movement.x < 0;
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            // Restar el tiempo del temporizador de disparo forzado
            if (forceShootTimer > 0) forceShootTimer -= Time.deltaTime;

            bool isMoving = movement.magnitude > 0.1f;
            // Dispara si mueve el joystick/teclas de apuntar, o si forzamos la animación con la habilidad
            bool isShooting = shootDir.magnitude > 0.1f || forceShootTimer > 0f;

            animator.SetBool(isMovingHash, isMoving);
            animator.SetBool(isShootingHash, isShooting);
        }
    }
    
    private void HandleGravity()
    {
        //basic code for gravity handling
        if(isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = iFallVelocity;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    // Lo llamará Specials.cs cuando use el Armadillo
    public void SetRollingAnimation(bool isRolling)
    {
        if (animator != null) animator.SetBool(isRollingHash, isRolling);
    }

    // Lo llamará Specials.cs cuando use la Araña
    public void ForceShootAnimation(float duration = 0.3f)
    {
        forceShootTimer = duration;
    }

    // Llámalo desde tu función de recibir daño (TakeDamage)
    public void TriggerDamageAnimation()
    {
        if (animator != null) animator.SetTrigger(damageHash);
    }

    public void ApplyFreeze(float duration)
    {
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        // 1. Guardar la velocidad original y ponerla a cero (o apagar el script de movimiento/estado)
        float originalSpeed = speed;
        speed = 0f;
        
        // 2. Esperar
        yield return new WaitForSeconds(duration);
        
        // 3. Restaurar velocidad
        speed = originalSpeed;
    }
}