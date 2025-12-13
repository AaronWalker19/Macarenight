using UnityEngine;

public class CameraLightScript : MonoBehaviour
{
    public static CameraLightScript Instance;

    public Camera cameralight;
    public Camera cameraMain;

    public AudioListener audioListenerLight;
    public AudioListener audioListenerMain;

    void Awake()
    {
        // setup singleton-like access so other scripts can call PlayLighttriffic()
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple instances of CameraLightScript detected. Using the first one.");
        }
    }

    void Start()
    {
        // Ensure cameralight is disabled at start
        if (cameralight != null)
            cameralight.enabled = false;

        if (audioListenerLight != null)
            audioListenerLight.enabled = false;
        
        // Ensure main camera is enabled at start
        if (cameraMain != null)
            cameraMain.enabled = true;

        if (audioListenerMain != null)
            audioListenerMain.enabled = true;
    }

    public void ActiveCamera(bool isActive)
    {
        // Toggle cameras using enabled instead of SetActive to avoid GameObject hierarchy issues
        if (cameralight != null)
            cameralight.enabled = isActive;

        if (audioListenerLight != null)
            audioListenerLight.enabled = isActive;
        
        if (cameraMain != null)
            cameraMain.enabled = !isActive;
        
        if (audioListenerMain != null)
            audioListenerMain.enabled = !isActive;
    }
    
}
