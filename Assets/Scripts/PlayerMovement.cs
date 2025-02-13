using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float boostSpeed;
    [SerializeField] private float standardRotationSpeed;
    [SerializeField] private float boostRotationSpeed;
    private float moveSpeed;
    private float rotationSpeed;
    [SerializeField] private Transform orientation;
    [SerializeField] private float boostLoadTime;
    private float startBoostTime;
    private bool isRunning;
    private bool isBoosting;

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
        boosting,
        airborne
    }    

    private float horizontalInput;
    private float verticalInput;

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

        // if player is moving walk
        if (horizontalInput != 0 || verticalInput != 0)
        {
            if (!animator.GetBool("isWalking"))
                { animator.SetBool("isWalking", true); }
        }

        // if player stops moving stop walk, and also run and boost
        else
        {
            if (animator.GetBool("isWalking"))
                { animator.SetBool("isWalking", false); }
            isRunning = false;
            isBoosting = false;
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isBoosting", isBoosting);
        }

        // if player isn't running start running, else stop. Also stop boosting if it stops running
        if (Input.GetButtonDown("leftStickButton") && animator.GetBool("isWalking") && isGrounded)
        {
            if (isRunning)
            { 
                isBoosting = false;
                animator.SetBool("isBoosting", isBoosting);
            }
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

        // if runnning and grounded get time, else if already boosting stop boosting
        if (Input.GetButtonDown("westButton") && isRunning && isGrounded)
        {
            if (!isBoosting)
                { startBoostTime = Time.time; }
            else
            {
                isBoosting = false;
                animator.SetBool("isBoosting", isBoosting);
            }
        }
        // if player held boost button less than required time cancel boost
        if (Input.GetButtonUp("westButton") && startBoostTime != 0)
        {
            if (Time.time - startBoostTime < boostLoadTime)
            {
                startBoostTime = 0f;
            }
        }

        // if player held boost button long enough enter boost state
        if (startBoostTime != 0f && Time.time - startBoostTime >= boostLoadTime)
        {
            isBoosting = true;
            animator.SetBool("isBoosting", isBoosting);
            startBoostTime = 0f;
        }

    }

    private void StateHandler()
    {
        // Boosting state
        if (isRunning && isBoosting)
        {
            moveState = MovementState.boosting;
            moveSpeed = boostSpeed;
        }

        // Running state
        else if((isGrounded && Input.GetButtonDown("leftStickButton")) || (isRunning && !isBoosting))
        {
            moveState = MovementState.running;
            moveSpeed = runSpeed;
        }

        // Walking state
        else if (isGrounded && !isRunning)
        {
            moveState = MovementState.walking;
            moveSpeed = walkSpeed;
            isBoosting = false;
            startBoostTime = 0f;
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
