using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // <--- ОБОВ'ЯЗКОВО ДЛЯ ЗАВАНТАЖЕННЯ СЦЕН

public class LevelQuestionTrigger : MonoBehaviour
{
    // --- ТЕКСТ ПИТАННЯ ---
    [Header("Текст Питання")]
    [TextArea(3, 10)]
    public string questionText = "Напишіть сюди ваше питання...";

    [Tooltip("Текст, який з'явиться, коли час вийде (перед завантаженням меню).")]
    public string timeOutText = "Час вийшов!";

    // --- НАЛАШТУВАННЯ UI ---
    [Header("UI Елементи")]
    [Tooltip("Перетягніть сюди TextMeshPro з Canvas.")]
    public TextMeshProUGUI questionTextUI;

    [Tooltip("Перетягніть сюди CanvasGroup (на батьківському об'єкті тексту).")]
    public CanvasGroup questionCanvasGroup;

    // --- НАЛАШТУВАННЯ ЧАСУ ТА СЦЕНИ ---
    [Header("Налаштування")]
    [Tooltip("Скільки секунд показувати питання.")]
    public float timeLimitSeconds = 30f;

    [Tooltip("Назва сцени, яку завантажити після смерті (наприклад, Menu).")]
    public string timeOutSceneName = "Menu"; // <--- НОВЕ ПОЛЕ

    [Tooltip("Як швидко текст з'являється (сек).")]
    public float fadeDuration = 1.0f;

    [Tooltip("Тег гравця.")]
    public string playerTag = "Player";

    private bool isActive = false;

    void Start()
    {
        // Ховаємо текст на старті
        if (questionCanvasGroup != null)
        {
            questionCanvasGroup.alpha = 0;
            questionCanvasGroup.blocksRaycasts = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isActive)
        {
            StartCoroutine(ShowQuestionAndFailRoutine());
        }
    }

    private IEnumerator ShowQuestionAndFailRoutine()
    {
        isActive = true;

        // 1. Встановлюємо текст питання
        if (questionTextUI != null) questionTextUI.text = questionText;

        // 2. Плавно показуємо
        yield return StartCoroutine(FadeUI(1));

        Debug.Log("Питання показано, таймер пішов...");

        // 3. Чекаємо 30 секунд
        yield return new WaitForSeconds(timeLimitSeconds);

        // 4. Час вийшов! Показуємо текст поразки
        if (questionTextUI != null) questionTextUI.text = timeOutText;
        Debug.Log("Час вийшов. Завантаження меню...");

        // 5. Чекаємо 2 секунди, щоб гравець встиг прочитати "Час вийшов"
        yield return new WaitForSeconds(2.0f);

        // 6. ЗАВАНТАЖУЄМО МЕНЮ
        LoadMenuScene();
    }

    private void LoadMenuScene()
    {
        // Перевіряємо, чи вписали назву сцени
        if (!string.IsNullOrEmpty(timeOutSceneName))
        {
            // Розблокуємо курсор, якщо він був схований (для меню це важливо)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(timeOutSceneName);
        }
        else
        {
            Debug.LogError("Не вказано назву сцени (Time Out Scene Name) в інспекторі!");
        }
    }

    private IEnumerator FadeUI(float targetAlpha)
    {
        if (questionCanvasGroup == null) yield break;

        float startAlpha = questionCanvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            questionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        questionCanvasGroup.alpha = targetAlpha;
    }
}