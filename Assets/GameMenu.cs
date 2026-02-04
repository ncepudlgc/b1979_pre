using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public GameObject resumeButton;
    public GameObject restartButton;
    public GameObject quitButton;
    
    private GameManager gameManager;
    
    void Start()
    {
        // Get reference to GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene!");
        }
        
        // Wire up button click handlers
        if (resumeButton != null)
        {
            Button button = resumeButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ResumeGame);
            }
            else
            {
                Debug.LogError("Resume button GameObject does not have a Button component!");
            }
        }
        else
        {
            Debug.LogError("Resume button GameObject is not assigned in GameMenu!");
        }
        
        if (restartButton != null)
        {
            Button button = restartButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(RestartGame);
            }
            else
            {
                Debug.LogError("Restart button GameObject does not have a Button component!");
            }
        }
        else
        {
            Debug.LogError("Restart button GameObject is not assigned in GameMenu!");
        }
        
        if (quitButton != null)
        {
            Button button = quitButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(QuitToMenu);
            }
            else
            {
                Debug.LogError("Quit button GameObject does not have a Button component!");
            }
        }
        else
        {
            Debug.LogError("Quit button GameObject is not assigned in GameMenu!");
        }
        
        // Hide the menu panel initially
        if (gameManager != null && gameManager.menuPanel != null)
        {
            gameManager.menuPanel.SetActive(false);
        }
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// Resumes the game and hides the pause menu
    /// </summary>
    public void ResumeGame()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }
    
    /// <summary>
    /// Restarts the game by reloading the Game scene
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(0); // Game scene has Build Index 0
    }
    
    /// <summary>
    /// Quits to the main menu by loading the Menu scene
    /// </summary>
    public void QuitToMenu()
    {
        SceneManager.LoadScene(1); // Menu scene has Build Index 1
    }
}
