using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{

    /// <summary>
    /// Responsible for changing states in the game (win/lose)
    /// </summary>

    bool gameEnded = false;

    public float restartDelay = 8f;  // how much time passes after losing before the game resets to menu screen

    private readonly GameObject[] sheepList;


    public void GameOver()
    {
        if (gameEnded == false)
        {
            gameEnded = true;
            Debug.Log("GAME OVER");
            Invoke("Restart", restartDelay);
        }
    }
    public void Win(float time, int scene)
    {
        if (gameEnded == false)
        {
            gameEnded = true;
            Debug.Log("You Win");
            PlayerPrefs.SetFloat("Time", time);
            PlayerPrefs.SetInt("GameMode", scene);
            // Load Winner menu
            SceneManager.LoadScene(3);
        }
    }

    public void QuitGame() { 
     
            Application.Quit();
        }
void Restart()
    {
        SceneManager.LoadScene("menu");

    }

}
