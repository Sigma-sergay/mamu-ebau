using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public enum DoorAction
{
    LoadScene, // Перехід на рівень
    Teleport   // Пастка (телепорт на govno)
}

public class DoorTriggerImproved : MonoBehaviour
{
    [Header("1. ГОЛОВНІ ОБ'ЄКТИ")]
    [Tooltip("Зона (прозорий куб), куди заходить гравець.")]
    [SerializeField] private Collider interactionZone;

    [Tooltip("Самі двері (3D модель), які будуть відчинятися.")]
    [SerializeField] private Transform doorModel;

    [Header("2. НАЛАШТУВАННЯ ДІЇ")]
    public KeyCode interactKey = KeyCode.E;
    public DoorAction actionType = DoorAction.LoadScene;

    [Header("-> Якщо LoadScene (Перехід)")]
    public string sceneName;
    public int sceneIndex = -1;
    [Tooltip("Куди поставити гравця на НОВІЙ сцені")]
    public Vector3 spawnPosition = new Vector3(0, 1, 0);

    [Header("-> Якщо Teleport (Пастка)")]
    [Tooltip("Перетягни сюди об'єкт 'govno'. Якщо пусто - скрипт шукатиме за назвою.")]
    public Transform teleportTargetTransform;

    [Tooltip("Назва об'єкта для пошуку (якщо поле вище пусте).")]
    public string teleportTargetName = "govno";

    [Tooltip("CanvasGroup чорного екрана.")]
    public CanvasGroup screenFader;
    public float fadeSpeed = 1.5f;

    [Header("3. АНІМАЦІЯ")]
    public float openAngle = 90.0f;
    public float animationSpeed = 2.0f;

    [Header("4. АВТО-ТЕКСТ")]
    public string promptText = "Натисни E";
    public float fadeDuration = 0.5f;

    // Приватні змінні
    private TextMeshProUGUI generatedTextUI;
    private CanvasGroup promptCanvasGroup;
    private bool playerNearby = false;
    private bool isBusy = false;
    private Quaternion initialRotation;

    void Start()
    {
        // 1. Налаштування тригера
        if (interactionZone == null)
        {
            Debug.LogError("❌ ПОМИЛКА: Не перетягнуто 'Interaction Zone' в інспекторі!", this);
            return;
        }

        if (!interactionZone.isTrigger) interactionZone.isTrigger = true;
        var listener = interactionZone.gameObject.AddComponent<DoorTriggerListener>();
        listener.Setup(this);

        // 2. Двері
        if (doorModel != null) initialRotation = doorModel.localRotation;

        // 3. Створюємо текст
        SetupAutoUI();

        // 4. Скидаємо екран
        if (screenFader != null) { screenFader.alpha = 0; screenFader.blocksRaycasts = false; }
    }

    private void SetupAutoUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return; // Якщо немає канваса, текст не створиться

        GameObject textObj = new GameObject($"DoorPrompt_{gameObject.name}");
        textObj.transform.SetParent(canvas.transform, false);

        generatedTextUI = textObj.AddComponent<TextMeshProUGUI>();
        generatedTextUI.text = promptText;
        generatedTextUI.fontSize = 36;
        generatedTextUI.alignment = TextAlignmentOptions.Center;
        generatedTextUI.color = Color.white;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.15f);
        rt.anchorMax = new Vector2(0.5f, 0.15f);
        rt.sizeDelta = new Vector2(600, 100);

        promptCanvasGroup = textObj.AddComponent<CanvasGroup>();
        promptCanvasGroup.alpha = 0;
        promptCanvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey) && !isBusy)
        {
            StartCoroutine(PerformSequence());
        }
    }

    private IEnumerator PerformSequence()
    {
        isBusy = true;
        ShowPrompt(false);

        // Відкриваємо двері
        if (doorModel != null)
        {
            Quaternion targetRotation = initialRotation * Quaternion.Euler(0, openAngle, 0);
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * animationSpeed;
                doorModel.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (actionType == DoorAction.LoadScene) LoadLevel();
        else if (actionType == DoorAction.Teleport) yield return StartCoroutine(DoTeleportSequence());
    }

    private void LoadLevel()
    {
        Debug.Log("Перехід на нову сцену...");
        // Зберігаємо координати для спавнера
        PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("SpawnZ", spawnPosition.z);
        PlayerPrefs.SetInt("ShouldSpawn", 1);
        PlayerPrefs.Save();

        if (sceneIndex >= 0) SceneManager.LoadScene(sceneIndex);
        else if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
    }

    private IEnumerator DoTeleportSequence()
    {
        Debug.Log("💀 Починаю телепорт...");

        // Затемнення
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            yield return StartCoroutine(FadeCanvas(screenFader, 1f, 1f / fadeSpeed));
        }

        yield return new WaitForSeconds(0.5f); // Коротка пауза в темряві

        // --- ТУТ ГОЛОВНА МАГІЯ ТЕЛЕПОРТУ ---

        // 1. Визначаємо куди телепортувати
        Transform target = teleportTargetTransform;
        if (target == null)
        {
            // Якщо забув перетягнути, шукаємо за ім'ям "govno"
            GameObject obj = GameObject.Find(teleportTargetName);
            if (obj != null) target = obj.transform;
        }

        // 2. Знаходимо гравця
        GameObject player = GameObject.FindWithTag("Player");

        // 3. Виконуємо переміщення
        if (target != null && player != null)
        {
            Debug.Log($"🚀 ТЕЛЕПОРТУЮ гравця на точку: {target.name} ({target.position})");

            // Вимикаємо контролер (це критично для CharacterController!)
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Переміщуємо
            player.transform.position = target.position;
            player.transform.rotation = target.rotation; // Повертаємо лицем куди треба

            // Чекаємо кадр, щоб фізика оновилась
            yield return null;

            // Вмикаємо контролер назад
            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogError($"❌ ПОМИЛКА: Не знайдено гравця або точку '{teleportTargetName}'!");
        }

        // 4. Закриваємо двері
        if (doorModel != null) doorModel.localRotation = initialRotation;

        // 5. Світлішаємо
        if (screenFader != null)
        {
            yield return StartCoroutine(FadeCanvas(screenFader, 0f, 1f / fadeSpeed));
            screenFader.blocksRaycasts = false;
        }

        isBusy = false;
        if (playerNearby) ShowPrompt(true);
    }

    // --- UI ---
    public void OnPlayerEnter() { playerNearby = true; if (!isBusy) ShowPrompt(true); }
    public void OnPlayerExit() { playerNearby = false; ShowPrompt(false); }

    private void ShowPrompt(bool show)
    {
        if (promptCanvasGroup != null) StartCoroutine(FadeCanvas(promptCanvasGroup, show ? 1 : 0, fadeDuration));
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float target, float duration)
    {
        float start = cg.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }
        cg.alpha = target;
    }
}

public class DoorTriggerListener : MonoBehaviour
{
    private DoorTriggerImproved parentScript;
    public void Setup(DoorTriggerImproved parent) { parentScript = parent; }
    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player") && parentScript != null) parentScript.OnPlayerEnter(); }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player") && parentScript != null) parentScript.OnPlayerExit(); }
}