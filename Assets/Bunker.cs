// Assets/Bunker.cs
using UnityEngine;


// Represents a destructible ground bunker
public class Bunker : MonoBehaviour
{
    // Health of the bunker
    public int health = 1;
    
    // Reference to game manager for scoring
    private GameManager gameManager;
    
    // Explosion settings
    public float explosionRadius = 8f;
    public float explosionForce = 400f;
    public GameObject explosionEffectPrefab;
    private bool hasExploded = false;
    
    void Start()
    {
        // Find the game manager
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Ensure correct tag is set
        gameObject.tag = "Bunker";
    }
    
    public void TriggerExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Find all colliders within explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hit in colliders)
        {
            // Check if the hit object is an enemy or bunker using tags
            if (hit.CompareTag("Bunker") || hit.CompareTag("Turret") || hit.CompareTag("Bogie"))
            {
                // Trigger explosion on the hit object
                if (hit.CompareTag("Bunker"))
                {
                    hit.GetComponent<Bunker>()?.TriggerExplosion();
                }
                else if (hit.CompareTag("Turret"))
                {
                    hit.GetComponent<Turret>()?.TriggerExplosion();
                }
                else if (hit.CompareTag("Bogie"))
                {
                    hit.GetComponent<Bogie>()?.TriggerExplosion();
                }
            }
            
            // Apply explosion force to rigidbodies
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        
        // Spawn explosion effect if available
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Add score
        if (gameManager != null)
        {
            gameManager.AddScore(100);
        }

        Destroy(gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if hit by a bomb
        if (collision.gameObject.CompareTag("Bomb"))
        {
            TriggerExplosion();
            Destroy(collision.gameObject);
        }
    }
}