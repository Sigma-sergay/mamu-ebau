using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        // Перевіряємо чи є збережена позиція спавну
        if (PlayerPrefs.GetInt("ShouldSpawn", 0) == 1)
        {
            // Читаємо збережені координати
            float x = PlayerPrefs.GetFloat("SpawnX", 0);
            float y = PlayerPrefs.GetFloat("SpawnY", 1);
            float z = PlayerPrefs.GetFloat("SpawnZ", 0);
            float rotY = PlayerPrefs.GetFloat("SpawnRotY", 0);

            // Телепортуємо гравця
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false; // Вимикаємо щоб можна було телепортувати
                transform.position = new Vector3(x, y, z);
                transform.rotation = Quaternion.Euler(0, rotY, 0);
                controller.enabled = true; // Вмикаємо назад
            }
            else
            {
                // Якщо немає CharacterController
                transform.position = new Vector3(x, y, z);
                transform.rotation = Quaternion.Euler(0, rotY, 0);
            }

            Debug.Log($"Гравець заспавнився на позиції: {transform.position}");

            // Скидаємо прапорець
            PlayerPrefs.SetInt("ShouldSpawn", 0);
            PlayerPrefs.Save();
        }
    }
}