using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Acceleration Settings")]
    [Tooltip("Ўвидк≥сть розгону/гальмуванн€")]
    public float acceleration = 10f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Camera Smoothing")]
    [Tooltip("«гладжуванн€ камери (0 = вимкнено, 10+ = плавно)")]
    public float cameraSmoothSpeed = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public string groundTag = "Ground";

    [Header("Footstep Sounds")]
    [Tooltip("ѕерет€гни сюди звук ходьби")]
    public AudioClip footstepSound;

    [Tooltip("√учн≥сть звуку ходьби (0 = тихо, 1 = голосно)")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;

    // Components
    private CharacterController controller;
    private Camera playerCamera;
    private AudioSource audioSource;

    // Movement
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 currentVelocity;

    // Camera rotation
    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;

    void Start()
    {
        // ¬идалено DontDestroyOnLoad Ч гравець тепер не збер≥гаЇтьс€ м≥ж сценами

        // Get or add CharacterController
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0, 1f, 0);
        }

        // «найти або створити камеру
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // якщо камера не доч≥рн€ - зробити њњ доч≥рньою
        if (playerCamera != null && playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
            playerCamera.transform.localRotation = Quaternion.identity;
        }

        // —творити AudioSource дл€ звук≥в крок≥в
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = footstepVolume;
        audioSource.clip = footstepSound;

        // Auto-create groundCheck if not assigned
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = checkObj.transform;
        }

        // ¬идалено TeleportToSpawnPoint() Ч телепортац≥€ вимкнена

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        HandleFootsteps();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

        if (cameraSmoothSpeed > 0)
        {
            currentRotationY = Mathf.Lerp(currentRotationY, rotationY, cameraSmoothSpeed * Time.deltaTime);
            currentRotationX = Mathf.Lerp(currentRotationX, rotationX, cameraSmoothSpeed * Time.deltaTime);
        }
        else
        {
            currentRotationY = rotationY;
            currentRotationX = rotationX;
        }

        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        isGrounded = CheckGround();

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 targetDirection = transform.right * moveX + transform.forward * moveZ;
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 targetVelocity = targetDirection * targetSpeed;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        controller.Move(currentVelocity * Time.deltaTime);

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

    void HandleFootsteps()
    {
        bool isWalking = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                         Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isWalking && !audioSource.isPlaying && footstepSound != null)
        {
            audioSource.Play();
        }
        else if (!isWalking && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}