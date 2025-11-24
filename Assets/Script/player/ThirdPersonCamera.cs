using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Об'єкт гравця, за яким слідкує камера")]
    public Transform target;

    [Tooltip("Точка на гравці, куди дивиться камера (голова/плечі)")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    [Header("Camera Distance")]
    [Tooltip("Відстань камери від гравця")]
    public float normalDistance = 5f;

    [Tooltip("Мінімальна відстань при зіткненні зі стіною")]
    public float minDistance = 0.5f;

    [Header("Camera Rotation")]
    [Tooltip("Чутливість миші")]
    public float mouseSensitivity = 2f;

    [Tooltip("Швидкість обертання камери")]
    public float rotationSpeed = 5f;

    [Tooltip("Мінімальний кут нахилу (вниз)")]
    public float minVerticalAngle = -40f;

    [Tooltip("Максимальний кут нахилу (вверх)")]
    public float maxVerticalAngle = 80f;

    [Header("Camera Smoothing")]
    [Tooltip("Швидкість згладжування руху камери")]
    public float positionSmoothing = 10f;

    [Tooltip("Швидкість згладжування обертання камери")]
    public float rotationSmoothing = 8f;

    [Header("Collision Settings")]
    [Tooltip("Шари з якими камера перевіряє зіткнення")]
    public LayerMask collisionLayers = -1;

    [Tooltip("Радіус камери для перевірки зіткнень")]
    public float cameraRadius = 0.3f;

    [Tooltip("Створити невидимий колайдер навколо камери")]
    public bool enableCameraCollider = true;

    [Tooltip("Розмір Box Collider навколо камери")]
    public Vector3 colliderSize = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Zoom Settings")]
    [Tooltip("Можливість зуму колесом миші")]
    public bool enableZoom = true;

    [Tooltip("Швидкість зуму")]
    public float zoomSpeed = 2f;

    [Tooltip("Мінімальна відстань зуму")]
    public float minZoomDistance = 2f;

    [Tooltip("Максимальна відстань зуму")]
    public float maxZoomDistance = 10f;

    // Private variables
    private float currentDistance;
    private float desiredDistance;
    private float horizontalAngle;
    private float verticalAngle;
    private Vector3 currentPosition;
    private Quaternion currentRotation;
    private BoxCollider cameraCollider;

    void Start()
    {
        // Ініціалізація
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("ThirdPersonCamera: Не знайдено гравця! Додай тег 'Player' до об'єкта гравця.");
                enabled = false;
                return;
            }
        }

        currentDistance = normalDistance;
        desiredDistance = normalDistance;

        // Початкові кути на основі поточної позиції камери
        Vector3 angles = transform.eulerAngles;
        horizontalAngle = angles.y;
        verticalAngle = angles.x;

        currentPosition = transform.position;
        currentRotation = transform.rotation;

        // Створення колайдера навколо камери
        SetupCameraCollider();

        // Сховати курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Обробка вводу миші
        HandleMouseInput();

        // Обробка зуму
        HandleZoom();

        // Розрахунок бажаної позиції камери
        Vector3 targetPosition = target.position + targetOffset;

        // Обертання камери
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

        // Перевірка зіткнень та розрахунок відстані
        float finalDistance = CalculateCameraDistance(targetPosition, rotation);

        // Плавне переміщення відстані
        currentDistance = Mathf.Lerp(currentDistance, finalDistance, Time.deltaTime * positionSmoothing);

        // Розрахунок позиції камери
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * currentDistance);

        // Плавне переміщення камери
        currentPosition = Vector3.Lerp(currentPosition, desiredPosition, Time.deltaTime * positionSmoothing);
        currentRotation = Quaternion.Slerp(currentRotation, rotation, Time.deltaTime * rotationSmoothing);

        // Застосування позиції та обертання
        transform.position = currentPosition;
        transform.rotation = currentRotation;
    }

    void HandleMouseInput()
    {
        // Отримання вводу миші
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Оновлення кутів
        horizontalAngle += mouseX;
        verticalAngle -= mouseY;

        // Обмеження вертикального кута
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    void HandleZoom()
    {
        if (!enableZoom) return;

        // Зум колесом миші
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            desiredDistance -= scroll * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minZoomDistance, maxZoomDistance);
        }
    }

    float CalculateCameraDistance(Vector3 targetPosition, Quaternion rotation)
    {
        // Напрямок від цілі до камери
        Vector3 direction = rotation * -Vector3.forward;

        // Перевірка зіткнення з об'єктами
        RaycastHit hit;
        float checkDistance = enableZoom ? desiredDistance : normalDistance;

        if (Physics.SphereCast(targetPosition, cameraRadius, direction, out hit, checkDistance, collisionLayers))
        {
            // Якщо є перешкода, розміщуємо камеру перед нею
            return Mathf.Max(hit.distance - cameraRadius, minDistance);
        }

        // Якщо перешкод немає, повертаємо бажану відстань
        return checkDistance;
    }

    void SetupCameraCollider()
    {
        if (!enableCameraCollider) return;

        // Перевіряємо чи вже є Box Collider
        cameraCollider = GetComponent<BoxCollider>();

        if (cameraCollider == null)
        {
            // Створюємо новий Box Collider
            cameraCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Налаштування колайдера
        cameraCollider.size = colliderSize;
        cameraCollider.center = Vector3.zero;
        cameraCollider.isTrigger = false; // Фізичний колайдер - не пропускає камеру крізь стіни

        // Додаємо Rigidbody щоб колайдер працював правильно
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Налаштування Rigidbody
        rb.isKinematic = true; // Камера не реагує на фізику
        rb.useGravity = false;

        // Ігноруємо зіткнення з гравцем
        if (target != null)
        {
            Collider[] playerColliders = target.GetComponentsInChildren<Collider>();
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(cameraCollider, playerCollider, true);
            }

            Debug.Log($"Camera Collider створено! Ігноруємо {playerColliders.Length} колайдерів гравця.");
        }
        else
        {
            Debug.Log("Camera Collider створено!");
        }
    }

    // Метод для увімкнення/вимкнення колайдера камери
    public void SetCameraColliderEnabled(bool enabled)
    {
        enableCameraCollider = enabled;

        if (cameraCollider != null)
        {
            cameraCollider.enabled = enabled;
        }
    }

    // Метод для зміни цілі камери
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Метод для миттєвого переміщення камери (без плавності)
    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + targetOffset;
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

        currentDistance = normalDistance;
        currentPosition = targetPosition - (rotation * Vector3.forward * currentDistance);
        currentRotation = rotation;

        transform.position = currentPosition;
        transform.rotation = currentRotation;
    }
}