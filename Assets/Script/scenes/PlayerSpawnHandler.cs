using UnityEngine;

// Ми змінили назву класу, щоб вона не конфліктувала зі старим
public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        // Перевіряємо, чи є сигнал від дверей, що треба перемістити гравця
        if (PlayerPrefs.GetInt("ShouldSpawn") == 1)
        {
            // Зчитуємо збережені координати
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            float z = PlayerPrefs.GetFloat("SpawnZ");
            float rotY = PlayerPrefs.GetFloat("SpawnRotY");

            Debug.Log($"[PlayerSpawnHandler] Телепортую гравця на: {x}, {y}, {z}");

            // Вимикаємо контролер на секунду, щоб телепорт спрацював без глюків фізики
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Ставимо гравця на нове місце
            transform.position = new Vector3(x, y, z);
            transform.rotation = Quaternion.Euler(0, rotY, 0);

            // Вмикаємо контролер назад
            if (cc != null) cc.enabled = true;

            // Скидаємо прапорець, щоб при наступному перезапуску гри гравець не стрибав
            PlayerPrefs.SetInt("ShouldSpawn", 0);
            PlayerPrefs.Save();
        }
    }
}