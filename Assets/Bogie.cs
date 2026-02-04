// Assets/Bogie.cs
using Unity.VisualScripting;
using UnityEngine;


// Controls an enemy bogie (aircraft) that flies in patterns and shoots at the player
public class Bogie : MonoBehaviour
{
    // Reference to the player plane
    public PlaneController playerPlane;
    // Prefab for the bogie's projectile
    public GameObject projectilePrefab;
    // Movement pattern settings
    public float moveSpeed = 15f;
    public float patternAmplitude = 10f;
    public float patternFrequency = 0.5f;
    // Fire rate and range
    public float fireRate = 2f;
    public float fireRange = 50f;
    public float projectileSpawnDistance = 2f;
    
    private float fireTimer = 0f;
    private Vector3 startPosition;
    private float timeSinceSpawn = 0f;
    private bool movingRight = true;
    // Movement control variables
    private float pauseTimer = 0f;
    private float pauseDuration = 2f;
    private float pauseInterval = 5f;
    private bool isPaused = false;
    private float mediumDistanceFromPlayer = 40f; // Distance to maintain from player
    private GameManager gameManager;
    
    // Ammo drop settings
    public GameObject ammoPrefab;
    public GameObject healthPrefab; // Add health prefab reference
    public float ammoDropChance = 0.5f; // 50% chance to drop ammo
    public float healthDropChance = 0.5f; // 50% chance to drop health
    
    // Explosion settings
    public float explosionRadius = 10f;
    public float explosionForce = 500f;
    public GameObject explosionEffectPrefab;
    private bool hasExploded = false;
    private bool isFalling = false;
    private float fallSpeed = 5f;
    // Minimum height above ground
    public float minHeightAboveGround = 5f;
    
    // Called to initialize the bogie
    void Start()
    {
        // Initialize bogie
        startPosition = transform.position;
        fireTimer = Random.Range(0f, fireRate);
        movingRight = Random.value > 0.5f; // Random initial direction
        pauseTimer = Random.Range(0f, pauseInterval); // Random initial pause timer
        
        // Get GameManager reference
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Ensure correct tag is set
        gameObject.tag = "Bogie";
    }
    
    // Called every frame to update bogie logic
    void Update()
    {
        if (isFalling)
        {
            // Add downward velocity
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            // Add more forward momentum
            transform.position += transform.forward * moveSpeed * 2f * Time.deltaTime;
            
            return;
        }

        if (playerPlane == null || gameManager == null)
            return;
            
        // Handle pause logic
        pauseTimer += Time.deltaTime;
        if (pauseTimer >= pauseInterval)
        {
            isPaused = !isPaused;
            pauseTimer = 0f;
            if (isPaused)
            {
                pauseDuration = Random.Range(1f, 3f); // Random pause duration between 1-3 seconds
            }
        }
        
        if (isPaused)
        {
            if (pauseTimer >= pauseDuration)
            {
                isPaused = false;
                pauseTimer = 0f;
            }
            // During pause, only move forward
            Vector3 forwardPosition = transform.position + playerPlane.transform.forward * moveSpeed * Time.deltaTime;
            transform.position = forwardPosition;
            return; // Skip other movement
        }
            
        // Move in pattern and follow player's forward direction
        timeSinceSpawn += Time.deltaTime;
        
        // Calculate sine wave pattern movement
        float xOffset = Mathf.Sin(timeSinceSpawn * patternFrequency) * patternAmplitude;
        
        // Calculate target position relative to player - maintain medium distance
        Vector3 targetPosition = playerPlane.transform.position + playerPlane.transform.forward * mediumDistanceFromPlayer;
        targetPosition.x += xOffset; // Add pattern movement
        
        // Constrain to ground plane x-axis limits using GameManager's spawn limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, gameManager.GetGroundSpawnMinX() + 5f, gameManager.GetGroundSpawnMaxX() - 5f);
        
        // Ensure minimum height above ground
        GameObject groundPlane = GameObject.FindGameObjectWithTag("Ground");
        float groundHeight = 0f;
        if (groundPlane != null)
        {
            groundHeight = groundPlane.transform.position.y + groundPlane.transform.localScale.y/2;
        }
        
        targetPosition.y = Mathf.Max(groundHeight + minHeightAboveGround, 
                                     Mathf.Clamp(targetPosition.y, 15f, 35f)); // Keep at a good height
        
        // Move towards target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Look at the direction of movement
        Vector3 lookDirection = playerPlane.transform.forward;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
        // Fire at player if in range
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            // Check if player is in front of the bogie
            Vector3 directionToPlayer = playerPlane.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer < 45f && directionToPlayer.magnitude < fireRange)
            {
                FireAtPlayer();
                fireTimer = 0f;
            }
        }
        
        // Only destroy if way too far behind
        if (Vector3.Distance(transform.position, playerPlane.transform.position) > 200f)
        {
            Destroy(gameObject);
        }
    }
    
    // Fires a projectile at the player
    void FireAtPlayer()
    {
        if (playerPlane == null || projectilePrefab == null)
            return;
            
        // Calculate direction to player with some inaccuracy
        Vector3 directionToPlayer = (playerPlane.transform.position - transform.position).normalized;
        
        // Add some randomness to make it not perfectly accurate
        directionToPlayer += new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f)
        );
        directionToPlayer.Normalize();
        
        // Spawn position at a safe distance from bogie
        Vector3 spawnPosition = transform.position + directionToPlayer * projectileSpawnDistance;
        
        // Instantiate projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(directionToPlayer));
        projectile.tag = "Projectile";
        
        // Add force to projectile
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = directionToPlayer * 30f; // Use velocity instead of AddForce for more direct control
            
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
                    hit.GetComponent<Bogie>()?.HandleDrop();
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

    public void HandleDrop(){
        // Always drop an item or crash
        float randomValue = Random.value;
            
        // 50% chance to start falling when shot
        if (randomValue < 0.5f && !isFalling)
        {
            isFalling = true;
            // Add some forward momentum
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.linearVelocity = transform.forward * moveSpeed * 2f;
            }
            return;
        }
            
        // Always drop an item if not falling
        if (!isFalling)
        {
            // Determine which item to drop
            float dropType = Random.value;
            if (dropType < healthDropChance && healthPrefab != null)
            {
                Instantiate(healthPrefab, transform.position, Quaternion.identity);
            }
            else if (ammoPrefab != null)
            {
                Instantiate(ammoPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            HandleDrop();
            // Destroy the bullet
            Destroy(collision.gameObject);
        }
        else if (isFalling)
        {
            if (collision.gameObject.CompareTag("Bullet")){
                Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
                Destroy(collision.gameObject);
            }
            // Explode on any collision while falling
            TriggerExplosion();
        }
    }
}
