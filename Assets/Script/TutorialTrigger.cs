using UnityEngine;
using TMPro; // Для тексту
using System.Collections;
using UnityEngine.UI; // Для створення Canvas

public class TutorialTrigger : MonoBehaviour
{
    [Header("Налаштування Навчання")]
    [TextArea(3, 5)] // Робить поле для тексту більшим в інспекторі
    [Tooltip("Текст підказки, який побачить гравець.")]
    public string tutorialText = "Використовуй WASD для переміщення";

    [Tooltip("Чи показувати це лише один раз? (Наприклад, навчання стрибку).")]
    public bool showOnlyOnce = false;

    [Header("Налаштування Вигляду")]
    public float fadeDuration = 0.5f; // Швидкість появи
    public Color textColor = Color.yellow; // Колір тексту

    // Приватні змінні
    private static Canvas tutorialCanvas; // Спільний канвас для всіх туторіалів
    private static TextMeshProUGUI textComponent;
    private static CanvasGroup canvasGroup;

    private bool hasTriggered = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Переконуємось, що колайдер є тригером
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Ініціалізуємо UI (лише один раз для всієї гри)
        SetupTutorialUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showOnlyOnce && hasTriggered) return; // Якщо вже було - не показуємо

            ShowTutorial(true);
            hasTriggered = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowTutorial(false);
        }
    }

    // --- ЛОГІКА UI ---

    private void ShowTutorial(bool show)
    {
        if (textComponent == null) return;

        // Якщо показуємо - оновлюємо текст
        if (show) textComponent.text = tutorialText;

        // Запускаємо плавну анімацію
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeUI(show ? 1 : 0));
    }

    private IEnumerator FadeUI(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    // --- АВТОМАТИЧНЕ СТВОРЕННЯ UI ---
    // Цей метод створить текст зверху екрана сам, тобі не треба нічого тягнути
    private void SetupTutorialUI()
    {
        // Якщо UI вже існує (створений іншим тригером) - не робимо новий
        if (textComponent != null) return;

        // Шукаємо існуючий Canvas або створюємо новий
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("Tutorial_Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Створюємо об'єкт для тексту
        GameObject textObj = new GameObject("Tutorial_Text_Auto");
        textObj.transform.SetParent(mainCanvas.transform, false);

        // Налаштовуємо текст
        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = 40;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = textColor;
        textComponent.enableWordWrapping = true;

        // Додаємо тінь для краси
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.outlineWidth = 0.2f;
        textComponent.outlineColor = new Color32(0, 0, 0, 255);

        // Розміщуємо ЗВЕРХУ по центру
        RectTransform rt = textComponent.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.85f); // 85% висоти екрана (верх)
        rt.anchorMax = new Vector2(0.5f, 0.85f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 200); // Ширина і висота блоку тексту

        // Додаємо CanvasGroup для прозорості
        canvasGroup = textObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0; // Приховано за замовчуванням
        canvasGroup.blocksRaycasts = false;
    }
}