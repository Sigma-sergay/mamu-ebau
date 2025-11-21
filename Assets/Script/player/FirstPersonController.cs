using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Rotation Settings")]
    [Tooltip("Ўвидк≥сть повороту гравц€ до напр€мку руху")]
    public float rotationSpeed = 10f;

    [Header("Camera Settings")]
    [Tooltip(" амера €ка контролюЇ напр€мок руху")]
    public Transform cameraTransform;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public string groundTag = "Ground";

    // Components
    private CharacterController controller;

    // Movement
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        // «берегти персонажа м≥ж сценами
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

        // «найти камеру €кщо не призначена
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }

        // Auto-create groundCheck if not assigned
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = checkObj.transform;
        }

        // “елепортувати персонажа на збережену позиц≥ю
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

            // ¬имкнути CharacterController дл€ телепортац≥њ
            controller.enabled = false;
            transform.position = new Vector3(x, y, z);
            transform.rotation = Quaternion.Euler(0, rotY, 0);
            controller.enabled = true;

            // ќчистити збережен≥ дан≥
            PlayerPrefs.DeleteKey("SpawnX");
            PlayerPrefs.DeleteKey("SpawnY");
            PlayerPrefs.DeleteKey("SpawnZ");
            PlayerPrefs.DeleteKey("SpawnRotY");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        isGrounded = CheckGround();

        // ќтриманн€ вводу
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // якщо немаЇ камери, використовуЇмо локальн≥ ос≥ гравц€ (€к ран≥ше)
        Vector3 move;
        if (cameraTransform != null)
        {
            // –ух в≥дносно камери (правильно дл€ 3-њ особи)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // ≤гноруЇмо вертикальну складову камери
            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            move = cameraRight * moveX + cameraForward * moveZ;
        }
        else
        {
            // якщо немаЇ камери, рух в≥дносно гравц€
            move = transform.right * moveX + transform.forward * moveZ;
        }

        // ѕоворот гравц€ в напр€мку руху
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Ўвидк≥сть руху
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // √рав≥тац≥€
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