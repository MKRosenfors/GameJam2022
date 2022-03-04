using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Declarations and Variables
    #region Stats
    [Header("Stats")]
    [SerializeField] public float health;
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
    Vector3 slopeMoveDirection;
    RaycastHit slopeHit;
    //objektet längst upp i player, använd för position i hela världen och right, left rotation
    [SerializeField] GameObject player;

    #region Wall Running
    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunJumpForce;
    [SerializeField] private float wallRunfov;
    [SerializeField] private float wallRunfovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;
    [SerializeField] private float wallDistance = .5f;
    [SerializeField] LayerMask noRunLayer;
    private bool wallLeft = false;
    private bool wallRight = false;
    public float tilt { get; private set; }
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    #endregion
    #endregion

    #region Physics
    [Header("Physics")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;
    [SerializeField] float weaponKnockForce;
    [SerializeField] float fallGrav;
    [SerializeField] float longJumpGrav;
    float horizontalMovement;
    float verticalMovement;
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

    #region Keybindings
    [Header("Keybindings")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode reloadKey = KeyCode.R;
    [SerializeField] KeyCode attackKey = KeyCode.Mouse0;
    [SerializeField] KeyCode secondaryAttackKey = KeyCode.Mouse1;
    [SerializeField] KeyCode dashKey = KeyCode.LeftShift;
    //Crouching
    #endregion

    #region References
    [Header("References")]
    Rigidbody rb;
    Weapon weaponScript;
    #endregion
    #endregion



    #region Updates
    void Awake()
    {
        health = 100;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        weaponScript = FindObjectOfType<Weapon>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        RotateCamera();
        CheckInput();
        CheckWallRun();
        MyInput();
        ControlDrag();
        ControlGrav();
        CheckWall();
        CheckIfDead();

        isGrounded = Physics.CheckSphere(new Vector3(player.transform.position.x, player.transform.position.y + groundDetectionSphereOffset, player.transform.position.z), groundDetectionSphere, groundMask);
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    #endregion

    #region Functions
    void CheckInput()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            StartCoroutine(weaponScript.Reload());
        }
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }
        if (Input.GetKeyDown(attackKey))
        {
            StartCoroutine(weaponScript.Attack());
        }
        if (Input.GetKeyDown(secondaryAttackKey))
        {
            StartCoroutine(weaponScript.SecondaryAttack());
        }
        if (Input.GetKeyDown(dashKey))
        {
            StartCoroutine(Dash());
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
    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        //f� rigidbody att rotera och ta den som forward + right
        moveDirection = player.transform.forward * verticalMovement + player.transform.right * horizontalMovement;
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

    public void TakeDamage(float damage)
    {
        health -= damage;
    }
    public float returnPlayerHealth()
    {
        return health;
    }
    public void CheckIfDead()
    {
        if(health < 1)
        {
            isDead = true;
            moveSpeed = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            sensX = 0;
            sensY = 0;
        }
        else
        {
            isDead = false;
        }
    }
    public void WeaponKnockback()
    {
        rb.AddForce(-playerCamera.transform.forward * weaponKnockForce, ForceMode.Impulse);
    }
    #endregion

    #region WallRunning
    void CheckWallRun()
    {
        if (!isGrounded)
        {
            if (wallLeft)
            {
                StartWallRun();
                Debug.Log("wall running on the left");
            }
            else if (wallRight)
            {
                StartWallRun();
                Debug.Log("wall running on the right");
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

    void CheckWall()
    {
        wallLeft = Physics.Raycast(player.transform.position, -playerCamera.transform.right, out leftWallHit, wallDistance, ~noRunLayer);
        wallRight = Physics.Raycast(player.transform.position, playerCamera.transform.right, out rightWallHit, wallDistance, ~noRunLayer);
    }

    void StartWallRun()
    {
        rb.useGravity = false;

        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        else if (wallRight)
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);


        if (Input.GetKeyDown(KeyCode.Space))
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

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }


    #endregion
    void OnDrawGizmos()
    {
        //isGrounded Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(player.transform.position.x, player.transform.position.y + groundDetectionSphereOffset, player.transform.position.z), groundDetectionSphere);
    }


}
