using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Variables
    //Inputs
    [SerializeField]private InputActionReference moveActions;
    [SerializeField]private InputActionReference JumpAction;
    private CharacterController charControl;
    //movement
    private Vector2 movement;
    public float speed = 6f;
    //jump
    public bool jumpEnabled = false;
    private bool isGrounded;
    private float verticalVelocity = 0f;
    public float jumpForce = 7f;
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
        JumpAction.action.performed += Jump;
    }

    private void OnDisable()
    {
        moveActions.action.performed -= storeInput;
        moveActions.action.canceled -= storeInput;
        JumpAction.action.performed -= Jump;
    }

    // Update is called once per frame
    void Update()
    {
        //gravity
        isGrounded = charControl.isGrounded;
        HandleGravity();
        //movement method
        Move();
    }

    //detects the player input and stores it on a vector2
    private void storeInput(InputAction.CallbackContext call)
    {
        movement = call.ReadValue<Vector2>();
    }

    //Movement, makes the camera direction "forward" and moves the character according to where they are facing
    void Move()
    {
        var mover = new Vector3(movement.x, 0, movement.y).normalized;
        var fMove = mover * speed;
        fMove.y = verticalVelocity;
        charControl.Move(fMove * Time.deltaTime);
    }
    
    private void Jump(InputAction.CallbackContext call)
    {
        //player on the ground will jump
        if (isGrounded && jumpEnabled)
        {
            verticalVelocity = jumpForce;
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