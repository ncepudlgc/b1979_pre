// Assets/GameManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;  // Add this at the top for TextMeshProUGUI


// Manages game state, spawning, and scoring
public class GameManager : MonoBehaviour
{
    // Reference to the player plane
    public PlaneController playerPlane;
    // Prefab references
    public GameObject turretPrefab;
    public GameObject bogiePrefab;
    public GameObject magnetBogiePrefab;
    public GameObject bunkerPrefab;
    public GameObject groundPlane;
    public GameObject playerPlanePrefab; // Add reference to player plane prefab
    public GameObject healthPrefab; // Add health prefab reference
    public GameObject healthDisplay;
    public GameObject bombDisplay;
    public GameObject scoreDisplay;
    public GameObject menuButton;
    public GameObject menuPanel;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI bombText;
    private TextMeshProUGUI scoreText;
    // Spawn area settings - now based on ground plane bounds
    private float groundSpawnMinZ;
    private float groundSpawnMaxZ;
    private float groundSpawnMinX;
    private float groundSpawnMaxX;
    private float bogieSpawnMinZ;
    private float bogieSpawnMaxZ;
    private float bogieSpawnMinX;
    private float bogieSpawnMaxX;
    private float bogieSpawnHeight = 20f; // Increased height to prevent ground clipping
    public Vector3 startingPosition = new Vector3(0, 20, 0);
    public Quaternion startingRotation = Quaternion.identity;
    
    // Game state variables
    public int score = 0;
    public int lives = 3;
    
    // Spawn timers and rates
    private float turretSpawnTimer = 0f;
    private float bogieSpawnTimer = 0f;
    private float magnetBogieSpawnTimer = 0f;
    private float bunkerSpawnTimer = 0f;
    public float turretSpawnRate = 10f;
    public float bogieSpawnRate = 5f;
    public float magnetBogieSpawnRate = 20f;
    public float bunkerSpawnRate = 15f;
    
    // Initial spawn counts
    public int initialTurretCount = 10;
    public int initialBogieCount = 5;
    public int initialMagnetBogieCount = 2;
    public int initialBunkerCount = 8;
    
    // Spawn area settings
    private float spawnAreaWidth = 50f;  // Width of spawn area
    private float spawnAreaAhead = 500f; // How far ahead to spawn
    private float spawnAreaBehind = 0f; // How far behind to spawn
    
    // Fixed spacing settings
    private float bunkerSpacing = 10f; // Fixed space between bunkers
    private float turretSpacing = 10f; // Fixed space between turrets
    private float groupSpacing = 40f; // Fixed space between groups
    
    // Object bounds
    private Bounds bunkerBounds;
    private Bounds turretBounds;
    
    // Lists to track objects
    private List<GameObject> bunkers = new List<GameObject>();
    private List<GameObject> turrets = new List<GameObject>();
    
    // Grid system for tracking occupied positions
    private Dictionary<Vector2Int, bool> occupiedPositions = new Dictionary<Vector2Int, bool>();
    private float gridCellSize = 5f; // Size of each grid cell
    
    // Public methods to access spawn limits
    public float GetGroundSpawnMinX() { return groundSpawnMinX; }
    public float GetGroundSpawnMaxX() { return groundSpawnMaxX; }
    public float GetGroundSpawnMinZ() { return groundSpawnMinZ; }
    public float GetGroundSpawnMaxZ() { return groundSpawnMaxZ; }

    // Called to initialize the game
    void Start()
    {
        // Calculate object bounds
        if (bunkerPrefab != null)
        {
            Collider bunkerCollider = bunkerPrefab.GetComponent<Collider>();
            if (bunkerCollider != null)
            {
                bunkerBounds = bunkerCollider.bounds;
            }
        }

        if (turretPrefab != null)
        {
            Collider turretCollider = turretPrefab.GetComponent<Collider>();
            if (turretCollider != null)
            {
                turretBounds = turretCollider.bounds;
            }
        }

        // Calculate spawn bounds based on ground plane
        if (groundPlane != null)
        {
            Renderer groundRenderer = groundPlane.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                Bounds groundBounds = groundRenderer.bounds;
                groundSpawnMinX = groundBounds.min.x;
                groundSpawnMaxX = groundBounds.max.x;
                groundSpawnMinZ = groundBounds.min.z;
                groundSpawnMaxZ = groundBounds.max.z;
            }
        }

        // Clear occupied positions
        occupiedPositions.Clear();

        // Spawn initial turrets and bunkers on the ground
        for (int i = 0; i < initialTurretCount; i++)
        {
            SpawnTurret();
        }

        for (int i = 0; i < initialBunkerCount; i++)
        {
            SpawnBunker();
        }

        // Spawn initial bogies in the air
        for (int i = 0; i < initialBogieCount; i++)
        {
            SpawnBogie();
        }

        // Spawn initial magnet bogies in the air
        for (int i = 0; i < initialMagnetBogieCount; i++)
        {
            SpawnMagnetBogie();
        }

        // Initialize player at starting position
        if (playerPlane != null)
        {
            playerPlane.transform.position = startingPosition;
        }
        
        // Initialize UI text components (add this at the end of Start)
        if (healthDisplay != null) healthText = healthDisplay.GetComponent<TextMeshProUGUI>();
        if (bombDisplay != null) bombText = bombDisplay.GetComponent<TextMeshProUGUI>();
        if (scoreDisplay != null) scoreText = scoreDisplay.GetComponent<TextMeshProUGUI>();
        
        // Initial UI update
        UpdateDisplays();
    }

    // Called every frame to update game state
    void Update()
    {
        if (playerPlane == null)
            return;

        // Handle spawning new enemies
        turretSpawnTimer += Time.deltaTime;
        bogieSpawnTimer += Time.deltaTime;
        magnetBogieSpawnTimer += Time.deltaTime;
        bunkerSpawnTimer += Time.deltaTime;

        if (turretSpawnTimer >= turretSpawnRate)
        {
            SpawnTurret();
            turretSpawnTimer = 0f;
        }

        if (bogieSpawnTimer >= bogieSpawnRate)
        {
            SpawnBogie();
            bogieSpawnTimer = 0f;
        }

        if (magnetBogieSpawnTimer >= magnetBogieSpawnRate)
        {
            SpawnMagnetBogie();
            magnetBogieSpawnTimer = 0f;
        }

        if (bunkerSpawnTimer >= bunkerSpawnRate)
        {
            SpawnBunker();
            bunkerSpawnTimer = 0f;
        }

        // Check win/lose conditions
        if (lives <= 0)
        {
            // Game over logic
            Debug.Log("Game Over!");
            // Restart the game or show game over screen
            // For now, just reset lives
            lives = 3;
            score = 0;
        }
        UpdateDisplays();
    }
    
    public void UpdateDisplays()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + playerPlane.currentHealth + "/" + playerPlane.maxHealth;  // Update lives display
        }
        else
        {
            Debug.LogWarning("Health display TextMeshProUGUI component is missing.");
        }
        
        if (bombText != null && playerPlane != null)
        {
            bombText.text = "Bombs: " + playerPlane.currentBombs + "/" + playerPlane.maxBombs;  // Access currentBombs from PlaneController
        }
        else
        {
            Debug.LogWarning("Bomb display TextMeshProUGUI or playerPlane is missing.");
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";  // Update score display
        }
        else
        {
            Debug.LogWarning("Score display TextMeshProUGUI component is missing.");
        }
    }
    
    // Helper method to check for intersections
    private void CheckIntersections()
    {
        // Check each turret against each bunker
        for (int i = turrets.Count - 1; i >= 0; i--)
        {
            if (turrets[i] == null) continue;

            Collider turretCollider = turrets[i].GetComponent<Collider>();
            if (turretCollider == null) continue;

            bool shouldDestroy = false;

            foreach (GameObject bunker in bunkers)
            {
                if (bunker == null) continue;

                Collider bunkerCollider = bunker.GetComponent<Collider>();
                if (bunkerCollider == null) continue;

                if (turretCollider.bounds.Intersects(bunkerCollider.bounds))
                {
                    shouldDestroy = true;
                    break;
                }
            }

            if (shouldDestroy)
            {
                Destroy(turrets[i]);
                turrets.RemoveAt(i);
            }
        }
    }
    
    // Helper method to convert world position to grid position
    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / gridCellSize),
            Mathf.RoundToInt(worldPos.z / gridCellSize)
        );
    }

    // Helper method to check if a position is valid for spawning
    private bool IsValidSpawnPosition(Vector3 position, float objectSize)
    {
        // Check if position is within ground plane bounds with a margin
        float margin = objectSize * 0.8f; // Increased margin to ensure objects are fully on the ground
        if (position.x < groundSpawnMinX + margin || position.x > groundSpawnMaxX - margin ||
            position.z < groundSpawnMinZ + margin || position.z > groundSpawnMaxZ - margin)
        {
            return false;
        }

        // Check surrounding grid cells for occupation
        Vector2Int gridPos = WorldToGridPosition(position);
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int checkPos = new Vector2Int(gridPos.x + x, gridPos.y + z);
                if (occupiedPositions.ContainsKey(checkPos) && occupiedPositions[checkPos])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Helper method to mark positions as occupied
    private void MarkPositionsOccupied(Vector3 position, float objectSize)
    {
        Vector2Int gridPos = WorldToGridPosition(position);
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int pos = new Vector2Int(gridPos.x + x, gridPos.y + z);
                occupiedPositions[pos] = true;
            }
        }
    }

    // Spawns a turret line at a random position on the ground
    void SpawnTurret()
    {
        if (playerPlane == null || turretPrefab == null) return;
        
        // Calculate random position on ground relative to player
        float randomX = Random.Range(groundSpawnMinX + turretBounds.size.x, groundSpawnMaxX - turretBounds.size.x);
        float randomZ = playerPlane.transform.position.z + Random.Range(spawnAreaBehind, spawnAreaAhead);
        Vector3 basePos = new Vector3(randomX, groundPlane.transform.position.y + groundPlane.transform.localScale.y/2, randomZ);
        
        // Randomly choose between X-axis and Z-axis line
        bool isXAxis = Random.value > 0.5f;
        int lineLength = Random.Range(3, 6); // Random line length between 3-5 turrets
        
        // Check if the entire line can be spawned
        bool canSpawnLine = true;
        for (int i = 0; i < lineLength; i++)
        {
            Vector3 checkPos = basePos;
            if (isXAxis)
            {
                checkPos.x += (i - (lineLength-1)/2f) * turretSpacing;
            }
            else
            {
                checkPos.z += (i - (lineLength-1)/2f) * turretSpacing;
            }
            
            if (!IsValidSpawnPosition(checkPos, turretBounds.size.x))
            {
                canSpawnLine = false;
                break;
            }
        }
        
        if (!canSpawnLine) return;
        
        // Spawn the line of turrets
        for (int i = 0; i < lineLength; i++)
        {
            Vector3 turretPosition = basePos;
            if (isXAxis)
            {
                turretPosition.x += (i - (lineLength-1)/2f) * turretSpacing;
            }
            else
            {
                turretPosition.z += (i - (lineLength-1)/2f) * turretSpacing;
            }
            
            // Instantiate turret
            GameObject turret = Instantiate(turretPrefab, turretPosition, Quaternion.identity);
            turrets.Add(turret);
            
            // Mark position as occupied
            MarkPositionsOccupied(turretPosition, turretBounds.size.x);
            
            // Set up turret references
            Turret turretScript = turret.GetComponent<Turret>();
            if (turretScript != null)
            {
                turretScript.playerPlane = playerPlane;
            }
        }
    }
    
    // Spawns a bunker cluster at a random position on the ground
    void SpawnBunker()
    {
        if (playerPlane == null || bunkerPrefab == null) return;
        
        // Calculate object size for margin
        float objectSize = bunkerBounds.size.x > bunkerBounds.size.z ? bunkerBounds.size.x : bunkerBounds.size.z;
        float margin = objectSize * 0.8f;
        
        // Calculate random position on ground relative to player
        float randomX = Random.Range(groundSpawnMinX + margin, groundSpawnMaxX - margin);
        float randomZ = playerPlane.transform.position.z + Random.Range(spawnAreaBehind, spawnAreaAhead);
        Vector3 basePos = new Vector3(randomX, groundPlane.transform.position.y + groundPlane.transform.localScale.y/2, randomZ);
        
        // Randomly choose between 2x2 and 3x3 cluster
        int clusterSize = Random.value > 0.5f ? 2 : 3;
        
        // Check if the entire cluster can be spawned
        bool canSpawnCluster = true;
        for (int x = 0; x < clusterSize; x++)
        {
            for (int z = 0; z < clusterSize; z++)
            {
                Vector3 checkPos = basePos;
                checkPos.x += (x - (clusterSize-1)/2f) * bunkerSpacing;
                checkPos.z += (z - (clusterSize-1)/2f) * bunkerSpacing;
                
                if (!IsValidSpawnPosition(checkPos, bunkerBounds.size.x))
                {
                    canSpawnCluster = false;
                    break;
                }
            }
            if (!canSpawnCluster) break;
        }
        
        if (!canSpawnCluster) return;
        
        // Spawn bunkers in a grid pattern
        for (int x = 0; x < clusterSize; x++)
        {
            for (int z = 0; z < clusterSize; z++)
            {
                Vector3 bunkerPosition = basePos;
                bunkerPosition.x += (x - (clusterSize-1)/2f) * bunkerSpacing;
                bunkerPosition.z += (z - (clusterSize-1)/2f) * bunkerSpacing;
                
                // Instantiate bunker
                GameObject bunker = Instantiate(bunkerPrefab, bunkerPosition, Quaternion.identity);
                
                // Check if the bunker's collider is outside the ground boundaries
                Collider bunkerCollider = bunker.GetComponent<Collider>();
                if (bunkerCollider != null)
                {
                    Bounds bunkerBounds = bunkerCollider.bounds;
                    if (bunkerBounds.min.x < groundSpawnMinX || bunkerBounds.max.x > groundSpawnMaxX)
                    {
                        Destroy(bunker);
                        continue;
                    }
                }
                
                bunkers.Add(bunker);
                
                // Mark position as occupied
                MarkPositionsOccupied(bunkerPosition, bunkerBounds.size.x);
            }
        }
    }
    
    // Spawns a bogie in the air
    void SpawnBogie()
    {
        if (playerPlane == null) return;
        
        // Calculate random position in air relative to player
        float randomX = Random.Range(-spawnAreaWidth, spawnAreaWidth);
        float randomZ = playerPlane.transform.position.z + Random.Range(spawnAreaBehind, spawnAreaAhead);
        Vector3 spawnPosition = new Vector3(randomX, bogieSpawnHeight, randomZ);
        
        // Instantiate bogiePrefab at random air position
        GameObject bogie = Instantiate(bogiePrefab, spawnPosition, Quaternion.identity);
        
        // Set up bogie references
        Bogie bogieScript = bogie.GetComponent<Bogie>();
        if (bogieScript != null)
        {
            bogieScript.playerPlane = playerPlane;
        }
    }
    
    // Spawns a magnet bogie in the air
    void SpawnMagnetBogie()
    {
        if (playerPlane == null || magnetBogiePrefab == null) return;
        
        // Calculate random position in air relative to player
        float randomX = Random.Range(-spawnAreaWidth, spawnAreaWidth);
        float randomZ = playerPlane.transform.position.z + Random.Range(spawnAreaBehind, spawnAreaAhead);
        Vector3 spawnPosition = new Vector3(randomX, bogieSpawnHeight, randomZ);
        
        // Instantiate magnetBogiePrefab at random air position
        GameObject magnetBogie = Instantiate(magnetBogiePrefab, spawnPosition, Quaternion.identity);
        
        // Set up magnet bogie references
        MagnetBogie magnetBogieScript = magnetBogie.GetComponent<MagnetBogie>();
        if (magnetBogieScript != null)
        {
            magnetBogieScript.playerPlane = playerPlane;
        }
    }
    
    public void OnPlayerHit()
    {
        // Handle player being hit
        lives--;
    }
    
    // Method to add score
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
    }

    public void GameOver()
    {
        lives = 0; // This will trigger the game over logic in Update
        Debug.Log("Game Over!");
    }
}
