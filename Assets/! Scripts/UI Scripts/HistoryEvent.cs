using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HistoryEvent : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI eventText;
    [SerializeField] public Button eventButton;

    public void SetHistory(string historyText, System.Action onButtonClick = null)
    {
        eventText.text = historyText;

        // Ensure the button's click event is set
        eventButton.onClick.RemoveAllListeners();
        if (onButtonClick != null)
        {
            eventButton.onClick.AddListener(() => onButtonClick.Invoke());
        }

        // Force layout rebuild to adjust size
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void OnDestroy()
    {
        eventButton.onClick.RemoveAllListeners();
    }
}
