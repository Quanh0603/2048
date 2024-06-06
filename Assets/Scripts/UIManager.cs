using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameOverScreen _gameOverScreen;
    
    public void RestartScene()
    {
        var activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
    public void RestartGameOver()
    {
        _gameOverScreen.SetGameOver(false);
        var activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
