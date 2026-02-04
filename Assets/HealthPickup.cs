// Assets/HealthPickup.cs
using UnityEngine;


public class HealthPickup : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        // Destroy after 10 seconds if not picked up
        Destroy(gameObject, 10f);
    }
    
    void Update()
    {
        // Rotate the pickup
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Make it float up and down
        Vector3 tempPos = startPosition;
        tempPos.y += Mathf.Sin(Time.time * Mathf.PI * floatFrequency) * floatAmplitude;
        transform.position = tempPos;
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if player plane touched the pickup
        PlaneController plane = other.GetComponent<PlaneController>();
        if (plane != null)
        {
            // Add health to the plane
            plane.AddHealth();
            // Destroy the pickup
            Destroy(gameObject);
        }
    }
}