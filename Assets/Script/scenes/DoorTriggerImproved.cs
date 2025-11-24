using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Потрібно для створення Canvas
using System.Collections;
using TMPro; // Потрібно для тексту

public enum DoorAction
{
    LoadScene,
    Teleport
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

    [Header("-> Якщо LoadScene")]
    public string sceneName;
    public int sceneIndex = -1;

    [Header("-> Якщо Teleport (Пастка)")]
    [Tooltip("Сюди перетягни об'єкт, куди телепортувати.")]
    public Transform teleportTargetTransform;
    [Tooltip("Або напиши ім'я (якщо лінь тягнути).")]
    public string teleportTargetName = "govno";

    [Tooltip("CanvasGroup чорного екрана (можна пустим).")]
    public CanvasGroup screenFader;
    public float fadeSpeed = 1.0f;

    [Header("3. АНІМАЦІЯ")]
    public float openAngle = 90.0f;
    public float animationSpeed = 2.0f;

    [Header("4. АВТО-ТЕКСТ")]
    public string promptText = "Натисни E";
    public float fadeDuration = 0.5f;

    // Приватні змінні
    private TextMeshProUGUI generatedTextUI; // Створений кодом текст
    private CanvasGroup promptCanvasGroup;
    private bool playerNearby = false;
    private bool isBusy = false;
    private Quaternion initialRotation;
    private Coroutine promptCoroutine;

    void Start()
    {
        // 1. Налаштування тригера
        if (interactionZone == null)
        {
            Debug.LogError("❌ ПОМИЛКА: Не перетягнуто 'Interaction Zone'!", this);
            return;
        }

        if (!interactionZone.isTrigger) interactionZone.isTrigger = true;
        var listener = interactionZone.gameObject.AddComponent<DoorTriggerListener>();
        listener.Setup(this);

        // 2. Двері
        if (doorModel != null) initialRotation = doorModel.localRotation;

        // 3. АВТОМАТИЧНЕ СТВОРЕННЯ UI (Магія тут)
        SetupAutoUI();

        // 4. Екран
        if (screenFader != null)
        {
            screenFader.alpha = 0;
            screenFader.blocksRaycasts = false;
        }
    }

    // --- МАГІЯ СТВОРЕННЯ ТЕКСТУ ---
    private void SetupAutoUI()
    {
        // Шукаємо Canvas. Якщо немає - створюємо.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Auto_Level_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Створюємо об'єкт тексту
        GameObject textObj = new GameObject($"DoorPrompt_{gameObject.name}");
        textObj.transform.SetParent(canvas.transform, false);

        // Додаємо TextMeshPro
        generatedTextUI = textObj.AddComponent<TextMeshProUGUI>();
        generatedTextUI.text = promptText;
        generatedTextUI.fontSize = 36;
        generatedTextUI.alignment = TextAlignmentOptions.Center;
        generatedTextUI.color = Color.white;

        // Налаштування позиції (Внизу по центру)
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.1f); // 10% від низу
        rt.anchorMax = new Vector2(0.5f, 0.1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500, 100);

        // Додаємо CanvasGroup для фейду
        promptCanvasGroup = textObj.AddComponent<CanvasGroup>();
        promptCanvasGroup.alpha = 0; // Ховаємо одразу
        promptCanvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey) && !isBusy)
        {
            StartCoroutine(PerformSequence());
        }
    }

    // --- ЛОГІКА ДІЙ ---
    private IEnumerator PerformSequence()
    {
        isBusy = true;
        ShowPrompt(false);

        // Відкриття дверей
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
        Debug.Log("Завантаження сцени...");
        PlayerPrefs.Save();
        if (sceneIndex >= 0) SceneManager.LoadScene(sceneIndex);
        else if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
    }

    private IEnumerator DoTeleportSequence()
    {
        // Затемнення
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            yield return StartCoroutine(FadeCanvas(screenFader, 1f, 1f / fadeSpeed));
        }

        yield return new WaitForSeconds(1.0f); // Пауза на чорному екрані

        // Телепорт
        Transform target = teleportTargetTransform;
        if (target == null)
        {
            GameObject obj = GameObject.Find(teleportTargetName);
            if (obj != null) target = obj.transform;
        }

        GameObject player = GameObject.FindWithTag("Player");

        if (target != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = target.position;
            player.transform.rotation = target.rotation;
            if (cc != null) cc.enabled = true;
            Debug.Log("Телепорт успішний.");
        }
        else
        {
            Debug.LogError("Помилка телепорту: не знайдено ціль або гравця.");
        }

        // Закриття дверей
        if (doorModel != null) doorModel.localRotation = initialRotation;

        yield return new WaitForSeconds(0.5f);

        // Освітлення
        if (screenFader != null)
        {
            yield return StartCoroutine(FadeCanvas(screenFader, 0f, 1f / fadeSpeed));
            screenFader.blocksRaycasts = false;
        }

        isBusy = false;
        if (playerNearby) ShowPrompt(true);
    }

    // --- UI МЕТОДИ ---
    public void OnPlayerEnter()
    {
        playerNearby = true;
        if (!isBusy) ShowPrompt(true);
    }

    public void OnPlayerExit()
    {
        playerNearby = false;
        ShowPrompt(false);
    }

    private void ShowPrompt(bool show)
    {
        if (promptCanvasGroup == null) return;
        if (promptCoroutine != null) StopCoroutine(promptCoroutine);
        promptCoroutine = StartCoroutine(FadeCanvas(promptCanvasGroup, show ? 1 : 0, fadeDuration));
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

// Слухач тригера
public class DoorTriggerListener : MonoBehaviour
{
    private DoorTriggerImproved parentScript;
    public void Setup(DoorTriggerImproved parent) { parentScript = parent; }
    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player") && parentScript != null) parentScript.OnPlayerEnter(); }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player") && parentScript != null) parentScript.OnPlayerExit(); }
}