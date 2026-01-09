using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManagerScript : MonoBehaviour
{
    
    // Load scene normally
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Load scene additively
    public void LoadSceneAdditive(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    // Unload scene
    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    // UI Button: Load scene
    public void OnLoadSceneButtonClicked()
    {
        LoadScene("YourSceneName"); // Replace with actual scene name
    }

    // UI Button: Load additive
    public void OnLoadSceneAdditiveButtonClicked()
    {
        LoadSceneAdditive("YourAdditiveSceneName"); // Replace with actual scene name
    }

    // UI Button: Unload scene
    public void OnUnloadSceneButtonClicked()
    {
        UnloadScene("YourAdditiveSceneName"); // Replace with actual scene name
    }

    // Quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
