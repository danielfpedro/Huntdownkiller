using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EmojiManager : MonoBehaviour
{
    public static EmojiManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Prefab containing validation components (e.g. TextMeshProUGUI)")]
    public GameObject emojiPrefab;
    [Tooltip("Default vertical offset above target")]
    public Vector3 worldOffset = Vector3.up * 2f;
    [Tooltip("Reference to canvas (will search for 'Canvas' if null)")]
    RectTransform canvasRect;

    // State per target
    private Dictionary<Transform, TargetEmojiState> targetStates = new Dictionary<Transform, TargetEmojiState>();
    private List<Transform> cleanupList = new List<Transform>();

    private class EmojiRequest
    {
        public string text;
        public float duration;
    }

    private class TargetEmojiState
    {
        public Queue<EmojiRequest> requestQueue = new Queue<EmojiRequest>();
        public GameObject currentInstance;
        public RectTransform currentRect;
        public TextMeshProUGUI currentText;
        public Coroutine activeCoroutine;
        public bool isShowing;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (canvasRect == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
                canvasRect = canvasObj.GetComponent<RectTransform>();
        }
    }

    void LateUpdate()
    {
        // Update positions
        if (Camera.main == null) return;
        
        cleanupList.Clear();

        foreach (var kvp in targetStates)
        {
            Transform target = kvp.Key;
            TargetEmojiState state = kvp.Value;

            if (target == null)
            {
                cleanupList.Add(target); // Target was destroyed
                continue;
            }

            // Only update position if showing
            if (state.isShowing && state.currentInstance != null && state.currentInstance.activeSelf)
            {
                Vector3 targetPos = target.position + worldOffset;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPos);
                
                // If using screen space overlay, screenPos is fine. 
                // If using WorldSpace canvas, this logic might need adjustment, but assuming ScreenSpace Overlay/Camera for UI.
                state.currentRect.position = screenPos;
            }
        }

        // Clean up destroyed targets
        foreach (var deadTarget in cleanupList)
        {
            TargetEmojiState state = targetStates[deadTarget];
            if (state.currentInstance != null)
            {
                Destroy(state.currentInstance);
            }
            if (state.activeCoroutine != null)
            {
                StopCoroutine(state.activeCoroutine);
            }
            targetStates.Remove(deadTarget);
        }
    }

    /// <summary>
    /// Enqueues an emoji to be shown on the target.
    /// </summary>
    public void ShowEmoji(Transform target, string text, float duration = 2f)
    {
        Debug.Log($"EmojiManager: ShowEmoji called for {target?.name} with text '{text}'");

        if (target == null)
        {
            Debug.LogWarning("EmojiManager: Target is null!");
            return;
        }

        if (!targetStates.ContainsKey(target))
        {
            targetStates[target] = new TargetEmojiState();
        }

        TargetEmojiState state = targetStates[target];
        EmojiRequest request = new EmojiRequest { text = text, duration = duration };

        if (state.isShowing)
        {
            Debug.Log("EmojiManager: Already showing emoji, queueing request.");
            // Already showing something, enqueue
            state.requestQueue.Enqueue(request);
        }
        else
        {
            Debug.Log("EmojiManager: Showing immediately.");
            // Show immediately
            DisplayRequest(target, state, request);
        }
    }

    private void DisplayRequest(Transform target, TargetEmojiState state, EmojiRequest request)
    {
        state.isShowing = true;

        // Create instance if needed (one per target, reused)
        if (state.currentInstance == null)
        {
            if (emojiPrefab == null || canvasRect == null)
            {
                Debug.LogError("EmojiManager: Missing prefab or canvas!");
                // Clear state to avoid lock
                state.isShowing = false;
                return;
            }

            state.currentInstance = Instantiate(emojiPrefab, canvasRect);
            state.currentRect = state.currentInstance.GetComponent<RectTransform>();
            
            // Try getting TMP component
            state.currentText = state.currentInstance.GetComponent<TextMeshProUGUI>();
            if (state.currentText == null)
                state.currentText = state.currentInstance.GetComponentInChildren<TextMeshProUGUI>();

            if (state.currentText == null)
            {
                 Debug.LogError($"EmojiManager: No TextMeshProUGUI found on prefab or children! Prefab name: {emojiPrefab.name}");
            }
        }

        // Setup textual content
        if (state.currentText != null)
        {
            state.currentText.text = request.text;
        }

        state.currentInstance.SetActive(true);
        Debug.Log($"EmojiManager: Displaying '{request.text}' on instance {state.currentInstance.name}");
        state.activeCoroutine = StartCoroutine(HandleEmojiLifecycle(target, state, request.duration));
    }

    private IEnumerator HandleEmojiLifecycle(Transform target, TargetEmojiState state, float duration)
    {
        yield return new WaitForSeconds(duration);

        // Hide current
        if (state.currentInstance != null)
        {
            state.currentInstance.SetActive(false);
        }
        
        state.isShowing = false;
        state.activeCoroutine = null;

        // Check queue for next item
        if (state.requestQueue.Count > 0)
        {
            EmojiRequest nextRequest = state.requestQueue.Dequeue();
            DisplayRequest(target, state, nextRequest);
        }
    }
}
