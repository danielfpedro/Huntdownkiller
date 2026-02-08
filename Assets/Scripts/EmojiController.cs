using UnityEngine;
using System.Collections;

public class EmojiController : MonoBehaviour
{
    [Tooltip("The TextMeshPro component for displaying temporary messages.")]
    public TMPro.TextMeshPro displayText;

    [Tooltip("Offset position for the display text relative to this object's transform.")]
    public Vector3 textOffset = new Vector3(0, 1, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        displayText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Displays a temporary message on the TextMeshPro component and disables it after the specified duration.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="duration">How long to show the text before disabling it.</param>
    public void ShowText(string text, float duration)
    {
        if (displayText == null) return;

        displayText.text = text;
        displayText.gameObject.SetActive(true);
        displayText.transform.localPosition = textOffset;

        StartCoroutine(HideTextAfter(duration));
    }

    private IEnumerator HideTextAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (displayText != null)
        {
            displayText.gameObject.SetActive(false);
        }
    }
}
