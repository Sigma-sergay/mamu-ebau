using System.Collections;
using TMPro;
using UnityEngine;

public class LevelQuestionTrigger : MonoBehaviour
{
    // --- ТЕКСТ ПИТАННЯ (Пишемо прямо на об'єкті) ---
    [Header("Текст Питання")]
    [TextArea(3, 10)]
    public string questionText = "Напишіть сюди ваше питання...";

    [Tooltip("Текст, який з'явиться, коли час вийде.")]
    public string timeOutText = "Час вийшов!";

    // --- НАЛАШТУВАННЯ UI ---
    [Header("UI Елементи")]
    [Tooltip("Перетягніть сюди TextMeshPro з Canvas.")]
    public TextMeshProUGUI questionTextUI;

    [Tooltip("Перетягніть сюди CanvasGroup (на батьківському об'єкті тексту).")]
    public CanvasGroup questionCanvasGroup;

    // --- НАЛАШТУВАННЯ ЧАСУ ---
    [Header("Таймер")]
    [Tooltip("Скільки секунд показувати питання.")]
    public float timeLimitSeconds = 30f;

    [Tooltip("Як швидко текст з'являється/зникає (сек).")]
    public float fadeDuration = 1.0f;

    [Tooltip("Тег гравця.")]
    public string playerTag = "Player";

    private bool isActive = false;
    private Coroutine activeCoroutine;

    void Start()
    {
        // Ховаємо текст на старті
        if (questionCanvasGroup != null)
        {
            questionCanvasGroup.alpha = 0;
            questionCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("Не забудьте прикріпити CanvasGroup в інспекторі!", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Якщо зайшов гравець і тригер ще не активований
        if (other.CompareTag(playerTag) && !isActive)
        {
            StartCoroutine(ShowQuestionRoutine());
        }
    }

    private IEnumerator ShowQuestionRoutine()
    {
        isActive = true;

        // 1. Встановлюємо текст питання
        if (questionTextUI != null)
        {
            questionTextUI.text = questionText;
        }

        // 2. Плавно показуємо (Fade In)
        yield return StartCoroutine(FadeUI(1));

        Debug.Log("Питання показано, таймер пішов...");

        // 3. Чекаємо 30 секунд (або скільки вказано)
        yield return new WaitForSeconds(timeLimitSeconds);

        Debug.Log("Час вийшов.");

        // 4. Міняємо текст на "Час вийшов"
        if (questionTextUI != null)
        {
            questionTextUI.text = timeOutText;
        }

        // 5. Чекаємо ще трохи (3 сек), щоб гравець прочитав, що час вийшов
        yield return new WaitForSeconds(3.0f);

        // 6. Плавно ховаємо (Fade Out)
        yield return StartCoroutine(FadeUI(0));

        // Опціонально: знищити тригер, щоб він більше не спрацьовував
        // Destroy(gameObject); 

        isActive = false; // Якщо хочете, щоб можна було активувати повторно, залиште false
    }

    // Допоміжна функція для плавної прозорості
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