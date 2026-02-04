using UnityEngine;
using System.Collections.Generic;

// A special bogie that can attract bullets, absorb them, and fire them back as a combined projectile
public class MagnetBogie : MonoBehaviour
{
    [Header("Base Bogie Settings")]
    public PlaneController playerPlane;
    public float moveSpeed = 15f;
    public float patternAmplitude = 10f;
    public float patternFrequency = 0.5f;
    public float minHeightAboveGround = 5f;
    
    [Header("Magnet Settings")]
    public float magnetRange = 20f;
    public float magnetForce = 50f;
    public int maxAbsorbedBullets = 10;
    public float absorptionRadius = 3f;
    public float absorptionDuration = 5f;
    
    [Header("Combined Projectile Settings")]
    public GameObject combinedProjectilePrefab;
    public float baseProjectileSpeed = 30f;
    public float speedScalePerBullet = 5f;
    public float baseSizeScale = 1f;
    public float sizeScalePerBullet = 0.3f;
    public float projectileSpawnDistance = 3f;
    
    [Header("Cooldown Settings")]
    public float cooldownDuration = 5f;
    public float vulnerabilityDuration = 3f;
    
    [Header("Drop Settings")]
    public GameObject ammoPrefab;
    public GameObject healthPrefab;
    public float ammoDropChance = 0.5f;
    public float healthDropChance = 0.5f;
    
    [Header("Explosion Settings")]
    public float explosionRadius = 10f;
    public float explosionForce = 500f;
    public GameObject explosionEffectPrefab;
    
    // State management
    private enum MagnetBogieState
    {
        Attracting,
        Vulnerable  // Removed Cooldown as per request
    }

    // Add private variables for absorption timing and original scale
    private float absorptionTimer = 0f;
    private bool hasStartedAbsorbing = false;
    private Vector3 originalScale;
    
    private MagnetBogieState currentState = MagnetBogieState.Attracting;
    private float stateTimer = 0f;
    private List<GameObject> absorbedBullets = new List<GameObject>();
    private Vector3 startPosition;
    private float timeSinceSpawn = 0f;
    private GameManager gameManager;
    private bool hasExploded = false;
    private bool isFalling = false;
    private float fallSpeed = 5f;
    
    // Visual feedback
    private Renderer bogieRenderer;
    private Color originalColor;
    private Color attractingColor = Color.cyan;
    private Color cooldownColor = Color.yellow;
    private Color vulnerableColor = Color.red;

    void Start()
    {
        startPosition = transform.position;
        gameManager = FindFirstObjectByType<GameManager>();
        gameObject.tag = "Bogie";

        // Get renderer for visual feedback
        bogieRenderer = GetComponent<Renderer>();
        if (bogieRenderer != null)
        {
            originalColor = bogieRenderer.material.color;
        }

        // Start in attracting state
        SetState(MagnetBogieState.Attracting);
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        if (isFalling)
        {
            HandleFalling();
            return;
        }
        
        if (playerPlane == null || gameManager == null)
            return;
        
        // Update state timer and absorption timer
        stateTimer += Time.deltaTime;
        if (hasStartedAbsorbing) absorptionTimer += Time.deltaTime;  // Only increment if absorption has started
        
        // Handle state transitions and behaviors
        switch (currentState)
        {
            case MagnetBogieState.Attracting:
                HandleAttractingState();
                break;
            case MagnetBogieState.Vulnerable:
                HandleVulnerableState();
                break;
        }
        
        // Move in pattern (same as regular bogie)
        MoveInPattern();
        
        // Destroy if too far away
        if (Vector3.Distance(transform.position, playerPlane.transform.position) > 200f)
        {
            Destroy(gameObject);
        }
    }
    
    void HandleAttractingState()
    {
        AttractBullets();
        
        // Modified firing condition: Fire if max absorbed or absorption time exceeded after starting
        if (absorbedBullets.Count >= maxAbsorbedBullets || 
            (hasStartedAbsorbing && absorptionTimer >= absorptionDuration && absorbedBullets.Count > 0))
        {
            FireCombinedProjectile();
            SetState(MagnetBogieState.Vulnerable);  // Go directly to Vulnerable after firing
        }
    }
    
    void HandleVulnerableState()
    {
        if (stateTimer >= vulnerabilityDuration)
        {
            SetState(MagnetBogieState.Attracting);
        }
    }
    
    void HandleFalling()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        transform.position += transform.forward * moveSpeed * 2f * Time.deltaTime;
    }
    
    void SetState(MagnetBogieState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        // Reset absorption-related variables when returning to Attracting
        if (newState == MagnetBogieState.Attracting)
        {
            hasStartedAbsorbing = false;
            absorptionTimer = 0f;
        }
        
        // Update visual feedback (removed Cooldown color)
        if (bogieRenderer != null)
        {
            switch (newState)
            {
                case MagnetBogieState.Attracting:
                    bogieRenderer.material.color = attractingColor;
                    break;
                case MagnetBogieState.Vulnerable:
                    bogieRenderer.material.color = vulnerableColor;
                    break;
            }
        }
    }
    
    void AttractBullets()
    {
        // Find all bullets within magnet range
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, magnetRange);
        
        foreach (Collider obj in nearbyObjects)
        {
            if (obj.CompareTag("Bullet"))
            {
                GameObject bullet = obj.gameObject;
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                
                if (bulletRb != null)
                {
                    // Calculate direction from bullet to this bogie
                    Vector3 direction = (transform.position - bullet.transform.position).normalized;
                    float distance = Vector3.Distance(transform.position, bullet.transform.position);
                    
                    // Apply magnetic force (stronger when closer)
                    float force = magnetForce / (distance + 1f);
                    bulletRb.AddForce(direction * force);
                    
                    // Check if bullet is close enough to absorb
                    if (distance <= absorptionRadius && absorbedBullets.Count < maxAbsorbedBullets)
                    {
                        AbsorbBullet(bullet);
                    }
                }
            }
        }
    }
    
    void AbsorbBullet(GameObject bullet)
    {
        if (!absorbedBullets.Contains(bullet))
        {
            absorbedBullets.Add(bullet);

            // Hide the bullet but don't destroy it yet (we'll use it for the combined projectile)
            bullet.SetActive(false);

            // Disable bullet's collider and rigidbody
            Collider bulletCollider = bullet.GetComponent<Collider>();
            if (bulletCollider != null) bulletCollider.enabled = false;

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null) bulletRb.isKinematic = true;
            
            // Add: Start absorption timer on first bullet
            if (absorbedBullets.Count == 1)
            {
                hasStartedAbsorbing = true;
                absorptionTimer = 0f;  // Reset to ensure full duration
            }
            
            // Add: Visual feedback - increase scale based on absorbed count
            float scaleFactor = 1f + (absorbedBullets.Count * 0.1f);  // e.g., 10% larger per bullet
            transform.localScale = originalScale * scaleFactor;
            }
    }

    void FireCombinedProjectile()
    {
        if (playerPlane == null || combinedProjectilePrefab == null || absorbedBullets.Count == 0)
            return;

        // Calculate direction to player
        Vector3 directionToPlayer = (playerPlane.transform.position - transform.position).normalized;
        Vector3 spawnPosition = transform.position + directionToPlayer * projectileSpawnDistance;

        // Create combined projectile
        GameObject combinedProjectile = Instantiate(combinedProjectilePrefab, spawnPosition,
            Quaternion.LookRotation(directionToPlayer));

        // Scale projectile based on absorbed bullets
        int bulletCount = absorbedBullets.Count;
        float sizeScale = baseSizeScale + (sizeScalePerBullet * bulletCount);
        float speed = baseProjectileSpeed + (speedScalePerBullet * bulletCount);

        combinedProjectile.transform.localScale *= sizeScale;
        combinedProjectile.tag = "Projectile";

        // Add velocity to projectile
        Rigidbody projectileRb = combinedProjectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = directionToPlayer * speed;
            Destroy(combinedProjectile, 8f); // Destroy after 8 seconds
        }

        // Clean up absorbed bullets
        foreach (GameObject bullet in absorbedBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        absorbedBullets.Clear();
        transform.localScale = originalScale;
        hasStartedAbsorbing = false;
        absorptionTimer = 0f;
    }
    
    void MoveInPattern()
    {
        timeSinceSpawn += Time.deltaTime;
        
        // Calculate sine wave pattern movement
        float xOffset = Mathf.Sin(timeSinceSpawn * patternFrequency) * patternAmplitude;
        
        // Calculate target position relative to player
        float mediumDistanceFromPlayer = 40f;
        Vector3 targetPosition = playerPlane.transform.position + playerPlane.transform.forward * mediumDistanceFromPlayer;
        targetPosition.x += xOffset;
        
        // Constrain to ground plane limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, 
            gameManager.GetGroundSpawnMinX() + 5f, 
            gameManager.GetGroundSpawnMaxX() - 5f);
        
        // Ensure minimum height above ground
        GameObject groundPlane = GameObject.FindGameObjectWithTag("Ground");
        float groundHeight = 0f;
        if (groundPlane != null)
        {
            groundHeight = groundPlane.transform.position.y + groundPlane.transform.localScale.y/2;
        }
        
        targetPosition.y = Mathf.Max(groundHeight + minHeightAboveGround, 
            Mathf.Clamp(targetPosition.y, 15f, 35f));
        
        // Move towards target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Look at movement direction
        Vector3 lookDirection = playerPlane.transform.forward;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
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
            // Trigger explosions on other objects
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
                Bogie bogie = hit.GetComponent<Bogie>();
                if (bogie != null)
                {
                    bogie.HandleDrop();
                }
                else
                {
                    // Handle MagnetBogie
                    MagnetBogie magnetBogie = hit.GetComponent<MagnetBogie>();
                    magnetBogie?.HandleDrop();
                }
            }
            
            // Apply explosion force
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        
        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Add score
        if (gameManager != null)
        {
            gameManager.AddScore(200); // Higher score for special enemy
        }
        
        Destroy(gameObject);
    }
    
    void HandleDrop()
    {
        // Only vulnerable during vulnerable state
        if (currentState != MagnetBogieState.Vulnerable)
            return;
        
        float randomValue = Random.value;
        
        // 50% chance to start falling when shot during vulnerable state
        if (randomValue < 0.5f && !isFalling)
        {
            isFalling = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.linearVelocity = transform.forward * moveSpeed * 2f;
            }
            return;
        }
        
        // Drop items if not falling
        if (!isFalling)
        {
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
            // Only take damage during vulnerable state
            if (currentState == MagnetBogieState.Vulnerable)
            {
                HandleDrop();
                Destroy(collision.gameObject);
            }
            else if (currentState == MagnetBogieState.Attracting)
            {
                // Try to absorb the bullet instead of taking damage
                if (absorbedBullets.Count < maxAbsorbedBullets)
                {
                    AbsorbBullet(collision.gameObject);
                }
                else
                {
                    // If can't absorb more, just destroy the bullet
                    Destroy(collision.gameObject);
                }
            }
            else
            {
                // During cooldown, just destroy bullets
                Destroy(collision.gameObject);
            }
        }
        else if (isFalling)
        {
            if (collision.gameObject.CompareTag("Bullet"))
            {
                Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
                Destroy(collision.gameObject);
            }
            // Explode on any collision while falling
            TriggerExplosion();
        }
    }
    
    // Visual debug for magnet range
    void OnDrawGizmosSelected()
    {
        // Draw magnet range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
        
        // Draw absorption radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, absorptionRadius);
    }
}
