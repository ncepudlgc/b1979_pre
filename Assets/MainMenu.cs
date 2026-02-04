using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject startGameButton;
    
    void Start()
    {
        // Wire up the start button to load the game scene
        if (startGameButton != null)
        {
            Button button = startGameButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(StartGame);
            }
            else
            {
                Debug.LogError("Start button GameObject does not have a Button component!");
            }
        }
        else
        {
            Debug.LogError("Start button GameObject is not assigned in MainMenu!");
        }
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// Loads the Game scene (Build Index 0) to start the game
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(0); // Game scene has Build Index 0
    }
}
