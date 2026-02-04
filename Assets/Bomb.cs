// Assets/Bomb.cs
using UnityEngine;


public class Bomb : MonoBehaviour
{
    public float explosionRadius = 15f;
    public float explosionForce = 1000f;
    public GameObject explosionEffectPrefab; // Optional visual effect
    
    void Start()
    {
        // Destroy bomb after 10 seconds if it hasn't exploded yet
        Destroy(gameObject, 10f);
        
        // Ensure correct tag is set
        gameObject.tag = "Bomb";
    }
    
    void TriggerExplosion()
    {
        // Find all colliders within explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hit in colliders)
        {
            // Check if the hit object is an enemy or bunker using tags
            if (hit.CompareTag("Bunker") || 
                hit.CompareTag("Bogie") || 
                hit.CompareTag("Turret"))
            {
                // Destroy the enemy
                Destroy(hit.gameObject);
                
                // Add score if game manager exists
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.AddScore(150);
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
    }
    
    void OnCollisionEnter(Collision collision)
    {
        TriggerExplosion();
        Destroy(gameObject);
    }
    
    // Optional: Visualize explosion radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}