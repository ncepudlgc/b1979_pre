// Assets/PlaneController.cs
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;


// Controls the player airplane. Handles movement, shooting, bombing, and camera control.
public class PlaneController : MonoBehaviour
{
    // Device compatibility settings
    [Header("Device Settings")]
    public bool isMobileDevice = false; // Set to true for mobile devices, false for laptop touchscreen
    public float touchSensitivity = 1.0f; // Adjust touch responsiveness
    public bool enableMultiTouch = true; // Enable multi-touch support
    
    // Reference to the main camera for camera controls
    public Camera mainCamera;
    // Reference to the AirReticle prefab (for aiming guns)
    public Transform airReticle;
    // Reference to the GroundReticle prefab (for bomb targeting)
    public Transform groundReticle;
    // Reference to the shoot button UI
    public GameObject shootButton;
    // Reference to the bomb button UI
    public GameObject bombButton;
    // Prefab for the bomb
    public GameObject bombPrefab;

    // Reticle settings
    public float airReticleDistance = 50f; // Much further distance for better visibility
    public float bombSpawnOffset = 2f; // Distance below plane to spawn bombs
    public float maxPitchAngle = 30f; // Maximum up/down rotation
    public float maxRollAngle = 45f;  // Maximum left/right tilt
    public float turnSpeed = 5f; // How quickly the plane turns

    // Plane movement speed
    public float moveSpeed = 20f;
    // Plane tilt/turn speed
    public float turnSpeedPlane = 5f; // Increased turn speed for more responsive controls
    // Height and area constraints
    public float minHeight = 10f;
    public float maxHeight = 50f;
    public float minX = -50f;
    public float maxX = 50f;
    public GameManager gameManager;
    
    // Rigidbody reference
    private Rigidbody rb;
    
    // Bullet prefab for shooting
    [SerializeField] private GameObject bulletPrefab;
    // Bullet settings
    public float bulletSpeed = 100f;
    public float shootCooldown = 0.1f; // Time between shots
    private float lastShootTime;
    private bool isShooting = false;
    
    // Bomb management
    public int maxBombs = 5;
    public int currentBombs = 5;
    public float bombReplenishRate = 10f; // Time in seconds to replenish one bomb
    private float lastBombReplenishTime;
    public float bombCooldown = 1f; // Cooldown between bomb drops
    private float lastBombTime;
    public float bulletLifeTime = 0.5f;
    // Health system
    public int maxHealth = 4;
    public int currentHealth;
    // Boost and brake system
    public float defaultMoveSpeed = 20f;
    public float maxBoostSpeed = 35f;
    public float minBrakeSpeed = 10f;
    public float boostBrakeRate = 5f; // How quickly speed changes when boosting/braking
    public float maxBoostBrakeResource = 100f;
    private float currentBoostBrakeResource = 100f;
    public float resourceDepletionRate = 20f; // Resource used per second
    public float resourceRegenerationRate = 10f; // Resource regenerated per second
    private bool isBoosting = false;
    private bool isBraking = false;
    
    // Touch input tracking
    private int trajectoryTouchId = -1; // ID of the touch controlling trajectory
    private Vector2 lastTrajectoryTouchPosition;
    private Dictionary<int, bool> activeTouches = new Dictionary<int, bool>();
    private bool isTrajectoryTouchActive = false;

    // Called to initialize the plane
    void Start()
    {
        // Initialize references and set up camera
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        }

        // Initialize bomb system
        currentBombs = maxBombs;
        lastBombReplenishTime = Time.time;
        lastShootTime = Time.time;
        lastBombTime = Time.time;
        // Initialize health
        currentHealth = maxHealth;
        // Initialize boost/brake system
        moveSpeed = defaultMoveSpeed;
        currentBoostBrakeResource = maxBoostBrakeResource;
        
        // Initialize reticle positions
        UpdateGroundReticle();
    }
    
    // Called every frame to update movement and input
    void Update()
    {
        // Handle input based on device type
        if (isMobileDevice)
        {
            HandleMobileInput();
        }
        else
        {
            HandleLaptopTouchInput();
        }

        // Handle boost and brake
        HandleBoostAndBrake();

        // Replenish bombs over time
        if (currentBombs < maxBombs && Time.time - lastBombReplenishTime >= bombReplenishRate)
        {
            currentBombs++;
            lastBombReplenishTime = Time.time;
        }
        
        // Move plane forward using Rigidbody
        if (rb != null)
        {
            // Set forward velocity
            Vector3 targetVelocity = transform.forward * moveSpeed;
            
            // Handle boundary constraints
            Vector3 currentPos = transform.position;
            Vector3 newVelocity = targetVelocity;
            
            // Height constraints
            if (currentPos.y >= maxHeight && targetVelocity.y > 0)
            {
                newVelocity.y = 0;
            }
            else if (currentPos.y <= minHeight && targetVelocity.y < 0)
            {
                newVelocity.y = 0;
            }
            
            // X position constraints
            if (currentPos.x >= maxX && targetVelocity.x > 0)
            {
                newVelocity.x = 0;
            }
            else if (currentPos.x <= minX && targetVelocity.x < 0)
            {
                newVelocity.x = 0;
            }
            
            // Apply the velocity
            rb.linearVelocity = newVelocity;
            
            // Force position within bounds if somehow outside
            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(transform.position.x, minX, maxX),
                Mathf.Clamp(transform.position.y, minHeight, maxHeight),
                transform.position.z
            );
            if (clampedPosition != transform.position)
            {
                transform.position = clampedPosition;
            }
        }
        
        // Update air reticle position
        UpdateAirReticle();
        
        // Update ground reticle
        UpdateGroundReticle();
        
        // Update camera
        UpdateCamera();

        // Handle continuous shooting
        if (isShooting && Time.time - lastShootTime >= shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    // New method to handle boost and brake functionality
    private void HandleBoostAndBrake()
    {
        // Resource regeneration when not boosting or braking
        if (!isBoosting && !isBraking && currentBoostBrakeResource < maxBoostBrakeResource)
        {
            currentBoostBrakeResource += resourceRegenerationRate * Time.deltaTime;
            currentBoostBrakeResource = Mathf.Min(currentBoostBrakeResource, maxBoostBrakeResource);
        }
        
        // Handle boosting
        if (isBoosting && currentBoostBrakeResource > 0)
        {
            // Increase speed
            moveSpeed = Mathf.Min(moveSpeed + boostBrakeRate * Time.deltaTime, maxBoostSpeed);
            
            // Deplete resource
            currentBoostBrakeResource -= resourceDepletionRate * Time.deltaTime;
            if (currentBoostBrakeResource <= 0)
            {
                currentBoostBrakeResource = 0;
                isBoosting = false;
            }
        }
        // Handle braking
        else if (isBraking && currentBoostBrakeResource > 0)
        {
            // Decrease speed
            moveSpeed = Mathf.Max(moveSpeed - boostBrakeRate * Time.deltaTime, minBrakeSpeed);
            
            // Deplete resource
            currentBoostBrakeResource -= resourceDepletionRate * Time.deltaTime;
            if (currentBoostBrakeResource <= 0)
            {
                currentBoostBrakeResource = 0;
                isBraking = false;
            }
        }
        // Return to default speed when not boosting or braking
        else if (!isBoosting && !isBraking)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, defaultMoveSpeed, Time.deltaTime * 2);
        }
    }
    
    // Methods for UI button events
    public void OnBoostButtonDown()
    {
        isBoosting = true;
        isBraking = false;
    }
    
    public void OnBoostButtonUp()
    {
        isBoosting = false;
    }
    
    public void OnBrakeButtonDown()
    {
        isBraking = true;
        isBoosting = false;
    }
    
    public void OnBrakeButtonUp()
    {
        isBraking = false;
    }
    
    // Method to add health (called by health pickup)
    public void AddHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
        }
    }

    // Called when the shoot button is pressed (for UI OnClick events)
    public void OnShootButton()
    {
        if (Time.time - lastShootTime >= shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    // Called when the shoot button is held down
    public void OnShootButtonDown()
    {
        isShooting = true;
    }

    // Called when the shoot button is released
    public void OnShootButtonUp()
    {
        isShooting = false;
    }

    private void Shoot()
    {
        // Get the plane's collider
        Collider planeCollider = GetComponent<Collider>();
        if (planeCollider == null) return;

        // Calculate spawn position at the front of the plane's collider
        Vector3 spawnPosition = transform.position + transform.forward * (planeCollider.bounds.extents.z + 0.1f);
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, transform.rotation);
        
        // Set bullet velocity
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = transform.forward * bulletSpeed;
        }
        
        // Make bullet ignore collisions with the player plane
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (bulletCollider != null && planeCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, planeCollider);
        }
        
        // Destroy bullet after 5 seconds if it doesn't hit anything
        Destroy(bullet, bulletLifeTime);
    }
    
    // Called when the bomb button is pressed
    public void OnBombButton()
    {
        if (currentBombs <= 0 || Time.time - lastBombTime < bombCooldown) return;
        
        // Drop bomb below the plane
        Vector3 spawnPosition = transform.position - transform.up * bombSpawnOffset;
        GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        
        // Calculate initial velocity to hit ground reticle
        Vector3 targetPosition = groundReticle.position;
        float height = spawnPosition.y - targetPosition.y;
        float gravity = Physics.gravity.magnitude;
        
        // Calculate time to reach target based on height and gravity
        float timeToTarget = Mathf.Sqrt(2 * height / gravity);
        
        // Calculate required velocity to reach target
        Vector3 horizontalDirection = (targetPosition - spawnPosition);
        horizontalDirection.y = 0;
        float horizontalDistance = horizontalDirection.magnitude;
        
        // Calculate required velocities
        float horizontalVelocity = horizontalDistance / timeToTarget;
        float verticalVelocity = Mathf.Sqrt(2 * gravity * height);
        
        // Combine velocities
        Vector3 velocity = horizontalDirection.normalized * horizontalVelocity;
        velocity.y = -verticalVelocity; // Negative because we want to go down
        
        // Add forward momentum from the plane
        velocity += transform.forward * (moveSpeed * 1.5f);
        
        // Add force to the bomb
        Rigidbody bombRb = bomb.GetComponent<Rigidbody>();
        if (bombRb != null)
        {
            bombRb.linearVelocity = velocity;
            bomb.tag = "Bomb";
        }
        
        // Decrease bomb count and update last bomb time
        currentBombs--;
        lastBombTime = Time.time;
    }
    
    // Method to add a bomb (called by ammo pickup)
    public void AddBomb()
    {
        if (currentBombs < maxBombs)
        {
            currentBombs++;
        }
    }
    
    // Updates the plane's rotation based on touch input
    void UpdatePlaneRotation(Vector2 touchPosition)
    {
        // Convert screen position to normalized coordinates (-1 to 1)
        Vector2 normalizedPosition = new Vector2(
            (touchPosition.x / Screen.width) * 2 - 1,
            (touchPosition.y / Screen.height) * 2 - 1
        );
        
        // Apply touch sensitivity
        normalizedPosition *= touchSensitivity;
        
        // Clamp to prevent over-rotation
        normalizedPosition.x = Mathf.Clamp(normalizedPosition.x, -1f, 1f);
        normalizedPosition.y = Mathf.Clamp(normalizedPosition.y, -1f, 1f);
        
        // Calculate target rotation based on touch position
        float targetPitch = -normalizedPosition.y * maxPitchAngle; // Negative because screen Y is inverted
        float targetRoll = normalizedPosition.x * maxRollAngle;
        
        // Get current rotation
        Vector3 eulerAngles = transform.eulerAngles;
        
        // Convert current angles to -180 to 180 range
        float currentPitch = eulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
        
        float currentRoll = eulerAngles.y;
        if (currentRoll > 180f) currentRoll -= 360f;
        
        // Adjust turn speed based on device type
        float adjustedTurnSpeed = isMobileDevice ? turnSpeed * 1.2f : turnSpeed;
        
        // Smoothly interpolate to target rotation
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, adjustedTurnSpeed * Time.deltaTime);
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, adjustedTurnSpeed * Time.deltaTime);
        
        // Apply new rotation
        eulerAngles.x = currentPitch;
        eulerAngles.y = currentRoll;
        eulerAngles.z = 0f; // Keep Z rotation at 0
        
        transform.eulerAngles = eulerAngles;
    }
    
    // Public method to switch device modes at runtime
    public void SetMobileMode(bool mobile)
    {
        isMobileDevice = mobile;
        
        // Reset touch tracking when switching modes
        trajectoryTouchId = -1;
        isTrajectoryTouchActive = false;
        activeTouches.Clear();
        
        // Adjust settings based on device type
        if (mobile)
        {
            touchSensitivity = Mathf.Max(touchSensitivity, 0.8f); // Ensure minimum sensitivity for mobile
            enableMultiTouch = true;
        }
        else
        {
            touchSensitivity = Mathf.Min(touchSensitivity, 1.2f); // Limit sensitivity for laptop
            enableMultiTouch = false;
        }
    }
    
    // Get current device mode
    public bool IsMobileMode()
    {
        return isMobileDevice;
    }
    
    // Get active touch count (useful for debugging)
    public int GetActiveTouchCount()
    {
        return activeTouches.Count;
    }
    
    // Updates the AirReticle to stay in front of the plane
    void UpdateAirReticle()
    {
        if (airReticle != null)
        {
            // Position reticle directly in front of plane
            Vector3 reticlePosition = transform.position + transform.forward * airReticleDistance;
            airReticle.position = reticlePosition;
            
            // Make reticle face camera at all times
            airReticle.rotation = Quaternion.Euler(270f, 0f, 0f); // -90 degrees on X-axis to face camera
        }
    }
    
    // Updates the GroundReticle position based on bomb trajectory
    void UpdateGroundReticle()
    {
        if (groundReticle != null)
        {
            // Calculate where bomb will land based on current height, speed, and position
            float height = transform.position.y;
            float gravity = Physics.gravity.magnitude;
            
            // Calculate time to reach ground
            float timeToGround = Mathf.Sqrt(2 * height / gravity);
            
            // Calculate forward distance based on plane's speed and time to ground
            float forwardDistance = moveSpeed * timeToGround;
            
            // Calculate where the bomb will land
            Vector3 landingPosition = transform.position + transform.forward * forwardDistance;
            landingPosition.y = 0.1f; // Just above ground
            
            // Update ground reticle position
            groundReticle.position = landingPosition;
        }
    }
    
    // Updates the camera to follow the plane
    void UpdateCamera()
    {
        if (mainCamera != null)
        {
            // Position camera behind and above the plane
            Vector3 targetPosition = transform.position - transform.forward * 10f + Vector3.up * 5f;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, 5f * Time.deltaTime);
            
            // Look at the plane
            mainCamera.transform.LookAt(transform.position + transform.forward * 10f);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bogie"))
        {
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
            GameObject.Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            // Reduce health instead of instant respawn
            currentHealth--;
            
            // Destroy the projectile
            Destroy(collision.gameObject);
            
            if (currentHealth <= 0)
            {
                // Reset health
                currentHealth = maxHealth;
                
                // Simple respawn - just reset everything
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                transform.position = gameManager.startingPosition;
                transform.rotation = gameManager.startingRotation;

                currentBombs = maxBombs;
                currentBoostBrakeResource = maxBoostBrakeResource;
            }
        }
    }
    
    // Handle multi-touch input for mobile devices
    private void HandleMobileInput()
    {
        // Process all active touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    HandleTouchMoved(touch);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandleTouchEnded(touch);
                    break;
            }
        }
        
        // Clean up finished touches
        CleanupFinishedTouches();
    }
    
    // Handle laptop touchscreen input (simpler, single-touch focused)
    private void HandleLaptopTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // Ignore touches on UI buttons
            if (!IsPointerOverUIObject(touch.position))
            {
                UpdatePlaneRotation(touch.position);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            // For testing in editor
            if (!IsPointerOverUIObject(Input.mousePosition))
            {
                UpdatePlaneRotation(Input.mousePosition);
            }
        }
    }
    
    // Handle when a touch begins
    private void HandleTouchBegan(Touch touch)
    {
        // Check if touch is over UI
        if (IsPointerOverUIObject(touch.position))
        {
            // Mark this touch as UI-related
            activeTouches[touch.fingerId] = true;
            return;
        }
        
        // If no trajectory touch is active, assign this touch to trajectory control
        if (!isTrajectoryTouchActive)
        {
            trajectoryTouchId = touch.fingerId;
            isTrajectoryTouchActive = true;
            lastTrajectoryTouchPosition = touch.position;
            activeTouches[touch.fingerId] = false; // Not a UI touch
        }
        else
        {
            // Additional touches can be used for other controls if needed
            activeTouches[touch.fingerId] = false;
        }
    }
    
    // Handle when a touch moves
    private void HandleTouchMoved(Touch touch)
    {
        // Only process trajectory touch
        if (touch.fingerId == trajectoryTouchId && isTrajectoryTouchActive)
        {
            // Check if still not over UI (in case user dragged from game area to UI)
            if (!IsPointerOverUIObject(touch.position))
            {
                UpdatePlaneRotation(touch.position);
                lastTrajectoryTouchPosition = touch.position;
            }
        }
    }
    
    // Handle when a touch ends
    private void HandleTouchEnded(Touch touch)
    {
        // If this was the trajectory touch, reset it
        if (touch.fingerId == trajectoryTouchId)
        {
            trajectoryTouchId = -1;
            isTrajectoryTouchActive = false;
        }
        
        // Remove from active touches
        if (activeTouches.ContainsKey(touch.fingerId))
        {
            activeTouches.Remove(touch.fingerId);
        }
    }
    
    // Clean up any touches that are no longer active
    private void CleanupFinishedTouches()
    {
        List<int> touchesToRemove = new List<int>();
        
        foreach (var kvp in activeTouches)
        {
            bool touchStillActive = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == kvp.Key)
                {
                    touchStillActive = true;
                    break;
                }
            }
            
            if (!touchStillActive)
            {
                touchesToRemove.Add(kvp.Key);
            }
        }
        
        foreach (int touchId in touchesToRemove)
        {
            activeTouches.Remove(touchId);
            if (touchId == trajectoryTouchId)
            {
                trajectoryTouchId = -1;
                isTrajectoryTouchActive = false;
            }
        }
    }
    
    private bool IsPointerOverUIObject(Vector2 position)
    {
        // For mobile devices, use the touch-specific method
        if (isMobileDevice)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = position;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
        else
        {
            // For laptop/editor, use the simpler method
            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject();
        }
    }
}
