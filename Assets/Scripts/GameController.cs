using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Frame Rate Settings")]
    [Tooltip("Target frame rate. Set to -1 for unlimited, 0 for platform default")]
    public int targetFrameRate = 60;

    [Tooltip("Enable VSync (0 = disabled, 1 = enabled, 2 = half refresh rate)")]
    [Range(0, 2)]
    public int vSyncCount = 0;

    [Tooltip("Force unlock mobile 30 FPS cap (recommended for mobile)")]
    public bool unlockMobileFPSCap = true;

    [Header("Platform Specific")]
    [Tooltip("Custom frame rate for mobile devices")]
    public int mobileTargetFrameRate = 120;

    [Tooltip("Custom frame rate for PC/WebGL")]
    public int pcTargetFrameRate = 144;

    void Awake()
    {
        ApplyFrameRateSettings();
    }

    void ApplyFrameRateSettings()
    {
        // Set VSync
        QualitySettings.vSyncCount = vSyncCount;

        // Determine target frame rate based on platform
        int finalTargetFrameRate = targetFrameRate;

        if (Application.isMobilePlatform)
        {
            if (unlockMobileFPSCap)
            {
                // Unlock mobile FPS cap by disabling VSync and setting high target
                QualitySettings.vSyncCount = 0;
                finalTargetFrameRate = mobileTargetFrameRate;
                Debug.Log("Mobile FPS cap unlocked. Target: " + finalTargetFrameRate + " FPS");
            }
            else
            {
                finalTargetFrameRate = Mathf.Min(targetFrameRate, 30); // Mobile default cap
            }
        }
        else
        {
            // PC/WebGL
            finalTargetFrameRate = pcTargetFrameRate;
        }

        // Apply target frame rate
        if (finalTargetFrameRate > 0)
        {
            Application.targetFrameRate = finalTargetFrameRate;
            Debug.Log("Target frame rate set to: " + finalTargetFrameRate + " FPS");
        }
        else if (finalTargetFrameRate == -1)
        {
            Application.targetFrameRate = -1;
            Debug.Log("Frame rate set to unlimited");
        }
        else
        {
            // Use platform default
            Application.targetFrameRate = -1;
            Debug.Log("Using platform default frame rate");
        }

        Debug.Log("VSync count: " + QualitySettings.vSyncCount);
    }

    // Public methods to change settings at runtime
    public void SetTargetFrameRate(int fps)
    {
        targetFrameRate = fps;
        ApplyFrameRateSettings();
    }

    public void SetVSync(int count)
    {
        vSyncCount = Mathf.Clamp(count, 0, 2);
        ApplyFrameRateSettings();
    }

    public void UnlockMobileFPS(bool unlock)
    {
        unlockMobileFPSCap = unlock;
        ApplyFrameRateSettings();
    }
}
