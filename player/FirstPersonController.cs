using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public string groundTag = "Ground";

    // Components
    private CharacterController controller;
    private Camera playerCamera;

    // Movement
    private Vector3 velocity;
    private bool isGrounded;

    // Camera rotation
    private float rotationX = 0f;

    void Start()
    {
        // Зберегти персонажа між сценами
        DontDestroyOnLoad(gameObject);

        // Get or add CharacterController
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0, 1f, 0);
        }

        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Auto-create groundCheck if not assigned
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = checkObj.transform;
        }

        // Телепортувати персонажа на збережену позицію
        TeleportToSpawnPoint();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void TeleportToSpawnPoint()
    {
        if (PlayerPrefs.HasKey("SpawnX"))
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            float z = PlayerPrefs.GetFloat("SpawnZ");
            float rotY = PlayerPrefs.GetFloat("SpawnRotY");

            // Вимкнути CharacterController для телепортації
            controller.enabled = false;
            transform.position = new Vector3(x, y, z);
            transform.rotation = Quaternion.Euler(0, rotY, 0);
            controller.enabled = true;

            // Очистити збережені дані
            PlayerPrefs.DeleteKey("SpawnX");
            PlayerPrefs.DeleteKey("SpawnY");
            PlayerPrefs.DeleteKey("SpawnZ");
            PlayerPrefs.DeleteKey("SpawnRotY");
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    void HandleMovement()
    {
        isGrounded = CheckGround();

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    bool CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundDistance))
        {
            return hit.collider.CompareTag(groundTag);
        }
        return false;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
}