using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicTextBackground : MonoBehaviour
{
    [SerializeField] private RectTransform background; // The Image (background)
    [SerializeField] private TextMeshProUGUI text; // The text inside

    [SerializeField] private Vector2 padding = new Vector2(20f, 10f); // Extra padding for readability

    private void Update()
    {
        if (text != null && background != null)
        {
            Vector2 textSize = text.GetPreferredValues(); // Get text size
            background.sizeDelta = textSize + padding; // Add padding
        }
    }
}
