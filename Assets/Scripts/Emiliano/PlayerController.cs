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
    [Header("2.5D Visuals")]
    public Transform spriteGraphic; 
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform mainCamera;
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isShootingHash = Animator.StringToHash("hasWeapon");

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
        //gravity
        isGrounded = charControl.isGrounded;
        HandleGravity();
        //movement method
        Move();
        HandleRotation();
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
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else if (moveDir.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
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
            // Detecta si te estás moviendo (WASD)
            bool isMoving = movement.magnitude > 0.1f;
            
            // Detecta si estás apuntando/disparando (Flechas / Joystick derecho)
            bool isShooting = shootDir.magnitude > 0.1f;

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
}