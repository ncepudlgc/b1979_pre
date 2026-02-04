// Assets/Turret.cs
using UnityEngine;


// Controls a ground turret that shoots at the player
public class Turret : MonoBehaviour
{
    // Reference to the player plane
    public PlaneController playerPlane;
    // Prefab for the turret's projectile
    public GameObject projectilePrefab;
    // Fire rate and range
    public float fireRate = 3f;
    public float fireRange = 100f;
    public float projectileSpawnDistance = 2f; // Distance from turret to spawn projectiles
    
    private float fireTimer = 0f;
    
    // Explosion settings
    public float explosionRadius = 8f;
    public float explosionForce = 400f;
    public GameObject explosionEffectPrefab;
    private bool hasExploded = false;
    
    // Called to initialize the turret
    void Start()
    {
        // Initialize turret
        fireTimer = Random.Range(0f, fireRate); // Randomize initial timer
        
        // Ensure correct tag is set
        gameObject.tag = "Turret";
    }
    
    // Called every frame to update turret logic
    void Update()
    {
        if (playerPlane == null)
            return;
            
        // Track player and fire if in range
        Vector3 directionToPlayer = playerPlane.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Rotate turret to face player (only on Y axis)
        Vector3 flatDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
        
        // Fire if player is in range
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate && distanceToPlayer <= fireRange)
        {
            FireAtPlayer();
            fireTimer = 0f;
        }
    }
    
    // Fires a projectile at the player
    void FireAtPlayer()
    {
        if (playerPlane == null || projectilePrefab == null)
            return;
            
        // Calculate direction to player with some inaccuracy
        Vector3 directionToPlayer = (playerPlane.transform.position - transform.position).normalized;
        
        // Calculate lead position based on player's velocity and distance
        float distanceToPlayer = Vector3.Distance(transform.position, playerPlane.transform.position);
        float timeToTarget = distanceToPlayer / 30f; // Assuming projectile speed of 30 units/sec
        Vector3 leadPosition = playerPlane.transform.position + playerPlane.transform.forward * (playerPlane.moveSpeed * timeToTarget);
        
        // Calculate direction to lead position
        Vector3 directionToLead = (leadPosition - transform.position).normalized;
        
        // Add some randomness to make it not perfectly accurate
        directionToLead += new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f)
        );
        directionToLead.Normalize();
        
        // Spawn position at a safe distance from turret
        Vector3 spawnPosition = transform.position + directionToLead * projectileSpawnDistance;
        
        // Instantiate projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(directionToLead));
        projectile.tag = "Projectile";
        
        // Add force to projectile
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = directionToLead * 30f; // Use velocity instead of AddForce for more direct control
            
            // Destroy projectile after 5 seconds
            Destroy(projectile, 5f);
        }
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
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(100);
        }

        Destroy(gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bomb") || collision.gameObject.CompareTag("Bullet"))
        {
            TriggerExplosion();
            Destroy(collision.gameObject);
        }
    }
}