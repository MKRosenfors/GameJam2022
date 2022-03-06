using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    #region Declarations and Variables
    #region Stats
    [Header("Stats")]
    public bool isDead;
    float playerHeight = 2f;
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    [SerializeField] float dashForce = 3.5f;
    [SerializeField] float dashCooldown;
    [SerializeField] bool hasDashed = false;
    public float jumpForce = 5f;
    float movementMultiplier = 10f;
    Vector3 moveDirection;
    Vector3 climbDirection;
    Vector3 slopeMoveDirection;
    RaycastHit slopeHit;
    //objektet längst upp i player, använd för position i hela världen och right, left rotation
    [SerializeField] GameObject player;

    #region Wall Running
    [Header("Wall Running")]
    [SerializeField] float wallRunGravity;
    [SerializeField] float wallRunJumpForce;
    [SerializeField] float wallRunfov;
    [SerializeField] float wallRunfovTime;
    [SerializeField] float camTilt;
    [SerializeField] float camTiltTime;
    [SerializeField] float wallDistance = .5f;
    [SerializeField] LayerMask RunLayer;
    private bool wallLeft = false;
    private bool wallRight = false;
    public float tilt { get; private set; }
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    #endregion

    #region Wall Climb
    [Header("Climbing")]
    //[SerializeField] float climbingGrav;
    [SerializeField] float climbingSpeed;
    [SerializeField] float climbCheckRadius;
    [SerializeField] LayerMask ClimbLayer;
    private bool canClimb;
    //private Vector3 climbingVector;
    //private bool wallForward = false;
    //RaycastHit forwardWallHit;
    #endregion

    #endregion

    #region Physics
    [Header("Physics")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;
    [SerializeField] float fallGrav;
    [SerializeField] float longJumpGrav;
    float horizontalMovement;
    float verticalMovement;
    private float fallGravClone;
    #endregion

    #region Ground Detection
    [Header("Ground Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDetectionSphere = 0.2f;
    [SerializeField] float groundDetectionSphereOffset;
    public bool isGrounded { get; private set; }
    #endregion

    #region Camera
    [Header("Camera")]
    //Kameran, kan användas för right,left och up,down rotation och mer lokal position
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float fov;
    #endregion

    #region Sensitivity
    [Header("Sensitivity")]
    [SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;
    float mouseX;
    float mouseY;
    float multiplier = 0.01f;
    float xRotation;
    float yRotation;
    #endregion

    #region Interact
    [SerializeField] float interactRange;
    List<IInteractable> interactables;
    LayerMask interactableLayer;
    #endregion

    #region Keybindings
    [Header("Keybindings")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    /*[SerializeField] KeyCode crouchKey = KeyCode.LeftControl; //Not in use*/
    #endregion

    #region References
    [Header("References")]
    [SerializeField] Transform raycastPoint;
    Rigidbody rb;
    private Transform latestCheckpointPosition;
    #endregion
    #endregion



    #region Updates
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        interactables = FindObjectsOfType<MonoBehaviour>().OfType<IInteractable>().ToList();
        interactableLayer = LayerMask.GetMask("Interactable");
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        fallGravClone = fallGrav;
    }

    void Update()
    {
        RotateCamera();
        CheckInput();
        CheckWallRun();
        CheckWallClimb();
        MyInput();
        ControlDrag();
        ControlGrav();
        ControlClimbPhysics();
        CheckWall();

        isGrounded = Physics.CheckSphere(new Vector3(player.transform.position.x, player.transform.position.y + groundDetectionSphereOffset, player.transform.position.z), groundDetectionSphere, groundMask);
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        CheckForInteractable();
    }
    #endregion

    #region Functions
    void CheckInput()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }
        if (Input.GetKeyDown(dashKey))
        {
            StartCoroutine(Dash());
        }
        if (Input.GetKeyDown(interactKey))
        {
            InteractWithObjects();
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    IEnumerator Dash()
    {
        if(hasDashed == false)
        {
        hasDashed = true;

        rb.AddForce(playerCamera.transform.forward * dashForce, ForceMode.Impulse);
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 100, 1000 * Time.deltaTime);

        yield return new WaitForSeconds(dashCooldown);
        hasDashed = false;
        }

    }
    void MovePlayer()
    {
        if (canClimb == false)
        {
            rb.constraints = RigidbodyConstraints.None;
            if (isGrounded && !OnSlope())
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
            }
            else if (isGrounded && OnSlope())
            {
                rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
            }
            else if (!isGrounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
            }
        }
        else if (canClimb)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.W))
            {
                rb.AddForce(climbDirection.normalized * moveSpeed * climbingSpeed, ForceMode.Acceleration);
                rb.constraints = RigidbodyConstraints.None;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                rb.AddForce(climbDirection.normalized * moveSpeed * climbingSpeed, ForceMode.Acceleration);
            }
        }
    }
    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        //f� rigidbody att rotera och ta den som forward + right
        moveDirection = player.transform.forward * verticalMovement + player.transform.right * horizontalMovement;
        climbDirection = player.transform.up * verticalMovement + player.transform.right * horizontalMovement;
    }
    void RotateCamera()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Wack??
        player.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, tilt);
    }
    void InteractWithObjects()
    {
        Debug.Log(interactables.Count);
        for (int i = 0; i < interactables.Count; i++)
        {
            interactables[i].Interact();
        }
    }
    void CheckForInteractable()
    {
        interactables.Clear();
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactRange, interactableLayer))
        {
            interactables.Add(hit.collider.gameObject.GetComponent<IInteractable>());
        }
    }
    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    void ControlGrav()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallGrav - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(jumpKey))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (longJumpGrav - 1) * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Trap")
        {
            RespawnPlayer();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Checkpoint")
        {
            latestCheckpointPosition = other.transform;
            Debug.Log("New Checkpoint set");
        }
    }

    void KillPlayer()
    {
        isDead = true;
    }
    void RespawnPlayer()
    {
        transform.position = latestCheckpointPosition.position;
    }
    #endregion

    #region WallRunning / WallClimbing
    void CheckWallRun()
    {
        if (!isGrounded)
        {
            if (wallLeft)
            {
                StartWallRun();
            }
            else if (wallRight)
            {
                StartWallRun();
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    void CheckWallClimb()
    {
        if (Physics.CheckSphere(transform.position, climbCheckRadius, ClimbLayer))
        {
            canClimb = true;
        }
        else
        {
            canClimb = false;
        }
    }

    /*void CheckWallClimb()
    {
        if (wallForward)
        {
            StartClimbing();
        }
        if (!wallForward)
        {
            StopClimbing();
        }
    }*/

    void CheckWall()
    {
        wallLeft = Physics.Raycast(player.transform.position, -raycastPoint.transform.right, out leftWallHit, wallDistance, RunLayer);
        wallRight = Physics.Raycast(player.transform.position, raycastPoint.transform.right, out rightWallHit, wallDistance, RunLayer);
       // wallForward = Physics.Raycast(player.transform.position, raycastPoint.transform.forward, out forwardWallHit, wallDistance, ClimbLayer);
    }

    void StartWallRun()
    {
        rb.useGravity = false;

        fallGrav = wallRunGravity;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        else if (wallRight)
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);


        if (Input.GetKeyDown(jumpKey))
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
            }
        }
    }

    void StopWallRun()
    {
        rb.useGravity = true;
        fallGrav = fallGravClone;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }

    void ControlClimbPhysics()
    {
        if (canClimb == true)
        {
            rb.useGravity = false;
            //rb.constraints = RigidbodyConstraints.FreezePositionZ;
            fallGrav = 0f;
        }
        if (canClimb == false)
        {
            rb.useGravity = true;
            //rb.constraints = RigidbodyConstraints.None;
            fallGrav = fallGravClone;
        }
    }

    /*void StartClimbing()
    {
        rb.useGravity = false;

        rb.AddForce(Vector3.down * climbingGrav, ForceMode.Force);

        canClimb = true;
    }

    void StopClimbing()
    {
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        canClimb = false;
    }

    void Climb()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionZ;
        climbingVector = new Vector3(0, climbingSpeed, 0);
        rb.AddForce(climbingVector, ForceMode.Force);
    }*/

    #endregion
    void OnDrawGizmos()
    {
        //isGrounded Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(player.transform.position.x, player.transform.position.y + groundDetectionSphereOffset, player.transform.position.z), groundDetectionSphere);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.transform.position, climbCheckRadius);
    }


}

