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

    [Header("Sound Settings")]
    [Tooltip("Перетягни сюди звук, який грає коли гравець підходить до дверей")]
    public AudioClip hoverSound;

    [Tooltip("Перетягни сюди звук, який грає коли натискаєш E")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    [Tooltip("Гучність звуків (0 = тихо, 1 = голосно)")]
    public float soundVolume = 1f;

    private bool playerNearby = false;
    private bool soundPlayed = false;
    private bool isTransitioning = false;
    private Transform player;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = soundVolume;
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
        Debug.Log("Відкриваю двері, перехід на сцену...");

        // Зберігаємо позицію спавну
        PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("SpawnZ", spawnPosition.z);
        PlayerPrefs.SetFloat("SpawnRotY", spawnRotation.y);
        PlayerPrefs.SetInt("ShouldSpawn", 1); // Прапорець що треба заспавнити
        PlayerPrefs.Save();

        // Відтворюємо звук кліку
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
            yield return new WaitForSeconds(0.2f);
        }

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
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 50, 400, 50), promptText, style);
        }
    }
}