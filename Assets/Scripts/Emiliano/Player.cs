using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
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
    public bool jumpEnabled = false;
    private bool isGrounded;
    private float verticalVelocity = 0f;
    public float gravity = -12f;
    public float iFallVelocity = -2f;

    void Awake()
    {
        //gets the character controller at the beginning
        charControl = GetComponent<CharacterController>();
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