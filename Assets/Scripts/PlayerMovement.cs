using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
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

        if (Input.GetButtonDown("leftStickButton") && animator.GetBool("isWalking"))
        {
            isRunning = !isRunning;
            animator.SetBool("isRunning", isRunning);
        }

        if (Input.GetButtonDown("southButton") && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();

            // call resetJump after a delay equal to jumpCD
            Invoke(nameof(ResetJump), jumpCD);
        }
    }


    private void MovePlayer()
    {
        // calculate direction and apply force for either run or walk
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ground movement
        if (isGrounded)
        {
            if (isRunning)
            {
                rigidBody.AddForce(moveDirection.normalized * runSpeed * 10f, ForceMode.Force);
            }
            else
            { 
                rigidBody.AddForce(moveDirection.normalized * walkSpeed * 10f, ForceMode.Force);
            }
        }

        // air movement
        else
        {
            if (isRunning)
            {
                rigidBody.AddForce(moveDirection.normalized * runSpeed * 10f * airMultiplier, ForceMode.Force);
            }
            else
            {
                rigidBody.AddForce(moveDirection.normalized * walkSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }
    }


    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);

        // clamp max speed to moveSpeed
        if (!isRunning && flatVelocity.magnitude > walkSpeed)
        {
            Vector3 clampVelocity = flatVelocity.normalized * walkSpeed;
            rigidBody.linearVelocity = new Vector3(clampVelocity.x, rigidBody.linearVelocity.y, clampVelocity.z);
        }
        else if (isRunning && flatVelocity.magnitude > walkSpeed)
        {
            Vector3 clampVelocity = flatVelocity.normalized * runSpeed;
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

        MyInput();
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
