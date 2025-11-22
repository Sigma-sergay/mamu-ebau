using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool playerNearby = false;
    private Transform player;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey))
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        Debug.Log("Відкриваю двері, перехід на сцену...");

        // Зберегти позицію для спавну
        PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("SpawnZ", spawnPosition.z);
        PlayerPrefs.SetFloat("SpawnRotY", spawnRotation.y);
        PlayerPrefs.Save();

        // Завантажити сцену
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
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.transform;
            Debug.Log("Гравець підійшов до дверей");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            player = null;
        }
    }

    void OnGUI()
    {
        if (playerNearby && showPrompt)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 50, 400, 50), promptText, style);
        }
    }
}