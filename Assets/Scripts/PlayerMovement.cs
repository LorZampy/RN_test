using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    private float moveSpeed;
    [SerializeField] private Transform orientation;

    [SerializeField] private float groundDrag;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCD;
    [SerializeField] private float airMultiplier;
    [SerializeField] bool readyToJump;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask ground;
    bool isGrounded;


    [SerializeField] private Animator animator;
    private Rigidbody rigidBody;

    public MovementState moveState;
    public enum MovementState
    {
        walking,
        running,
        airborne
    }    

    private float horizontalInput;
    private float verticalInput;

    private bool isRunning;

    Vector3 moveDirection;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        readyToJump = true;
    }


    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (horizontalInput != 0 || verticalInput != 0)
        {
            if (!animator.GetBool("isWalking"))
                { animator.SetBool("isWalking", true); }
        }
        else
        {
            if (animator.GetBool("isWalking"))
                { animator.SetBool("isWalking", false); }
            isRunning = false;
            animator.SetBool("isRunning", isRunning);
        }

        if (Input.GetButtonDown("leftStickButton") && animator.GetBool("isWalking") && isGrounded)
        {
            isRunning = !isRunning;
            animator.SetBool("isRunning", isRunning);
        }

        if (Input.GetButtonDown("southButton") && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            animator.SetTrigger("jump");

            // call resetJump after a delay equal to jumpCD
            Invoke(nameof(ResetJump), jumpCD);
        }
    }

    private void StateHandler()
    {
        // Running state
        if(isGrounded && Input.GetButtonDown("leftStickButton"))
        {
            moveState = MovementState.running;
            moveSpeed = runSpeed;
        }

        // Walking state
        else if (isGrounded && !isRunning)
        {
            moveState = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Airborne state
        else
        { 
            moveState = MovementState.airborne;
        }
    }

    private void MovePlayer()
    {
        // calculate direction and apply force for either run or walk
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ground movement
        if (isGrounded)
        {
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // air movement
        else
        {
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }


    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);

        // clamp max speed to moveSpeed
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 clampVelocity = flatVelocity.normalized * moveSpeed;
            rigidBody.linearVelocity = new Vector3(clampVelocity.x, rigidBody.linearVelocity.y, clampVelocity.z);
        }
    }


    private void Jump()
    {
        // reset y velocity to avoid interference w/ jump
        rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);

        // apply impulsive force
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }


    private void Update()
    {
        // check ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);

        if (animator.GetBool("isGrounded") != isGrounded)
            { animator.SetBool("isGrounded", isGrounded); }

        MyInput();
        StateHandler();
        SpeedControl();

        if(isGrounded) 
            { rigidBody.linearDamping = groundDrag; }
        else
            { rigidBody.linearDamping = 0; }
    }


    private void FixedUpdate()
    {
        MovePlayer();
    }
}
