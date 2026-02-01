using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Which corner to display the FPS counter")]
    public TextAnchor anchor = TextAnchor.UpperRight;
    
    [Tooltip("Offset from the corner in pixels")]
    public Vector2 offset = new Vector2(10, 10);
    
    [Tooltip("Font size of the FPS text")]
    public int fontSize = 20;
    
    [Tooltip("Color of the FPS text")]
    public Color textColor = Color.white;
    
    [Header("FPS Calculation")]
    [Tooltip("How often to update the FPS display (in seconds)")]
    public float updateInterval = 0.5f;
    
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private float fps = 0.0f;
    private string display = "";
    
    void Start()
    {
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            fps = accum / frames;
            display = string.Format("{0:F2} FPS", fps);
            
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }
    
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        style.alignment = anchor;
        
        Vector2 screenPos = GetScreenPosition(anchor, offset);
        
        GUI.Label(new Rect(screenPos.x, screenPos.y, 200, 50), display, style);
    }
    
    private Vector2 GetScreenPosition(TextAnchor anchor, Vector2 offset)
    {
        float x = 0, y = 0;
        
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                x = offset.x;
                y = offset.y;
                break;
            case TextAnchor.UpperCenter:
                x = Screen.width / 2 - 100 + offset.x;
                y = offset.y;
                break;
            case TextAnchor.UpperRight:
                x = Screen.width - 200 - offset.x;
                y = offset.y;
                break;
            case TextAnchor.MiddleLeft:
                x = offset.x;
                y = Screen.height / 2 - 25 + offset.y;
                break;
            case TextAnchor.MiddleCenter:
                x = Screen.width / 2 - 100 + offset.x;
                y = Screen.height / 2 - 25 + offset.y;
                break;
            case TextAnchor.MiddleRight:
                x = Screen.width - 200 - offset.x;
                y = Screen.height / 2 - 25 + offset.y;
                break;
            case TextAnchor.LowerLeft:
                x = offset.x;
                y = Screen.height - 50 - offset.y;
                break;
            case TextAnchor.LowerCenter:
                x = Screen.width / 2 - 100 + offset.x;
                y = Screen.height - 50 - offset.y;
                break;
            case TextAnchor.LowerRight:
                x = Screen.width - 200 - offset.x;
                y = Screen.height - 50 - offset.y;
                break;
        }
        
        return new Vector2(x, y);
    }
}
