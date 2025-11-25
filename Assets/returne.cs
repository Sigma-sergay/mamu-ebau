using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Повісь цей скрипт на порожній GameObject в ПЕРШІЙ сцені.
/// Він буде автоматично вивантажувати та перезавантажувати деактивовані сцени.
/// </summary>
public class SceneResetter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Через скільки секунд після деактивації вивантажити сцену")]
    public float unloadDelay = 2f;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Перевіряємо всі завантажені сцени
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Якщо сцена не активна і всі об'єкти вимкнені
            if (scene.isLoaded && scene != SceneManager.GetActiveScene())
            {
                bool allInactive = true;
                foreach (GameObject obj in scene.GetRootGameObjects())
                {
                    if (obj.activeSelf)
                    {
                        allInactive = false;
                        break;
                    }
                }

                // Якщо всі об'єкти вимкнені - вивантажуємо сцену
                if (allInactive && scene.rootCount > 0)
                {
                    StartCoroutine(UnloadSceneDelayed(scene.name));
                }
            }
        }
    }

    System.Collections.IEnumerator UnloadSceneDelayed(string sceneName)
    {
        yield return new WaitForSeconds(unloadDelay);

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded && scene != SceneManager.GetActiveScene())
        {
            Debug.Log($"Вивантажую сцену: {sceneName}");
            yield return SceneManager.UnloadSceneAsync(sceneName);

            // Звільняємо пам'ять
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}

/// <summary>
/// АЛЬТЕРНАТИВНИЙ ВАРІАНТ: Додай цей метод в DoorTrigger якщо не хочеш окремий скрипт.
/// Викликай UnloadPreviousScene() після активації нової сцени.
/// </summary>
/*
IEnumerator UnloadPreviousScene(string sceneName)
{
    yield return new WaitForSeconds(2f);
    
    Scene scene = SceneManager.GetSceneByName(sceneName);
    if (scene.isLoaded)
    {
        Debug.Log($"Вивантажую сцену: {sceneName}");
        yield return SceneManager.UnloadSceneAsync(sceneName);
        yield return Resources.UnloadUnusedAssets();
    }
}
*/