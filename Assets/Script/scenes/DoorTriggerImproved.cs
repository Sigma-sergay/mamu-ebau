using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

// Цей enum можна залишити без змін
public enum DoorBehavior
{
    LoadScene, // Перехід на наступну сцену (правильні двері)
    StopGame // Пастка: Телепорт гравця
}

// *** КЛАС ПЕРЕЙМЕНОВАНО, ЩОБ УНИКНУТИ КОНФЛІКТУ З ІСНУЮЧИМ DoorTrigger ***
public class DoorTriggerImproved : MonoBehaviour
{
    // --- Налаштування Дверей ---
    [Header("Door Behavior")]
    [Tooltip("Визначає дію: перехід на сцену чи пастка/телепорт.")]
    public DoorBehavior behavior = DoorBehavior.LoadScene;

    // --- Налаштування Взаємодії ---
    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;

    // --- Анімація ---
    [Header("Animation")]
    public Transform doorModel;
    public float openAngle = 90.0f;
    public float animationSpeed = 2.0f;

    private bool isBusy = false;
    private Quaternion initialRotation;

    // --- Налаштування Сцени та Спауну ---
    [Header("Scene Settings")]
    public string sceneName;
    public int sceneIndex = -1;

    [Header("Spawn Settings (Використовується для PlayerPrefs)")]
    public Vector3 spawnPosition = new Vector3(0, 1, 0);
    public Vector3 spawnRotation = new Vector3(0, 0, 0);

    // --- НОВЕ: Налаштування Телепорту та Смерті ---
    [Header("Death/Teleport Settings")]
    [Tooltip("Назва об'єкта, на позицію якого телепортується гравець (наприклад, 'govno').")]
    public string teleportTargetName = "govno";

    [Header("Screen Fade (Death)")]
    [Tooltip("CanvasGroup чорного екрана для плавного переходу при смерті.")]
    public CanvasGroup screenFaderCanvasGroup;
    [Tooltip("Швидкість затемнення/посвітлення екрана.")]
    public float fadeSpeed = 1.5f;

    // --- Налаштування UI ---
    [Header("UI Settings")]
    public string promptText = "Натисни E";
    public TextMeshProUGUI promptTextUI;
    public float fadeDuration = 0.5f;

    private bool playerNearby = false;
    private Coroutine activePromptCoroutine;
    private CanvasGroup promptCanvasGroup;

    // --- Unity Методи ---

    void Start()
    {
        if (doorModel != null)
        {
            initialRotation = doorModel.localRotation;
        }

        if (promptTextUI != null)
        {
            promptCanvasGroup = promptTextUI.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                Debug.LogError($"На '{promptTextUI.name}' відсутній компонент CanvasGroup! Додайте CanvasGroup до елементу UI.", this);
            }
            else
            {
                promptTextUI.text = promptText;
                promptCanvasGroup.alpha = 0;
            }
        }

        // Перевірка фейдера (опціонально)
        if (screenFaderCanvasGroup != null)
        {
            // Переконаємося, що фейдер спочатку прозорий і дозволяє взаємодію
            if (screenFaderCanvasGroup.alpha != 0)
                screenFaderCanvasGroup.alpha = 0;
            screenFaderCanvasGroup.blocksRaycasts = false;
            screenFaderCanvasGroup.interactable = false;
        }
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey) && !isBusy)
        {
            TryOpenDoor();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            if (promptTextUI != null && promptCanvasGroup != null && !isBusy)
            {
                if (activePromptCoroutine != null)
                {
                    StopCoroutine(activePromptCoroutine);
                }
                activePromptCoroutine = StartCoroutine(FadePrompt(1));
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (promptTextUI != null && promptCanvasGroup != null)
            {
                if (activePromptCoroutine != null)
                {
                    StopCoroutine(activePromptCoroutine);
                }
                activePromptCoroutine = StartCoroutine(FadePrompt(0));
            }
        }
    }

    // --- Логіка Дверей ---

    public void TryOpenDoor()
    {
        if (doorModel == null)
        {
            Debug.LogError("Не вказано 'Door Model' в інспекторі! Анімація неможлива.", this);
            return;
        }

        // Приховуємо підказку незалежно від результату
        if (activePromptCoroutine != null) StopCoroutine(activePromptCoroutine);
        if (promptCanvasGroup != null) StartCoroutine(FadePrompt(0));

        isBusy = true;
        if (behavior == DoorBehavior.LoadScene)
        {
            Debug.Log($"[DoorTrigger] Правильні двері! Відкриваю... -> Сцена: {(sceneIndex >= 0 ? sceneIndex.ToString() : sceneName)}");
            StartCoroutine(AnimateDoorAndLoadScene());
        }
        else if (behavior == DoorBehavior.StopGame)
        {
            // ШЛЯХ СМЕРТІ
            Debug.Log("[DoorTrigger] Пастка! Смерть та Телепорт.");
            StartCoroutine(AnimateWrongDoor());
        }
    }

    // --- Анімація та Перехід ---

    private IEnumerator AnimateDoorAndLoadScene()
    {
        // Анімація відкриття
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0, openAngle, 0); // Поворот навколо Y, як зазвичай для дверей
        float t = 0;

        while (t < 1)
        {
            doorModel.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            t += Time.deltaTime * animationSpeed;
            yield return null;
        }
        doorModel.localRotation = targetRotation;

        yield return new WaitForSeconds(1.0f);
        LoadNextScene();
    }

    private IEnumerator AnimateWrongDoor()
    {
        // 1. Анімація "похитування" дверей (паніка)
        float jiggleAngle = 5f;
        float duration = 0.1f;
        int shakeCount = 3;

        for (int i = 0; i < shakeCount; i++)
        {
            Quaternion targetJiggle = initialRotation * Quaternion.Euler(0, jiggleAngle, 0);
            yield return AnimateRotation(doorModel.localRotation, targetJiggle, duration);
            yield return AnimateRotation(doorModel.localRotation, initialRotation, duration);
        }

        doorModel.localRotation = initialRotation;
        yield return new WaitForSeconds(0.5f);

        // 2. ЕКРАН ПОВНІСТЮ ТЕМНІШАЄ (СМЕРТЬ/ЗАКРИТТЯ ОЧЕЙ)
        Debug.Log("Екран темнішає...");
        if (screenFaderCanvasGroup != null) screenFaderCanvasGroup.blocksRaycasts = true; // Блокуємо керування
        yield return StartCoroutine(FadeScreen(1));

        // 3. ТЕЛЕПОРТ
        TeleportPlayerToTarget();
        Debug.Log("Гравець телепортований.");

        // 4. ЕКРАН ПОВЕРТАЄТЬСЯ (ПОЧАТОК ЗНОВУ)
        Debug.Log("Екран світлішає.");
        yield return StartCoroutine(FadeScreen(0));
        if (screenFaderCanvasGroup != null) screenFaderCanvasGroup.blocksRaycasts = false; // Розблоковуємо керування

        isBusy = false;
        // Перевіряємо, чи гравець все ще поряд (наприклад, якщо точка спауну поряд з дверима-пасткою)
        if (playerNearby && promptCanvasGroup != null)
        {
            activePromptCoroutine = StartCoroutine(FadePrompt(1));
        }
    }

    private IEnumerator AnimateRotation(Quaternion fromRot, Quaternion toRot, float duration)
    {
        float t = 0;
        float rate = 1.0f / duration;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            doorModel.localRotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }
        doorModel.localRotation = toRot;
    }

    /// <summary>
    /// Зберігає позицію спауну та завантажує наступну сцену (LoadScene Behavior).
    /// </summary>
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneName) && sceneIndex < 0)
        {
            Debug.LogError("Неможливо перейти! Не вказано жодної сцени!", this);
            isBusy = false;
            return;
        }

        // Зберігання PlayerPrefs (поки що тут немає логіки зберігання, але це правильне місце)
        // Наприклад: PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
        PlayerPrefs.Save();

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
            Debug.LogError("Неможливо перейти! Не вказано жодної сцени!", this);
            isBusy = false;
        }
    }

    /// <summary>
    /// Телепортує гравця до цільового об'єкта ("govno").
    /// </summary>
    private void TeleportPlayerToTarget()
    {
        GameObject targetObject = GameObject.Find(teleportTargetName);
        GameObject player = GameObject.FindWithTag("Player");

        if (targetObject == null)
        {
            Debug.LogError($"Неможливо знайти об'єкт '{teleportTargetName}' для телепорту. Телепорт скасовано.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Неможливо знайти об'єкт гравця з тегом 'Player'. Телепорт скасовано.");
            return;
        }

        // *** Логіка скидання та телепорту ***
        // Зберігаємо компонент, щоб керувати його увімкненням/вимкненням
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Телепорт
        player.transform.position = targetObject.transform.position;
        player.transform.rotation = targetObject.transform.rotation;

        // Скидаємо швидкість (для Rigidbody)
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Повертаємо контролер
        if (cc != null) cc.enabled = true;

        Debug.Log($"Гравець успішно телепортований до '{teleportTargetName}'!");
    }

    // --- Корутина для фейду екрана ---
    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (screenFaderCanvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup для фейдера не підключено! Фейд не відбудеться.");
            yield break;
        }

        float startAlpha = screenFaderCanvasGroup.alpha;
        float time = 0;
        // Щоб уникнути ділення на нуль, якщо fadeSpeed = 0
        if (fadeSpeed <= 0) fadeSpeed = 0.01f;
        float duration = 1f / fadeSpeed;

        while (time < duration)
        {
            time += Time.deltaTime;
            screenFaderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        screenFaderCanvasGroup.alpha = targetAlpha;
    }

    // --- Корутина для тексту UI (без змін) ---
    private IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptCanvasGroup == null) yield break;

        float startAlpha = promptCanvasGroup.alpha;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }
        promptCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0)
        {
            activePromptCoroutine = null;
        }
    }
}