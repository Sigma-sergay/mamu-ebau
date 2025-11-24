using UnityEngine;
using System.Collections; // Потрібно для використання IEnumerator

public class FlickerLight : MonoBehaviour
{
    [Tooltip("Компонент світла, який буде моргати.")]
    public Light targetLight;

    [Tooltip("Мінімальна інтенсивність світла.")]
    public float minIntensity = 0.5f;

    [Tooltip("Максимальна інтенсивність світла.")]
    public float maxIntensity = 2.0f;

    [Tooltip("Як часто буде змінюватись інтенсивність (в секундах).")]
    public float flickerSpeed = 0.1f;

    private float originalIntensity;

    void Start()
    {
        // Перевіряємо, чи компонент світла був призначений.
        if (targetLight == null)
        {
            // Якщо не призначений, спробуємо знайти його на цьому ж об'єкті.
            targetLight = GetComponent<Light>();
        }

        if (targetLight != null)
        {
            // Зберігаємо оригінальну інтенсивність.
            originalIntensity = targetLight.intensity;
            // Запускаємо корутину для моргання.
            StartCoroutine(DoFlicker());
        }
        else
        {
            Debug.LogError("Компонент Light не знайдено на об'єкті " + gameObject.name + "!");
        }
    }

    // Корутина, яка відповідає за періодичне моргання
    IEnumerator DoFlicker()
    {
        while (true) // Вічний цикл
        {
            // Генеруємо випадкове значення інтенсивності між Min та Max
            float randomIntensity = Random.Range(minIntensity, maxIntensity);

            // Застосовуємо нову інтенсивність
            targetLight.intensity = randomIntensity;

            // Чекаємо заданий інтервал перед наступною зміною
            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    // При зупинці або знищенні об'єкта повертаємо початкову інтенсивність.
    void OnDisable()
    {
        if (targetLight != null)
        {
            targetLight.intensity = originalIntensity;
        }
    }
}