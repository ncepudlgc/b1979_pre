using UnityEngine;


public class Bullet : MonoBehaviour
{
    // Reference to the GameManager for scoring
    public GameManager gameManager;
    
    void Start()
    {
        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        // Ensure correct tag is set
        gameObject.tag = "Bullet";
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check for different enemy types
        if (collision.gameObject.CompareTag("Turret"))
        {
            // Destroy the enemy
            Destroy(collision.gameObject);
            
            // Add score if GameManager is available
            if (gameManager != null)
            {
                gameManager.AddScore(100);
            }
            
            // Destroy the bullet
            Destroy(gameObject);
        }
    }
}