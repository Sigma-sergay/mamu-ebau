using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Назва сцени, яку треба активувати")]
    public string targetSceneName;

    [Tooltip("Назва поточної сцени (яку треба деактивувати)")]
    public string currentSceneName;



    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Settings")]
    public bool showPrompt = true;
    public string promptText = "Натисни E щоб відкрити двері";

    [Tooltip("Розмір тексту підказки")]
    [Range(20, 100)]
    public int textSize = 48;

    [Header("Door Animation")]
    [Tooltip("Кут відкривання дверей (в градусах)")]
    public float openAngle = 90f;

    [Tooltip("Швидкість відкривання дверей")]
    public float openSpeed = 2f;

    [Header("Sound Settings")]
    [Tooltip("Звук при наближенні")]
    public AudioClip hoverSound;

    [Tooltip("Звук при взаємодії")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    [Tooltip("Гучність звуків")]
    public float soundVolume = 1f;

    private bool playerNearby = false;
    private bool soundPlayed = false;
    private bool isTransitioning = false;
    private AudioSource audioSource;
    private Vector3 closedRotation;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = soundVolume;

        closedRotation = transform.localEulerAngles;

        // Автоматично визначити поточну сцену
        if (string.IsNullOrEmpty(currentSceneName))
        {
            currentSceneName = SceneManager.GetActiveScene().name;
        }
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey) && !isTransitioning)
        {
            StartCoroutine(OpenDoorAndSwitchScene());
        }
    }

    IEnumerator OpenDoorAndSwitchScene()
    {
        isTransitioning = true;
        Debug.Log("Відкриваю двері...");

        // Звук
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
        }

        // Анімація дверей
        Vector3 targetRotation = closedRotation + new Vector3(0, 0, openAngle);
        float elapsedTime = 0f;
        float duration = 1f / openSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.localEulerAngles = Vector3.Lerp(closedRotation, targetRotation, t);
            yield return null;
        }

        transform.localEulerAngles = targetRotation;
        yield return new WaitForSeconds(0.3f);

        // Просто завантажуємо нову сцену (стара автоматично видалиться)
        Debug.Log($"Завантажую сцену: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Гравець підійшов до дверей");

            if (hoverSound != null && !soundPlayed)
            {
                audioSource.PlayOneShot(hoverSound, soundVolume);
                soundPlayed = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            soundPlayed = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Гравець зіткнувся з дверима");

            if (hoverSound != null && !soundPlayed)
            {
                audioSource.PlayOneShot(hoverSound, soundVolume);
                soundPlayed = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
            soundPlayed = false;
        }
    }

    void OnGUI()
    {
        if (playerNearby && showPrompt && !isTransitioning)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = textSize;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;

            int rectWidth = Mathf.Max(200, textSize * promptText.Length / 2);
            int rectHeight = textSize + 10;

            GUI.Label(
                new Rect(Screen.width / 2 - rectWidth / 2, Screen.height / 2 + 50, rectWidth, rectHeight),
                promptText,
                style
            );
        }
    }
}