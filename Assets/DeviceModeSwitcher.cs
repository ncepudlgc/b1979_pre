// Assets/DeviceModeSwitcher.cs
using UnityEngine;
using UnityEngine.UI;

// Simple UI component to switch between mobile and laptop input modes
public class DeviceModeSwitcher : MonoBehaviour
{
    [Header("UI References")]
    public Button mobileButton;
    public Button laptopButton;
    public Text statusText;
    
    [Header("Plane Reference")]
    public PlaneController planeController;
    
    // Colors for button states
    private Color activeColor = Color.green;
    private Color inactiveColor = Color.white;
    
    void Start()
    {
        // Set up button listeners
        if (mobileButton != null)
        {
            mobileButton.onClick.AddListener(() => SetMobileMode(true));
        }
        
        if (laptopButton != null)
        {
            laptopButton.onClick.AddListener(() => SetMobileMode(false));
        }
        
        // Find plane controller if not assigned
        if (planeController == null)
        {
            planeController = FindObjectOfType<PlaneController>();
        }
        
        // Update UI to reflect current state
        UpdateUI();
    }
    
    void Update()
    {
        // Update status text with current touch info
        if (statusText != null && planeController != null)
        {
            string mode = planeController.IsMobileMode() ? "Mobile" : "Laptop";
            int touchCount = planeController.GetActiveTouchCount();
            statusText.text = $"Mode: {mode}\nActive Touches: {touchCount}";
        }
    }
    
    public void SetMobileMode(bool isMobile)
    {
        if (planeController != null)
        {
            planeController.SetMobileMode(isMobile);
            UpdateUI();
            
            Debug.Log($"Switched to {(isMobile ? "Mobile" : "Laptop")} mode");
        }
    }
    
    private void UpdateUI()
    {
        if (planeController == null) return;
        
        bool isMobile = planeController.IsMobileMode();
        
        // Update button colors
        if (mobileButton != null)
        {
            ColorBlock colors = mobileButton.colors;
            colors.normalColor = isMobile ? activeColor : inactiveColor;
            mobileButton.colors = colors;
        }
        
        if (laptopButton != null)
        {
            ColorBlock colors = laptopButton.colors;
            colors.normalColor = !isMobile ? activeColor : inactiveColor;
            laptopButton.colors = colors;
        }
    }
    
    // Public method to toggle between modes (useful for testing)
    public void ToggleMode()
    {
        if (planeController != null)
        {
            SetMobileMode(!planeController.IsMobileMode());
        }
    }
}
