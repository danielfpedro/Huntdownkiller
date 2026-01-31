using UnityEngine;
using TMPro;

public class HitIndicator : MonoBehaviour
{
    public TMP_Text textMesh;

    public void Initialize(string text)
    {
        textMesh.text = text;
        // Start a timer or animation to return to pool
        Invoke("ReturnToPool", 2f); // Adjust duration as needed
    }

    private void ReturnToPool()
    {
        HitIndicatorManager.Instance.ReturnIndicator(gameObject);
    }
}