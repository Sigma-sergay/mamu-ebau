using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Налаштування мигтіння")]
    [Tooltip("Мінімальний час між миготіннями")]
    public float minFlickerInterval = 0.1f;

    [Tooltip("Максимальний час між миготіннями")]
    public float maxFlickerInterval = 2.0f;

    [Tooltip("Мінімальна тривалість мигтіння")]
    public float minFlickerDuration = 0.05f;

    [Tooltip("Максимальна тривалість мигтіння")]
    public float maxFlickerDuration = 0.3f;

    [Tooltip("Ймовірність мигтіння (0-1)")]
    [Range(0f, 1f)]
    public float flickerChance = 0.3f;

    private Light lightComponent;
    private float nextFlickerTime;
    private bool isFlickering = false;
    private float flickerEndTime;
    private float originalIntensity;

    void Start()
    {
        // Отримуємо компонент Light
        lightComponent = GetComponent<Light>();

        if (lightComponent == null)
        {
            Debug.LogError("Компонент Light не знайдено! Додайте Light компонент до об'єкту.");
            enabled = false;
            return;
        }

        // Зберігаємо оригінальну яскравість
        originalIntensity = lightComponent.intensity;

        // Встановлюємо час наступного мигтіння
        SetNextFlickerTime();
    }

    void Update()
    {
        if (isFlickering)
        {
            // Якщо мигтіння закінчилось
            if (Time.time >= flickerEndTime)
            {
                lightComponent.enabled = true;
                lightComponent.intensity = originalIntensity;
                isFlickering = false;
                SetNextFlickerTime();
            }
            else
            {
                // Випадкове вмикання/вимикання під час мигтіння
                lightComponent.enabled = Random.value > 0.5f;
                if (lightComponent.enabled)
                {
                    lightComponent.intensity = originalIntensity * Random.Range(0.3f, 1.0f);
                }
            }
        }
        else
        {
            // Перевіряємо, чи настав час мигтіння
            if (Time.time >= nextFlickerTime)
            {
                // Випадково вирішуємо, чи буде мигтіння
                if (Random.value <= flickerChance)
                {
                    StartFlicker();
                }
                else
                {
                    SetNextFlickerTime();
                }
            }
        }
    }

    void StartFlicker()
    {
        isFlickering = true;
        flickerEndTime = Time.time + Random.Range(minFlickerDuration, maxFlickerDuration);
    }

    void SetNextFlickerTime()
    {
        nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
    }
}