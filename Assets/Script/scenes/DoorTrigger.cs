using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName;
    public int sceneIndex = -1;

    [Header("Spawn Settings")]
    public Vector3 spawnPosition = new Vector3(0, 1, 0);
    public Vector3 spawnRotation = new Vector3(0, 0, 0);

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 3f;

    [Header("UI Settings")]
    public bool showPrompt = true;
    public string promptText = "Натисни E щоб відкрити двері";

    [Tooltip("Розмір тексту підказки")]
    [Range(20, 100)]
    public int textSize = 48;

    [Header("Door Animation")]
    [Tooltip("Кут відкривання дверей навколо осі Z (в градусах). Напівоберт = 180.")]
    public float openAngle = 90f; // зміни на 180f, якщо хочеш повний напівоберт

    [Tooltip("Швидкість відкривання дверей (1 = за 1 секунду)")]
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
    private Transform player;
    private AudioSource audioSource;
    private Vector3 closedRotation;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = soundVolume;

        // Зберігаємо початкове обертання
        closedRotation = transform.localEulerAngles;
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey) && !isTransitioning)
        {
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        isTransitioning = true;
        Debug.Log("Відкриваю двері з анімацією обертання...");

        // Відтворюємо звук кліку
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
        }

        // Цільовий кут: початковий + відкритий кут по осі Z
        Vector3 targetRotation = closedRotation + new Vector3(0, 0, openAngle);

        float elapsedTime = 0f;
        float duration = 1f / openSpeed; // наприклад, при openSpeed = 2 → тривалість = 0.5 сек

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.localEulerAngles = Vector3.Lerp(closedRotation, targetRotation, t);
            yield return null;
        }

        // Переконуємось, що кут точно встановлений
        transform.localEulerAngles = targetRotation;

        // Невелика затримка перед переходом
        yield return new WaitForSeconds(0.3f);

        // Зберігаємо позицію спавну
        PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("SpawnZ", spawnPosition.z);
        PlayerPrefs.SetFloat("SpawnRotY", spawnRotation.y);
        PlayerPrefs.SetInt("ShouldSpawn", 1);
        PlayerPrefs.Save();

        // Завантажуємо сцену
        if (sceneIndex >= 0)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Не вказано сцену для переходу!");
            isTransitioning = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.transform;
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
            player = null;
            soundPlayed = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            player = collision.transform;
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
            player = null;
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