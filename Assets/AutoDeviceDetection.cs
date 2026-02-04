// Assets/AutoDeviceDetection.cs
using UnityEngine;

// Automatically detects device type and configures touch input accordingly
public class AutoDeviceDetection : MonoBehaviour
{
    [Header("Auto-Detection Settings")]
    public bool enableAutoDetection = true;
    public bool overrideMobileDetection = false;
    public bool forceMobileMode = false;
    
    [Header("References")]
    public PlaneController planeController;
    
    [Header("Device-Specific Settings")]
    [Range(0.5f, 2.0f)]
    public float mobileTouchSensitivity = 1.0f;
    [Range(0.5f, 2.0f)]
    public float laptopTouchSensitivity = 0.8f;
    
    void Start()
    {
        // Find plane controller if not assigned
        if (planeController == null)
        {
            planeController = FindObjectOfType<PlaneController>();
        }
        
        if (enableAutoDetection && planeController != null)
        {
            DetectAndConfigureDevice();
        }
    }
    
    void DetectAndConfigureDevice()
    {
        bool isMobileDevice = false;
        
        if (overrideMobileDetection)
        {
            isMobileDevice = forceMobileMode;
            Debug.Log($"Device detection overridden: {(isMobileDevice ? "Mobile" : "Laptop")} mode forced");
        }
        else
        {
            // Detect device type based on platform and input capabilities
            isMobileDevice = DetectMobileDevice();
            Debug.Log($"Auto-detected device type: {(isMobileDevice ? "Mobile" : "Laptop")}");
        }
        
        // Configure plane controller
        planeController.SetMobileMode(isMobileDevice);
        
        // Set appropriate touch sensitivity
        if (isMobileDevice)
        {
            planeController.touchSensitivity = mobileTouchSensitivity;
        }
        else
        {
            planeController.touchSensitivity = laptopTouchSensitivity;
        }
        
        // Log configuration
        Debug.Log($"Touch sensitivity set to: {planeController.touchSensitivity}");
        Debug.Log($"Multi-touch enabled: {planeController.enableMultiTouch}");
    }
    
    bool DetectMobileDevice()
    {
        // Check Unity's built-in platform detection
        RuntimePlatform platform = Application.platform;
        
        switch (platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                return true;
                
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                // For desktop platforms, check if touch is supported
                return Input.touchSupported && HasMobileCharacteristics();
                
            default:
                // For other platforms, assume mobile if touch is supported
                return Input.touchSupported;
        }
    }
    
    bool HasMobileCharacteristics()
    {
        // Additional checks to distinguish between mobile devices and laptops with touchscreens
        
        // Check screen size and DPI
        float screenDiagonal = GetScreenDiagonalInches();
        float dpi = Screen.dpi;
        
        // Mobile devices typically have:
        // - Smaller screens (3-7 inches diagonal)
        // - Higher DPI (200+ typically)
        // - Touch as primary input
        
        bool smallScreen = screenDiagonal < 8.0f;
        bool highDPI = dpi > 200f;
        bool touchPrimary = !Input.mousePresent || Input.touchSupported;
        
        // Score-based detection
        int mobileScore = 0;
        if (smallScreen) mobileScore += 2;
        if (highDPI) mobileScore += 1;
        if (touchPrimary) mobileScore += 1;
        
        Debug.Log($"Device characteristics - Screen: {screenDiagonal:F1}\" DPI: {dpi:F0} Touch: {Input.touchSupported} Mouse: {Input.mousePresent}");
        Debug.Log($"Mobile score: {mobileScore}/4");
        
        return mobileScore >= 3;
    }
    
    float GetScreenDiagonalInches()
    {
        // Calculate screen diagonal in inches
        float dpi = Screen.dpi;
        if (dpi <= 0) dpi = 96f; // Fallback DPI
        
        float widthInches = Screen.width / dpi;
        float heightInches = Screen.height / dpi;
        
        return Mathf.Sqrt(widthInches * widthInches + heightInches * heightInches);
    }
    
    // Public method to manually trigger detection
    public void RedetectDevice()
    {
        if (planeController != null)
        {
            DetectAndConfigureDevice();
        }
    }
    
    // Public method to override detection
    public void SetDeviceMode(bool isMobile)
    {
        overrideMobileDetection = true;
        forceMobileMode = isMobile;
        
        if (planeController != null)
        {
            DetectAndConfigureDevice();
        }
    }
    
    // Get current device info as string
    public string GetDeviceInfo()
    {
        float diagonal = GetScreenDiagonalInches();
        return $"Platform: {Application.platform}\n" +
               $"Screen: {Screen.width}x{Screen.height} ({diagonal:F1}\")\n" +
               $"DPI: {Screen.dpi}\n" +
               $"Touch Supported: {Input.touchSupported}\n" +
               $"Mouse Present: {Input.mousePresent}\n" +
               $"Current Mode: {(planeController?.IsMobileMode() == true ? "Mobile" : "Laptop")}";
    }
    
    void OnGUI()
    {
        // Debug overlay (only in development builds)
        if (Debug.isDebugBuild)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Device Detection Debug");
            GUILayout.Label(GetDeviceInfo());
            
            if (GUILayout.Button("Redetect Device"))
            {
                RedetectDevice();
            }
            
            if (GUILayout.Button("Force Mobile Mode"))
            {
                SetDeviceMode(true);
            }
            
            if (GUILayout.Button("Force Laptop Mode"))
            {
                SetDeviceMode(false);
            }
            
            GUILayout.EndArea();
        }
    }
}
