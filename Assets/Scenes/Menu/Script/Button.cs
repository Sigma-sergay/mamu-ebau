using UnityEngine;
using UnityEngine.SceneManagement; 

public class Button : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("started"); 
    }

    public void QuitApplication()
    {
        Debug.Log("Вихід з гри...");

        
        Application.Quit();

       
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
